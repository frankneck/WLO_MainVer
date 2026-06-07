using Unity.Collections;
using Unity.Entities;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial struct DestoryItemEntityWithoutCharacterOwnerSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (ghostCharacterTarget, entity) in SystemAPI
            .Query<ItemOwner>()
            .WithEntityAccess()
            .WithNone<DestroyEntityTag>())
        {
            if (!SystemAPI.Exists(ghostCharacterTarget.Entity))
            {
                ecb.AddComponent<DestroyEntityTag>(entity);
            }
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}