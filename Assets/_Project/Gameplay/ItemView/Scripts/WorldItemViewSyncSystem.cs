using Unity.Entities;
using Unity.Transforms;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
public partial struct WorldItemViewSyncSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (viewTransform, owner) in SystemAPI
            .Query<RefRW<ItemViewTransform>, ItemViewOwner>()
            .WithAll<WorldViewTag>())
        {
            if (!SystemAPI.HasComponent<LocalTransform>(owner.Entity))
            {
                UnityEngine.Debug.Log("[WorldItemViewSyncSystem] Current Item owner hasn't transform");
                continue;
            }

            var ownerTransform = SystemAPI.GetComponent<LocalTransform>(owner.Entity);
            viewTransform.ValueRW.Position = ownerTransform.Position;
        }
    }
}