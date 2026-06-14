using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;

public class UIController : MonoBehaviour
{
    private Inventory m_Inventory;
    private InventoryView m_InventoryView;

    private HudScreen m_HudView;
    private AppMenuWindow m_AppMenuUI;
    private AppStatsUI m_AppStatsUI;
    private PlayerListWindow m_PlayerListUI;
    private WorldSpaceUIController m_WorldSpaceUIController;
    private GameConsoleWindow m_GameConsole;
    private TeamSelectionScreen m_TeamSelection;
    private PendingStartMatchScreen m_PendingStartMatchView;
    private FinishingMatchScreen m_FinishingMatchScreen;

    public static UIController Instance;
    private Dictionary<WindowType, VisualElement> windows = new();
    
    private GameScreen m_CurrentGameScreen;
    private GameUIWindowsState m_CurrentGameUIState;
    private CursorMode m_CursorMode;

    private GameMode m_CurrentGameMode = GameMode.None;
    private EntityCommandBuffer m_CommandBuffer;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void Init(
        Inventory inventory,
        HudScreen hudView,
        AppMenuWindow appMenuUI,
        AppStatsUI appStatsUI,
        PlayerListWindow playerListUI,
        WorldSpaceUIController worldSpaceUIController,
        GameConsoleWindow gameConsole,
        TeamSelectionScreen teamSelection,
        PendingStartMatchScreen pendingStart,
        FinishingMatchScreen finishingMatchScreen
    )
    {
        UnityEngine.Debug.Log("[GameUI] UI controller has initialized.");

        // current = WindowType.None;

        m_Inventory = inventory;
        m_InventoryView = inventory.GetInventoryView();

        m_HudView = hudView;
        m_AppMenuUI = appMenuUI;
        m_AppStatsUI = appStatsUI;
        m_GameConsole = gameConsole;
        m_PlayerListUI = playerListUI;
        m_TeamSelection = teamSelection;
        m_WorldSpaceUIController = worldSpaceUIController;
        m_PendingStartMatchView = pendingStart;
        m_FinishingMatchScreen = finishingMatchScreen;
    }

    /// <summary>
    /// It used to update current game mode match on local client 
    /// </summary>
    public void UpdateCurrentGameMode(GameMode gameMode)
    {
        if (m_CurrentGameMode == gameMode)
            return;

        m_CurrentGameMode = gameMode;
    }

    #region Public API
    
    public void OnPlayerListPressed() => OpenPlayerList();
    public void OnPlayerListReleased() => ClosePlayerList();       
    public void OnInventoryPressed() => ToggleInventory();
    public void OnAppMenuPressed() => ToggleMenu();
    
    public void OnStartMatchCalled()
    {
        OpenGameScreen(GameScreen.StartingMatchScreen);
    }

    public void OnActiveMatchCalled()
    {
        OpenGameScreen(GameScreen.GameplayHudScreen);
    } 

    public void OnFinishMatchCalled()
    {
        OpenGameScreen(GameScreen.FinishingMatchScreen);
    } 

    public void OnTeamSelectionCalled() => OpenGameScreen(GameScreen.TeamSelectionScreen);
    public void OnStartRoundCalled() => OpenGameScreen(GameScreen.StartRoundScreen);
    public void OnFinishRoundCalled() => OpenGameScreen(GameScreen.EndRoundScreen);

    public void OnPlayerRespawningCalled() => OpenGameScreen(GameScreen.RespawningScreen);
    public void OnPlayerDeadCalled() => OpenGameScreen(GameScreen.DeadScreen);

    public void OnSpectating() => OpenGameScreen(GameScreen.Spectating);

    public void OnDisconnectCalled()
    {
        m_CurrentGameScreen = GameScreen.None;
        m_CurrentGameUIState = GameUIWindowsState.None;
    }

    // TODO: It used in first person character. Change it in future
    public bool HasStateFlag(GameUIWindowsState target) => m_CurrentGameUIState.HasFlag(target);

