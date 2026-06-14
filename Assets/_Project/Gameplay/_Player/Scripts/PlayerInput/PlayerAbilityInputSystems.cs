// using Unity.Entities;
// using Unity.NetCode;

// [UpdateInGroup(typeof(GhostInputSystemGroup))]
// public partial class AbilityInputSystem : SystemBase
// {
//     private InputSystem_Actions _inputActions;

//     protected override void OnCreate()
//     {
//         _inputActions = new InputSystem_Actions();
//     }

//     protected override void OnStartRunning()
//     {
//         _inputActions.Enable(); 
//     }

//     protected override void OnStopRunning()
//     {
//         _inputActions.Disable();
//     }

//     protected override void OnUpdate()
//     {
//         var newAbilityInput = new ChooseStuffInput();

//         if (_inputActions.Player.ChooseFirstItem.WasPressedThisFrame())
//         {
//             newAbilityInput.ChooseFirstItem.Set();
//         }

//         if (_inputActions.Player.ChooseSecondItem.WasPressedThisFrame())
//         {
//             newAbilityInput.ChooseSecondItem.Set();
//         }

//         if (_inputActions.Player.ChooseThirdItem.WasPressedThisFrame())
//         {
//             newAbilityInput.ChooseThirdItem.Set();
//         }

//         if (_inputActions.Player.ChooseForthItem.WasPressedThisFrame())
//         {
//             newAbilityInput.ChooseFourthItem.Set();
//         }

//         foreach (var (abilityInput, entity) in SystemAPI.Query<RefRW<ChooseStuffInput>>().WithEntityAccess())
//         {
//             //  need to know what character to do

//             if (SystemAPI.HasComponent<ClientCharacterState>(entity))
//             {
//                 var state = SystemAPI.GetComponentRO<ClientCharacterState>(entity);

//                 // if he's in Menu
//                 if (state.ValueRO.Value == CharacterState.InMenu)
//                 {
//                     newAbilityInput = new ChooseStuffInput();
//                 }
//                 // else we handle input
//             }
            
//             abilityInput.ValueRW = newAbilityInput;
//         }
//     }
// }

// [UpdateInGroup(typeof(GhostInputSystemGroup))]
// public partial class AttackInputSystem : SystemBase
// {
//     private InputSystem_Actions _inputActions;

//     protected override void OnCreate()
//     {
//         _inputActions = new InputSystem_Actions();
//     }

//     protected override void OnStartRunning()
//     {
//         _inputActions.Enable(); 
//     }

//     protected override void OnStopRunning()
//     {
//         _inputActions.Disable();
//     }

//     protected override void OnUpdate()
//     {
//         var newAttackInput = new WeaponControl();

//         // AttackInput
//         if (_inputActions.Player.Attack.IsPressed())
//         {
//             newAttackInput.ShootPressed.Set();
//         }

//         // Shield
//         if (_inputActions.Player.Shield.IsPressed())
//         {
//             newAttackInput.ShieldHeld = true;
//         }
//         else
//         {
//             newAttackInput.ShieldHeld = false;
//         }

//         foreach (var (abilityInput, entity) in SystemAPI.Query<RefRW<WeaponControl>>().WithEntityAccess())
//         {
//             //  need to know what character to do
//             if (SystemAPI.HasComponent<ClientCharacterState>(entity))
//             {
//                 var state = SystemAPI.GetComponentRO<ClientCharacterState>(entity);

//                 if (state.ValueRO.Value != CharacterState.InGame)
//                 {
//                     newAttackInput = new WeaponControl();
//                 }
//             }
            
//             abilityInput.ValueRW = newAttackInput;
//         }
//     }
// }


// Как можно решить?
// Создать отдельный компонент, который прикпреляется к игроку, когда он в инвентаре
// Создать состояние игрока - когда в инвентаре, в меню (можно отслеживать)

// Кастуется спелл, когда в инвентаре
// Решение когда в инвентаре - не кастовать спелл (да ну нах) 