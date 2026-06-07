using Unity.Entities;
using Unity.NetCode;

[GhostComponent(PrefabType = GhostPrefabType.AllPredicted)]
public struct ItemControl : IComponentData
{
    [GhostField] public bool MainActionPressed;
    [GhostField] public bool ShieldHeld;
}