using Unity.NetCode;

[GhostComponent(PrefabType = GhostPrefabType.AllPredicted)]
public struct AttackInput : IInputComponentData
{
    [GhostField] public InputEvent SkillShotAttack;
    [GhostField] public InputEvent AoeAttack;
}