    #endregion 

    private void Update()
    {
        RenderUI();
    }

#region  GameScreen UI

    // Sets game screens in depends of game mode 
    private void SetGameScreensOnMode()
    {
        m_PendingStartMatchView.SetOnMode(m_CurrentGameMode);
        m_HudView.SetOnMode(m_CurrentGameMode);
        m_FinishingMatchScreen.SetOnMode(m_CurrentGameMode);
    }

    // sets current game screen
    private void OpenGameScreen(GameScreen target)
    {
        if (m_CurrentGameScreen == target)
            return;
        
        ResetGameUIWindowsState();

        m_CurrentGameScreen = target;
    }

#endregion

#region Gameplay UI
    
    private void OpenPlayerList()
    {
        TryOpen(GameUIWindowsState.PlayerList);
    }

    private void ClosePlayerList()
    {
        TryClose(GameUIWindowsState.PlayerList);
    }

    private void ToggleMenu()
    {
        // if inventory is open and ECS pressed -> close inventory
        if (m_CurrentGameUIState.HasFlag(GameUIWindowsState.Inventory))
        {
            m_CurrentGameUIState &= ~GameUIWindowsState.Inventory;

            SendCloseInventoryRequest();
        }
        else
        {
            // if not -> simple toggling
            Toggle(GameUIWindowsState.GameMenu);
        }

    }

    private void ToggleInventory()
    {
        if (m_CurrentGameUIState.HasFlag(GameUIWindowsState.Inventory))
        {
            SendCloseInventoryRequest();
        }
        else 
        {
            SendOpenInventoryRequest();
        }

        Toggle(GameUIWindowsState.Inventory);
    }

    private void Toggle(GameUIWindowsState target)
    {
        if (m_CurrentGameUIState.HasFlag(target))
        {
            TryClose(target);
        }
        else
        {
            TryOpen(target);
        }
    }

    private void TryOpen(GameUIWindowsState target)
    {
        if (!CanOpen(target))
            return;
       
        m_CurrentGameUIState |= target;  
    }

    private void TryClose(GameUIWindowsState target)
    {
        if (!CanClose(target))
            return;

        m_CurrentGameUIState &= ~target;
    }

#endregion

    private bool CanClose(GameUIWindowsState target)
    {
        if (!m_CurrentGameUIState.HasFlag(target))
            return false;

        return true;
    }

    // Checking that window can be opened
    private bool CanOpen(GameUIWindowsState target)
    {
        switch (m_CurrentGameScreen)
        {
            case GameScreen.TeamSelectionScreen :
            case GameScreen.StartingMatchScreen :
                return IsNotGameplayUI(target);
            default:
                break;
        }

        // if inventory is open 
        if ((m_CurrentGameUIState & GameUIWindowsState.Inventory) != 0 && 
            target != GameUIWindowsState.Inventory && 
            target != GameUIWindowsState.GameMenu)
        {
            return false;
        }

        // if menu is open 
        if ((m_CurrentGameUIState & GameUIWindowsState.GameMenu) != 0 &&
            (target != GameUIWindowsState.GameMenu))
        {
            return false;
        }

        // if player list is open
        if ((m_CurrentGameUIState & GameUIWindowsState.PlayerList) != 0 &&
            (target != GameUIWindowsState.PlayerList))
        {
            return false;
        }

        return true;
    }

    private bool IsNotGameplayUI(GameUIWindowsState target)
    {
        return target == GameUIWindowsState.GameMenu;
    }

