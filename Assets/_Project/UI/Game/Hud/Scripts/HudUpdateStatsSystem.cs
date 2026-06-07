using Unity.Entities;
using Unity.NetCode;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
[UpdateInGroup(typeof(PresentationSystemGroup))]
public partial class ManabarUpdateSystem : SystemBase
{
    protected override void OnCreate()
    {
        RequireForUpdate<HudScreen>();
    }

    protected override void OnUpdate()
    {
         var hud = SystemAPI.ManagedAPI.GetSingleton<HudScreen>();
 
        foreach (var (hp, maxHp, currentStuff) in SystemAPI
            .Query<RefRO<CurrentHealth>, 
                RefRO<MaxHealth>, RefRO<ActiveItem>>()
            .WithAll<GhostOwnerIsLocal>())
        {
            var stuff = currentStuff.ValueRO.Entity;

            if (stuff == Entity.Null)
            {
                hud.HideManabar();
            }

            hud.SetHealthbar(hp.ValueRO.Value, maxHp.ValueRO.Value);
            
            if (!SystemAPI.HasComponent<WeaponMaxMana>(stuff) 
                || !SystemAPI.HasComponent<CurrentMana>(stuff))
            {
                continue;
            }
            
            var mana = SystemAPI.GetComponentRO<CurrentMana>(stuff);
            var maxMana = SystemAPI.GetComponentRO<WeaponMaxMana>(stuff);

            hud.SetManabar(mana.ValueRO.Value, maxMana.ValueRO.Value);
            
            if (maxMana.ValueRO.Value - mana.ValueRO.Value > 0.1f)
            {
                hud.ShowManabar();
            }
            else
            {
                hud.HideManabar();
            }
        }
    }
}