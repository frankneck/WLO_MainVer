using Unity.Entities;
using UnityEngine;

public class ClientCurrentObservedObjectAuthoring : MonoBehaviour
{
    class Baker : Baker<ClientCurrentObservedObjectAuthoring>
    {
        public override void Bake(ClientCurrentObservedObjectAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.None);
            AddComponent<ClientCurrentObservedObject>(entity);
        }
    }
}