using Unity.Entities;
using UnityEngine;

public class MenuStateAuthoring : MonoBehaviour
{
    public MenuState StartMenuState;

    class MenuStateBaker : Baker<MenuStateAuthoring>
    {
        public override void Bake(MenuStateAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.None);
            AddComponent(entity, new MenuStateComponent
            {
                State = authoring.StartMenuState        
            });
        }
    }
}

public struct MenuStateComponent : IComponentData
{
    public MenuState State;
}

public enum MenuState
{
    Game,
    Menu
}