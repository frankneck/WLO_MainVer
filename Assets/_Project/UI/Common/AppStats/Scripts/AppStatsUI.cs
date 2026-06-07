using UnityEngine;
using UnityEngine.UIElements;
using Unity.NetCode;
using Unity.Entities;
using Unity.Collections;

[RequireComponent(typeof(UIDocument))]
public class AppStatsUI : MonoBehaviour
{
    private UIDocument document;

    private VisualElement m_Root;
    public VisualElement Root => m_Root;

    private VisualElement appStatsVE;
    private Label fpsL;

    public float FPS_Interval = 1f;
    int frames;
    float fps_timeAccumulator;
    float currentFps;

    public float ping_Interval = 0.25f;
    private float ping_Timer;
    private string clientWorldName = "Client World";
    private World clientWorld;
    private Label pingL;
    private Label lossL;


    public void Init()
    {
        document = GetComponent<UIDocument>();
        m_Root = document.rootVisualElement;

        appStatsVE = m_Root.Query<VisualElement>("AppStats");
        fpsL = appStatsVE.Query<Label>("FPS");
        pingL = appStatsVE.Query<Label>("ping");
        lossL = appStatsVE.Query<Label>("loss");

        clientWorld = GetClientWorld();
    }

    void Update()
    {
        FPSCount();
        PingCount();
    }

    void FPSCount()
    {
        if (fpsL == null)
            return;

        frames++;
        fps_timeAccumulator += Time.deltaTime;

        if (fps_timeAccumulator >= FPS_Interval)
        {
            currentFps = frames / fps_timeAccumulator;
            frames = 0; fps_timeAccumulator = 0f;
            fpsL.text = $"{currentFps:F0} FPS";
        }
    }

    void PingCount()
    {
        if (clientWorld == null) return;

        ping_Timer += Time.unscaledDeltaTime;
        if (ping_Timer < ping_Interval) return;
        ping_Timer = 0f;

        var em = clientWorld.EntityManager;

        using (var query = em.CreateEntityQuery(typeof(NetworkSnapshotAck)))
        {
            var entities = query.ToEntityArray(Allocator.Temp);
            if (entities.Length == 0)
            {
                entities.Dispose();
                return;
            }

            var ackEntity = entities[0];

            entities.Dispose();

            if (!em.HasComponent<NetworkSnapshotAck>(ackEntity)) return;
            var ack = em.GetComponentData<NetworkSnapshotAck>(ackEntity);

            float estimatedMs = ack.EstimatedRTT;
            pingL.text = $"{estimatedMs:F0} PING";
            float loss = (float)ack.SnapshotPacketLoss.CombinedPacketLossPercent*100;
            lossL.text = $"{loss:F0}% LOSS";
            lossL.style.display = loss < 1 ? DisplayStyle.None : DisplayStyle.Flex;
        }
    }

    World GetClientWorld()
    {
        // Попытка найти World по имени
        for (int i = 0; i < World.All.Count; i++)
        {
            var w = World.All[i];
            if (w == null) continue;
            if (w.Name == clientWorldName) return w;
        }
        return null;
    }
}
