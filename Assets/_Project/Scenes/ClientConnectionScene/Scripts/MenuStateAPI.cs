// using Unity.Entities;

// public static class MenuStateAPI
//     {
//         public static void ToggleMenu()
//         {
//             var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            
//             var menuEntity = em.CreateEntityQuery(typeof(MenuStateComponent)).GetSingletonEntity();
//             var currentState = em.GetComponentData<MenuStateComponent>(menuEntity);
            
//             MenuState newState = currentState.Value == MenuState.InMenu ? MenuState.InGame : MenuState.InMenu;
            
//             // Create a new request in order to change a state menu  
//             var evtEntity = em.CreateEntity();
//             em.AddComponentData(evtEntity, new MenuStateRequest { RequestedState = newState });
//         }
//     }