using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public static class PlayerInput
{
    public static InputAction PlayersList;
    public static InputAction AppMenu;
    public static InputAction GameConsole;
    public static InputAction Inventory;
    public static InputAction ShiftFastSlot; 

    public static event UnityAction InputsInitialized;

    public static void Initialize()
    {
#if UNITY_EDITOR
        Debug.Log("Player Input Initialize");
#endif
        var playerActionMap = InputSystem.actions.FindActionMap("Player");
        var inventoryActionMap = InputSystem.actions.FindActionMap("Inventory");

        // PLAYER (IN GAME)

        PlayersList = playerActionMap.FindAction("PlayersList");
        AppMenu = playerActionMap.FindAction("AppMenu");
        GameConsole = playerActionMap.FindAction("Console");
        Inventory = playerActionMap.FindAction("Inventory");
        
        // INVENTORY 

        ShiftFastSlot = inventoryActionMap.FindAction("ShiftFastSlot");
        
        InputsInitialized?.Invoke();
    }
}
