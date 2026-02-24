using Unity.Entities;
using Unity.NetCode;

public struct SpellPrefabs : IComponentData
{
    public Entity AoeAbility;
    public Entity SkillShotAbility;
}

public struct SpellCooldown : IComponentData
{
    public float AoeAbility;
    public float SkillShotAbility;
}

[GhostComponent(PrefabType = GhostPrefabType.AllPredicted)]
public struct SpellCooldownTargetTicks : ICommandData
{
    public NetworkTick Tick { get ; set ; }
    public NetworkTick AoeAbility;
    public NetworkTick SkillShotAbility;
}

public struct SpellMoveSpeed : IComponentData
{
    public float Value;
}