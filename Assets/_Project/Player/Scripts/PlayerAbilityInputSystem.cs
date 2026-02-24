using Unity.Entities;
using Unity.NetCode;

[UpdateInGroup(typeof(GhostInputSystemGroup))]
public partial class AbilityInputSystem : SystemBase
{
    private InputSystem_Actions _inputActions;

    protected override void OnCreate()
    {
        _inputActions = new InputSystem_Actions();
    }

    protected override void OnStartRunning()
    {
        _inputActions.Enable(); 
    }

    protected override void OnStopRunning()
    {
        _inputActions.Disable();
    }

    protected override void OnUpdate()
    {
        var newAbilityInput = new AttackInput();

        if (_inputActions.Player.SkillShotAttack.WasPressedThisFrame())
        {
            // InputEvent
            newAbilityInput.SkillShotAttack.Set();
        }

        if (_inputActions.Player.AoeAttack.WasPressedThisFrame())
        {
            // InputEvent
            newAbilityInput.AoeAttack.Set();
        }

        foreach (var abilityInput in SystemAPI.Query<RefRW<AttackInput>>())
        {
            // Assign new value
            abilityInput.ValueRW = newAbilityInput;   
        }
    }
}