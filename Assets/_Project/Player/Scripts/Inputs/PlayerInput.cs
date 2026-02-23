using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public static class PlayerInput
{
    public static InputAction Move;
    public static InputAction PlayersList;
    public static InputAction AppMenu;
    public static InputAction GameConsole;

    public static event UnityAction InputsInitialized;

    public static void Initialize()
    {
#if UNITY_EDITOR
        Debug.Log("Player Input Initialize");
#endif
        var actionsMap = InputSystem.actions.FindActionMap("Player");
        Move = actionsMap.FindAction("Move");
        PlayersList = actionsMap.FindAction("PlayersList");
        AppMenu = actionsMap.FindAction("AppMenu");
        GameConsole = actionsMap.FindAction("GameConsole");
        InputsInitialized?.Invoke();
    }
}