    /// <summary>
    /// Render UI based Current Game UI State
    /// </summary>
    private void RenderUI()
    {
        ResetGameScreen();

        SetGameScreensOnMode();
        
        UpdateCursorMode();

        switch (m_CurrentGameScreen)
        {
            case GameScreen.TeamSelectionScreen:
                ShowTeamSelectionScreen();
                break;
            case GameScreen.StartingMatchScreen:
                ShowPendingStartMatchScreen();
                break;
            case GameScreen.GameplayHudScreen :
                ShowGameplayHud();
                break;
            case GameScreen.StartRoundScreen :
                ShowStartRoundScreen();
                break;
            case GameScreen.EndRoundScreen:
                ShowEndRoundWindow();
                break;
            case GameScreen.DeadScreen :
                ShowDeathWindow();
                break;
            case GameScreen.Spectating :
                ShowSpectationScreen();
                break;
            case GameScreen.FinishingMatchScreen :
                ShowFinishingMatchScreen();
                break;
            default :
                ResetGameScreen();
                break;
        }

        RenderUIWindow(m_InventoryView, GameUIWindowsState.Inventory);
        RenderUIWindow(m_AppMenuUI, GameUIWindowsState.GameMenu);
        RenderUIWindow(m_PlayerListUI, GameUIWindowsState.PlayerList);
        
        if (m_CurrentGameUIState.HasFlag(GameUIWindowsState.Inventory) ||
            m_CurrentGameUIState.HasFlag(GameUIWindowsState.PlayerList) ||
            m_CurrentGameUIState.HasFlag(GameUIWindowsState.GameMenu))
        {
            m_HudView.Hide();
        }
    }

    private void ShowSpectationScreen()
    {        
        m_HudView.Show();
        m_HudView.MatchHeaderShow();
        m_HudView.HealthbarHide();
        m_HudView.EquipmentHide();
        m_HudView.DeadInfoHide();
        m_HudView.RespawnInfoHide();
        m_HudView.WinnerContainerHide();
        m_HudView.HelpSectionHide();
        m_HudView.CrosshairHide();

        m_WorldSpaceUIController.Hide();
    }

    private void ShowDeathWindow()
    {        
        m_HudView.Show();
        m_HudView.MatchHeaderShow();
        m_HudView.HealthbarHide();
        m_HudView.EquipmentHide();
        m_HudView.DeadInfoShow();
        m_HudView.RespawnInfoHide();
        m_HudView.WinnerContainerHide();
        m_HudView.HelpSectionHide();
        m_HudView.CrosshairHide();

        m_WorldSpaceUIController.Hide();
    }

    private void ShowEndRoundWindow()
    {
        m_HudView.Show();
        m_HudView.MatchHeaderShow();
        m_HudView.HealthbarShow();
        m_HudView.EquipmentShow();
        m_HudView.DeadInfoHide();
        m_HudView.RespawnInfoHide();
        m_HudView.WinnerContainerShow();
        m_HudView.HelpSectionHide();
        m_HudView.CrosshairShow();

        m_WorldSpaceUIController.Hide();
    }

    private void ShowStartRoundScreen()
    {
        m_HudView.Show();
        m_HudView.MatchHeaderShow();
        m_HudView.HealthbarShow();
        m_HudView.EquipmentShow();
        m_HudView.DeadInfoHide();
        m_HudView.RespawnInfoHide();
        m_HudView.WinnerContainerHide();
        m_HudView.HelpSectionShow();
        m_HudView.CrosshairShow();

        m_WorldSpaceUIController.Hide();
    }

    private void ShowGameplayHud()
    {
        m_HudView.Show();
        m_HudView.MatchHeaderShow();
        m_HudView.HealthbarShow();
        m_HudView.EquipmentShow();
        m_HudView.DeadInfoHide();
        m_HudView.RespawnInfoHide();
        m_HudView.WinnerContainerHide();
        m_HudView.HelpSectionShow();
        m_HudView.CrosshairShow();

        m_WorldSpaceUIController.Show();
    }

    private void ShowFinishingMatchScreen()
    {
        m_FinishingMatchScreen.Show(); 
    }

    private void ShowTeamSelectionScreen()
    {
        m_TeamSelection.Show();
    }

    private void ShowPendingStartMatchScreen()
    {
        m_PendingStartMatchView.Show(); 
    }

    private void RenderUIWindow(
        IUIView view, 
        GameUIWindowsState targetState
    )
    {
        if (m_CurrentGameUIState.HasFlag(targetState))
        {            
            view.Show();
        }
        else
        {
            view.Hide();
        }
    }

