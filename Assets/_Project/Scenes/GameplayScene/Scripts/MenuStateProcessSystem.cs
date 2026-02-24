using UnityEngine;
using Unity.Entities;
using Unity.Collections;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ClientSimulation)]
public partial struct MenuStateProcessSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<MenuStateComponent>();
    }

    public void OnUpdate(ref SystemState state)
    {
        foreach (var menuState in SystemAPI.Query<RefRO<MenuStateComponent>>())
        {
            if (menuState.ValueRO.State == MenuState.Game)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else if (menuState.ValueRO.State == MenuState.Menu)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }
}

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ClientSimulation)]
public partial struct ChangeMenuStateSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<MenuStateComponent>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var menuState = SystemAPI.GetSingletonRW<MenuStateComponent>();
        foreach (var newState in SystemAPI.Query<NewMenuState>())
        {
            if (menuState.ValueRO.State != newState.NewState)
                menuState.ValueRW.State = newState.NewState;
        }
    }
}

public struct NewMenuState : IComponentData
{
    public MenuState NewState;
}