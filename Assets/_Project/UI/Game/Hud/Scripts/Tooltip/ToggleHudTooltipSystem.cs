using Unity.Entities;
using Unity.NetCode;

/// <summary>
/// Updates toggle state by current singleton state on client. It can be simple interacton tooltip 
/// or Pickup tooltip with info about it
/// </summary>
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
[UpdateInGroup(typeof(PresentationSystemGroup))]
public partial class ToggleHudTooltipSystem : SystemBase
{
    protected override void OnCreate()
    {
        RequireForUpdate<LocalCharacterTag>();
        RequireForUpdate<HudScreen>();
    }

    protected override void OnUpdate()
    {
        var view = SystemAPI.ManagedAPI.GetSingleton<HudScreen>();

        foreach (var tooltipState in SystemAPI
            .Query<ClientCurrentObservedObject>()
            .WithChangeFilter<ClientCurrentObservedObject>())
        {
            if (tooltipState.IsCollectable)
            {
                var itemId = SystemAPI.GetComponent<CurrentItemId>(tooltipState.Target).Value; 
                
                var dto = new GameplayDataForHudTooltip
                {
                    Id = itemId     
                };

                view.TogglePickupTooltip(dto, tooltipState.IsVisible);
            }
            else 
                view.ToggleInteractionTooltip(tooltipState.IsVisible);
        }
    }
}