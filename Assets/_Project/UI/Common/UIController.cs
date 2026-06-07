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
    private GameUIState m_CurrentGameUIState;
    private CursorMode m_CursorMode;

    private GameMode m_CurrentGameMode = GameMode.None;

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
        m_CurrentGameUIState = GameUIState.None;
    }

    // TODO: It used in first person character. Change it in future
    public bool HasStateFlag(GameUIState target) => m_CurrentGameUIState.HasFlag(target);

    #endregion 

    // private void LateUpdate()
    // {
    //     UnityEngine.Debug.Log($"[UIController] Game UI State:{m_CurrentGameUIState}; GameScreen: {m_CurrentGameScreen}; GameMode {m_CurrentGameMode}");
    //     Render();
    // }

#region  GameScreen UI

    private void SetGameScreensOnMode()
    {
        m_PendingStartMatchView.SetOnMode(m_CurrentGameMode);
        m_HudView.SetOnMode(m_CurrentGameMode);
        m_FinishingMatchScreen.SetOnMode(m_CurrentGameMode);
    }

    private void OpenGameScreen(GameScreen target)
    {
        if (m_CurrentGameScreen == target)
            return;

        m_CurrentGameScreen = target;

        Render();
    }

#endregion

#region Gameplay UI
    
    private void OpenPlayerList()
    {
        TryOpen(GameUIState.PlayerList);
        Render();
    }

    private void ClosePlayerList()
    {
        TryClose(GameUIState.PlayerList);
        Render();
    }

    private void ToggleMenu()
    {
        // if inventory is open and ECS pressed -> close inventory
        if (m_CurrentGameUIState.HasFlag(GameUIState.Inventory))
        {
            m_CurrentGameUIState &= ~GameUIState.Inventory;

            SendCloseInventoryRequest();
            
            Render();
        }
        else
        {
            // if not -> simple toggling
            Toggle(GameUIState.Menu);
        }

    }

    private void ToggleInventory()
    {
        if (m_CurrentGameUIState.HasFlag(GameUIState.Inventory))
        {
            SendCloseInventoryRequest();
        }
        else 
        {
            SendOpenInventoryRequest();
        }

        Toggle(GameUIState.Inventory);
    }

    private void Toggle(GameUIState target)
    {
        if (m_CurrentGameUIState.HasFlag(target))
        {
            TryClose(target);
        }
        else
        {
            TryOpen(target);
        }

        Render();
    }

    private void TryOpen(GameUIState target)
    {
        if (!CanOpen(target))
            return;
       
        m_CurrentGameUIState |= target;  
    }

    private void TryClose(GameUIState target)
    {
        if (!CanClose(target))
            return;

        m_CurrentGameUIState &= ~target;
    }

#endregion

    private bool CanClose(GameUIState target)
    {
        if (!m_CurrentGameUIState.HasFlag(target))
            return false;

        return true;
    }

    /// <summary>
    /// Checking that windw can be opened
    /// </summary>
    private bool CanOpen(GameUIState target)
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
        if ((m_CurrentGameUIState & GameUIState.Inventory) != 0 && 
            target != GameUIState.Inventory && 
            target != GameUIState.Menu)
        {
            return false;
        }

        // if menu is open 
        if ((m_CurrentGameUIState & GameUIState.Menu) != 0 &&
            (target != GameUIState.Menu))
        {
            return false;
        }

        // if player list is open
        if ((m_CurrentGameUIState & GameUIState.PlayerList) != 0 &&
            (target != GameUIState.PlayerList))
        {
            return false;
        }

        return true;
    }

    private bool IsNotGameplayUI(GameUIState target)
    {
        return target == GameUIState.Menu;
    }

    /// <summary>
    /// Render UI based Current Game UI State
    /// </summary>
    private void Render()
    {
        SetGameScreensOnMode();
        UpdateCursor();

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
                HideAll();
                break;
        }

        RenderUIView(m_InventoryView, GameUIState.Inventory);
        RenderUIView(m_AppMenuUI, GameUIState.Menu);
        RenderUIView(m_PlayerListUI, GameUIState.PlayerList);

        if (m_CurrentGameUIState.HasFlag(GameUIState.Inventory) ||
            m_CurrentGameUIState.HasFlag(GameUIState.PlayerList) ||
            m_CurrentGameUIState.HasFlag(GameUIState.Menu))
        {
            m_HudView.Hide();
        }
    }

    private void ShowSpectationScreen()
    {
        HideAll();
        
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
        HideAll();
        
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
        HideAll();

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
        HideAll();

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
        HideAll();

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
        HideAll();
        m_FinishingMatchScreen.Show(); 
    }

    private void ShowTeamSelectionScreen()
    {
        HideAll();
        m_TeamSelection.Show();
    }

    private void ShowPendingStartMatchScreen()
    {
        HideAll();
        m_PendingStartMatchView.Show(); 
    }

    private void RenderUIView(IUIView view, GameUIState targetState)
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

    private void HideAll()
    {
        UnityEngine.Debug.Log("Hide all");

        m_AppMenuUI.Hide();
        m_HudView.Hide();
        m_PlayerListUI.Hide();
        m_Inventory.GetInventoryView().Hide();
        m_TeamSelection.Hide();
        m_PendingStartMatchView.Hide();
        m_FinishingMatchScreen.Hide();
    }

    private void UpdateCursor()
    {
        // 1. Меню всегда UI режим
        if (m_CurrentGameUIState.HasFlag(GameUIState.Menu))
        {
            ApplyCursor(CursorMode.UI);
            return;
        }

        // 2. Inventory тоже UI
        if (m_CurrentGameUIState.HasFlag(GameUIState.Inventory))
        {
            ApplyCursor(CursorMode.UI);
            return;
        }

        // 3. PlayerList — UI
        if (m_CurrentGameUIState.HasFlag(GameUIState.PlayerList))
        {
            ApplyCursor(CursorMode.Gameplay);
            return;
        }

        // 4. Глобальные экраны
        switch (m_CurrentGameScreen)
        {
            case GameScreen.TeamSelectionScreen:
            case GameScreen.StartingMatchScreen:
                ApplyCursor(CursorMode.UI);
                return;
            case GameScreen.GameplayHudScreen:
            case GameScreen.StartRoundScreen:
            case GameScreen.EndRoundScreen:
                ApplyCursor(CursorMode.Gameplay);
                return;
            default :
                ApplyCursor(CursorMode.UI);
                return;
        }
    }
    
    private void ApplyCursor(CursorMode mode)
    {
        m_CursorMode = mode;

        switch (mode)
        {
            case CursorMode.Gameplay:
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                break;
            case CursorMode.UI:
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                break;
            case CursorMode.LockedUI:
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = true;
                break;
        }
    }

    private void SendCloseInventoryRequest()
    {
        var em = World.DefaultGameObjectInjectionWorld.EntityManager;
        var request = em.CreateEntity();
        em.AddComponent<CloseInventoryRequest>(request);
    }

    private void SendOpenInventoryRequest()
    {
        var em = World.DefaultGameObjectInjectionWorld.EntityManager;
        var request = em.CreateEntity();
        em.AddComponent<OpenInventoryRequest>(request);
    }
}

[Flags]
public enum GameUIState : byte
{
    None = 0,
    
    // Gameplay states
    HUD = 1 << 0,
    Inventory = 1 << 1,
    PlayerList = 1 << 2,
    
    // Not gameplay states
    Menu = 1 << 3,
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
    Gameplay,
    UI,
    LockedUI
}