    private void ResetGameScreen()
    {        
        m_HudView.Hide();
        m_TeamSelection.Hide();
        m_PendingStartMatchView.Hide();
        m_FinishingMatchScreen.Hide();
    }

    private void ResetGameUIWindowsState()
    {
        // Game windows
        TryClose(GameUIWindowsState.GameMenu);
        TryClose(GameUIWindowsState.PlayerList);
        TryClose(GameUIWindowsState.Inventory);

        // if inventory is open send req to close inventory
        if (m_CurrentGameUIState.HasFlag(GameUIWindowsState.Inventory))
        {
            SendCloseInventoryRequest();
        }
    }

    private void UpdateCursorMode()
    {
        // 1. Меню, Inventory всегда показывать мышь
        if (m_CurrentGameUIState.HasFlag(GameUIWindowsState.GameMenu) || 
            m_CurrentGameUIState.HasFlag(GameUIWindowsState.Inventory))
        {
            ApplyCursor(CursorMode.Visible);
            return;
        }

        // 3. PlayerList — UI
        if (m_CurrentGameUIState.HasFlag(GameUIWindowsState.PlayerList))
        {
            ApplyCursor(CursorMode.Invisible);
            return;
        }

        // 4. Глобальные экраны
        switch (m_CurrentGameScreen)
        {
            case GameScreen.TeamSelectionScreen:
            case GameScreen.StartingMatchScreen:
                ApplyCursor(CursorMode.Visible);
                return;
            
            case GameScreen.GameplayHudScreen:
            case GameScreen.StartRoundScreen:
            case GameScreen.EndRoundScreen:
                ApplyCursor(CursorMode.Invisible);
                return;
            
            default :
                ApplyCursor(CursorMode.Invisible);
                return;
        }
    }
    
    private void ApplyCursor(CursorMode mode)
    {
        m_CursorMode = mode;

        switch (mode)
        {
            case CursorMode.Invisible:
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                break;
            case CursorMode.Visible:
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                break;
        }
    }

    private void SendCloseInventoryRequest()
    {
        if (m_CommandBuffer.IsCreated)
        {
            var request = m_CommandBuffer.CreateEntity();
            m_CommandBuffer.AddComponent<CloseInventoryRequest>(request);
        }
        else
        {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            var request = em.CreateEntity();
            em.AddComponent<CloseInventoryRequest>(request);
        }
    }

    private void SendOpenInventoryRequest()
    {
        if (m_CommandBuffer.IsCreated)
        {
            var request = m_CommandBuffer.CreateEntity();
            m_CommandBuffer.AddComponent<OpenInventoryRequest>(request);
        }
        else
        {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            var request = em.CreateEntity();
            em.AddComponent<OpenInventoryRequest>(request);
        }
    }

    /// <summary>
    /// Set the command buffer for deferred entity changes (used during system updates)
    /// </summary>
    public void SetCommandBuffer(
        ref EntityCommandBuffer ecb
    )
    {
        m_CommandBuffer = ecb;
    }

    /// <summary>
    /// Clear the command buffer after system update completes
    /// </summary>
    public void ClearCommandBuffer()
    {
        m_CommandBuffer = default;
    }
}

[Flags]
public enum GameUIWindowsState : byte
{
    None = 0,
    
    // Gameplay states
    HUD = 1 << 0,
    Inventory = 1 << 1,
    PlayerList = 1 << 2,
    
    // Not gameplay states
    GameMenu = 1 << 3,
}

public enum GameScreen : byte
{
    None = 0,
    TeamSelectionScreen,
    GameplayHudScreen,
    StartingMatchScreen,
    FinishingMatchScreen,
    StartRoundScreen,
    EndRoundScreen, 
    RespawningScreen,
    DeadScreen,
    Spectating
}

public enum CursorMode
{
    Invisible,
    Visible,
    LockedUI
}