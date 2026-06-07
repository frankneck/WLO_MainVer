using Unity.Entities;
using UnityEngine;

public class DestroyOnTimerAuthoring : MonoBehaviour
{
    public float DestroyOnTimer;

    class DestroyOnTimerBaker : Baker<DestroyOnTimerAuthoring>
    {
        public override void Bake(DestroyOnTimerAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.None);
            AddComponent(entity, new DestroyOnTimer { Value = authoring.DestroyOnTimer });
        }
    }
}