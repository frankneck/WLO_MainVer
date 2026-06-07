using System;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine.UIElements;

/// <summary>
/// Stores current UI state
/// </summary>
public struct UIState : IComponentData
{
    public WindowType Window;
}

public class UIView
{
    private VisualElement m_Root;
    private WindowType m_Window;

    public UIView(VisualElement root, WindowType window)
    {
        m_Root = root;
        m_Window = window;
    }
}

// public class UIView : IComponentData
// {
//     public UIWindow Window;
//     public VisualElement Root;
// }

// tag that procedure window is ready
public struct UIInitialized : IComponentData { }

public struct UIInputLockRequest : IComponentData { }


// Defines that player on what step of game. For example, player can be on choice of team or in game
[GhostComponent()]
public struct CurrentPlayerState : IComponentData
{
    [GhostField()] 
    public PlayerState Value;
}

/// <summary>
/// Global local client state that stores data about hoverable entity 
/// </summary>
public struct ClientCurrentObservedObject : IComponentData
{
    public Entity Target;
    public bool IsVisible;
    public bool IsCollectable;
}

// Character state
public enum PlayerState : byte
{
    // Common
    None = 0,
    
    PendingStartMatch,
    Playing,
    FinishingMatch,
    
    Respawning,
    Spectating,
    Dead,

    // Deathmatch
    SelctingTeam,
    PendingStartRound,
    PendingFinishRound,
}

// Mode of UIWindow
public enum WindowType : byte
{
    None = 0,
    MenuWindow = 1,
    InventoryWindow = 2,
    Console = 3,
} 

public enum UIAction : byte
{
    Open = 1,
    Close = 2,
    Toggle = 3
}

/// <summary>
/// Defines what can input player. It depends of current player state
/// </summary>
[GhostComponent()]
public struct InputPermissions : IComponentData
{
    [GhostField()] public InputFlags Value;
}

[Flags]
public enum InputFlags
{
    None = 0,
    Move = 1 << 0,
    Look = 1 << 1,
    Shoot = 1 << 2,
    Inventory = 1 << 3,
    Menu = 1 << 4,
    PlayerList = 1 << 5,
    Interact = 1 << 6
}