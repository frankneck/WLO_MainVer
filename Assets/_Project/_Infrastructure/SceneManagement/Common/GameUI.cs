using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Starts only on client side 
/// </summary>
public class GameUI : MonoBehaviour
{
    [Header("Configs")]
    [SerializeField] private InventoryConfig m_InventoryConfig;
    [SerializeField] private ItemManagedDatabase m_ItemManagedDatabase;
    
    [Header("Controller")]
    [SerializeField] private UIController m_UIController;
    
    [Header("UI Views")]
    [SerializeField] private Inventory m_Inventory;
    [SerializeField] private HudScreen m_HudView;
    [SerializeField] private AppMenuWindow m_AppMenuWindow;
    [SerializeField] private AppStatsUI m_AppStatsUI;
    [SerializeField] private PlayerListWindow m_PlayerLisWindow;
    [SerializeField] private WorldSpaceUIController m_WorldSpaceUIController;
    [SerializeField] private GameConsoleWindow m_GameConsoleWindow;
    [SerializeField] private TeamSelectionScreen m_TeamSelectionScreen;
    [SerializeField] private PendingStartMatchScreen m_PendingStarMatchScreen;
    [SerializeField] private FinishingMatchScreen m_FinishingMatchScreen;
    
    private void Start()
    {
        BindObjects();
        InitializeObjects();
    }

    private void InitializeObjects()
    {        
        // Data for UI
        m_ItemManagedDatabase.Init();
        
        // Views
        m_Inventory.Init(m_InventoryConfig, m_ItemManagedDatabase);
        m_HudView.Init(m_InventoryConfig, m_ItemManagedDatabase);
        m_TeamSelectionScreen.Init();
        m_AppMenuWindow.Init();
        m_AppStatsUI.Init();
        m_WorldSpaceUIController.Init();
        m_PlayerLisWindow.Init();
        m_PendingStarMatchScreen.Init();
        m_FinishingMatchScreen.Init();
    
        // Controller
        m_UIController.Init(
            inventory: m_Inventory,
            hudView: m_HudView,
            appMenuUI: m_AppMenuWindow,
            appStatsUI: m_AppStatsUI,
            playerListUI: m_PlayerLisWindow,
            worldSpaceUIController: m_WorldSpaceUIController,
            gameConsole: m_GameConsoleWindow,
            teamSelection: m_TeamSelectionScreen,
            pendingStart: m_PendingStarMatchScreen,
            finishingMatchScreen: m_FinishingMatchScreen
        );
    }

    private void BindObjects()
    {
        m_UIController = Instantiate(m_UIController);

        m_ItemManagedDatabase = Instantiate(m_ItemManagedDatabase);
        
        m_Inventory = Instantiate(m_Inventory);
        m_HudView = Instantiate(m_HudView);
        
        m_AppMenuWindow = Instantiate(m_AppMenuWindow);
        m_AppStatsUI = Instantiate(m_AppStatsUI);
        m_WorldSpaceUIController = Instantiate(m_WorldSpaceUIController);
        m_PlayerLisWindow = Instantiate(m_PlayerLisWindow);
        
        m_TeamSelectionScreen = Instantiate(m_TeamSelectionScreen);
        m_PendingStarMatchScreen = Instantiate(m_PendingStarMatchScreen);
        m_FinishingMatchScreen = Instantiate(m_FinishingMatchScreen);
    }
}