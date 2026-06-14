using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;

/// <summary>
/// It used to control the main menu screen. The main menu consists from StartScreen and ConnectionScreen.
/// Main logic is on Connection Screen. 
/// </summary>
[RequireComponent(typeof(UIDocument))]
public class MainMenuController : MonoBehaviour
{
    private UIDocument m_Document;
    
    private VisualElement m_StartScreen;
    private VisualElement m_ConnnectionScreen;
    
    private TextField m_NicknameTextField;
    private TextField m_AddressTextField;
    
    private TextField m_DeathmatchNumberOfRoundsTextField;
    private TextField m_DeathmatchRoundTimeTextField;
    private TextField m_DominationMatchTimeTextField;
    private TextField m_DominationRevivalTimeTextField;
    
    private IntegerField m_PortIntegerField;
    private IntegerField m_DominationMaxScoreIntegerField;
    
    private DropdownField m_ConnectionModeDropdown;
    private DropdownField m_GameModeDropdown;
    private DropdownField m_LevelMapDropdown;
    private DropdownField m_MaxPlayersDropdown;
    
    private Button m_StartButton;
    private Button m_StartGameButton;
    private Button m_CancelButton;
    private Button m_ExitButton;

    private ushort m_PortValue => (ushort)m_PortIntegerField.value;
    private string m_AddressValue => m_AddressTextField.value;
    private FixedString32Bytes m_NicknameValue => (FixedString32Bytes)m_NicknameTextField.value;    
    private int m_LevelValue => m_LevelMapDropdown.index;
    
    private GameMode m_GameModeValue;
    private WorldsMode m_WorldsModeValue;

    private int m_MaxPlayersValue => int.Parse(m_MaxPlayersDropdown.value);
    
    private int m_DeathmatchRoundTimeValue => int.Parse(m_DeathmatchRoundTimeTextField.text);
    private int m_DeathmatchNumberOfRoundsValue => int.Parse(m_DeathmatchNumberOfRoundsTextField.value);
    
    private int m_DominationRevivalTimeValue => int.Parse(m_DominationRevivalTimeTextField.text);
    private int m_DominationMaxScoreValue => m_DominationMaxScoreIntegerField.value;
    private int m_DominationMatchTimeValue => int.Parse(m_DominationMatchTimeTextField.text);


    private void Awake()
    {
        m_Document = GetComponent<UIDocument>();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        FindUIElementsFromRoot();
        
        PrepareUI();

    #if !UNITY_SERVER && !UNITY_EDITOR
        m_ConnectionModeDropdown.choices = new List<string> 
        { 
            "Client" 
        };
    #else
        m_ConnectionModeDropdown.choices = new List<string> 
        { 
            "Host", 
            "Client", 
            "Server" 
        };
    #endif      

        m_GameModeDropdown.choices = new List<string>
        {
            "Deathmatch",
            "Domination",
        };    

        m_MaxPlayersDropdown.choices = new List<string>
        {
            "6",
            "8",
            "12"
        };              

        m_ConnectionModeDropdown.index = 0;
        m_GameModeDropdown.index = 0;

        // First init
        m_WorldsModeValue = DefineSelectedConnectionMode(m_ConnectionModeDropdown.value); 
        m_GameModeValue = DefineSelectedGameMode(m_GameModeDropdown.value);

        // Subscribes
        m_ConnectionModeDropdown.RegisterValueChangedCallback(OnConnectionModeChanged);
        m_GameModeDropdown.RegisterValueChangedCallback(OnGameModeChanged);

        m_StartButton.clicked += OnStartButtonClick;
        m_CancelButton.clicked += OnCancelButton;
        
        m_StartGameButton.clicked += OnStartGameButtonClick;
        m_ExitButton.clicked += OnExitButton;

        ShowCommonMatchOptions(m_WorldsModeValue);
        ShowGameModeOptions(m_WorldsModeValue, m_GameModeValue);
    }

    /// <summary>
    /// Preparing UI to using. It includes hiding and displaying certain windows. 
    /// </summary>
    private void PrepareUI()
    {
        m_ConnnectionScreen.style.display = DisplayStyle.None;
        m_StartScreen.style.display = DisplayStyle.Flex;
    }

    private void FindUIElementsFromRoot()
    {
        m_ConnnectionScreen = m_Document.rootVisualElement.Q<VisualElement>("ConnectionScreen");
        m_StartScreen = m_Document.rootVisualElement.Q<VisualElement>("StartScreen");
        
        m_NicknameTextField = m_ConnnectionScreen.Q<TextField>("nickname");
        m_AddressTextField = m_ConnnectionScreen.Q<TextField>("IP");
        m_PortIntegerField = m_ConnnectionScreen.Q<IntegerField>("Port");

        m_ConnectionModeDropdown = m_ConnnectionScreen.Q<DropdownField>("ConnectionMode");
        m_GameModeDropdown = m_ConnnectionScreen.Q<DropdownField>("GameMode");
        m_LevelMapDropdown = m_ConnnectionScreen.Q<DropdownField>("Level");

        m_StartGameButton = m_StartScreen.Q<Button>("Start");
        m_ExitButton = m_StartScreen.Q<Button>("Exit");
        
        m_StartButton = m_ConnnectionScreen.Q<Button>("StartConnection");
        m_CancelButton = m_ConnnectionScreen.Q<Button>("Cancel");
        
        m_MaxPlayersDropdown = m_ConnnectionScreen.Q<DropdownField>("MaxPlayers");

        m_DominationMatchTimeTextField = m_ConnnectionScreen.Q<TextField>("DominationMatchTime");
        m_DominationRevivalTimeTextField = m_ConnnectionScreen.Q<TextField>("DominationRevivalTime");
        m_DominationMaxScoreIntegerField = m_ConnnectionScreen.Q<IntegerField>("DominationMaxScore");

        m_DeathmatchRoundTimeTextField = m_ConnnectionScreen.Q<TextField>("DeathmatchRoundTime");
        m_DeathmatchNumberOfRoundsTextField = m_ConnnectionScreen.Q<TextField>("DeathmatchNumberOfRounds");
        
    }
    
    private void OnConnectionModeChanged(ChangeEvent<string> evt)
    {
        m_WorldsModeValue = DefineSelectedConnectionMode(m_ConnectionModeDropdown.value); 
        ShowCommonMatchOptions(m_WorldsModeValue);
        ShowGameModeOptions(m_WorldsModeValue, m_GameModeValue);
    }

    private void OnGameModeChanged(ChangeEvent<string> evt)
    {
        m_GameModeValue = DefineSelectedGameMode(m_GameModeDropdown.value);
        ShowGameModeOptions(m_WorldsModeValue, m_GameModeValue);
    }

    /// <summary>
    /// Shows certain options in depends of selected game mode. 
    /// </summary>
    private void ShowGameModeOptions(WorldsMode connectionMode, GameMode gameMode)
    {   
        if (connectionMode == WorldsMode.Client)
        {
            HideGameModeOptions();
            return;
        }

        switch (gameMode)
        {
            case GameMode.Deathmatch :
                ShowDeathmatchOptions();
                break;
            case GameMode.Domination :
                ShowDominationOptions();
                break;
            case GameMode.None:
                HideGameModeOptions();
                break;
        }
    }

    /// <summary>
    /// Shows certain match options in depends of connection mode.
    /// </summary>
    private void ShowCommonMatchOptions(WorldsMode mode)
    {
        switch (mode)
        {
            case WorldsMode.Host:
                ShowHostMatchOptions();
                break;
            case WorldsMode.Client:
                ShowClientMatchOptions();
                break;
            case WorldsMode.Server:
                ShowServerMatchOptions();
                break;
        }
    }

#region Common match settings

    // Host (Debug)
    private void ShowHostMatchOptions()
    {
        m_PortIntegerField.style.display = DisplayStyle.Flex;
        m_AddressTextField.style.display = DisplayStyle.None;
        m_NicknameTextField.style.display = DisplayStyle.Flex;
        
        m_LevelMapDropdown.style.display = DisplayStyle.Flex;
        
        m_GameModeDropdown.style.display = DisplayStyle.Flex; 
    }

    // Server (Debug)
    private void ShowServerMatchOptions()
    {
        m_PortIntegerField.style.display = DisplayStyle.Flex;
        m_AddressTextField.style.display = DisplayStyle.None;
        m_NicknameTextField.style.display = DisplayStyle.None;
        
        m_LevelMapDropdown.style.display = DisplayStyle.Flex;
        
        m_GameModeDropdown.style.display = DisplayStyle.Flex; 
    }

    // Client (Build)
    private void ShowClientMatchOptions()
    {
        m_PortIntegerField.style.display = DisplayStyle.Flex;
        m_AddressTextField.style.display = DisplayStyle.Flex;
        m_NicknameTextField.style.display = DisplayStyle.Flex;
        
        m_LevelMapDropdown.style.display = DisplayStyle.None;
        
        m_GameModeDropdown.style.display = DisplayStyle.Flex; 
    }

#endregion

#region  Game mode settings

    private void ShowDeathmatchOptions()
    {
        m_MaxPlayersDropdown.style.display = DisplayStyle.Flex; 
        
        m_DeathmatchNumberOfRoundsTextField.style.display = DisplayStyle.Flex;
        m_DeathmatchRoundTimeTextField.style.display = DisplayStyle.Flex;
        
        m_DominationMaxScoreIntegerField.style.display = DisplayStyle.None;
        m_DominationMatchTimeTextField.style.display = DisplayStyle.None;
        m_DominationRevivalTimeTextField.style.display = DisplayStyle.None;
    }

    private void ShowDominationOptions()
    {
        m_MaxPlayersDropdown.style.display = DisplayStyle.Flex; 
        
        m_DeathmatchNumberOfRoundsTextField.style.display = DisplayStyle.None; 
        m_DeathmatchRoundTimeTextField.style.display = DisplayStyle.None;
       
        m_DominationMaxScoreIntegerField.style.display = DisplayStyle.Flex;
        m_DominationMatchTimeTextField.style.display = DisplayStyle.Flex;
        m_DominationRevivalTimeTextField.style.display = DisplayStyle.Flex;
    }

    private void HideGameModeOptions()
    {
        m_MaxPlayersDropdown.style.display = DisplayStyle.None; 

        m_DeathmatchNumberOfRoundsTextField.style.display = DisplayStyle.None; 
        m_DeathmatchRoundTimeTextField.style.display = DisplayStyle.None;
       
        m_DominationMaxScoreIntegerField.style.display = DisplayStyle.None;
        m_DominationMatchTimeTextField.style.display = DisplayStyle.None;
        m_DominationRevivalTimeTextField.style.display = DisplayStyle.None;
    }

#endregion

    private GameMode DefineSelectedGameMode(string mode) => mode switch
    {
        "Domination" => GameMode.Domination,
        "Deathmatch" => GameMode.Deathmatch,
        _ => GameMode.None  
    };

    private WorldsMode DefineSelectedConnectionMode(string mode) => mode switch 
    {
        "Host" => WorldsMode.Host,
        "Client" => WorldsMode.Client,
        "Server" => WorldsMode.Server,
        _ => WorldsMode.Local
    };

    private void OnStartButtonClick()
    {
        Debug.Log($"Start Button Clicked. Nickname: {m_NicknameValue}, ConnectionMode: {m_WorldsModeValue}, GameMode {m_GameModeValue}, IP: {m_AddressTextField.value}, Port: {m_PortIntegerField.value}");

        CreateWorlds(m_WorldsModeValue);

        m_ConnnectionScreen.SetEnabled(false);
    }

    private void OnCancelButton()
    {
        ShowStartScreenAndHidPrevious();
    }

    private void OnStartGameButtonClick()
    {
        ShowConnectionAndHidePreviousScreen();
    }

    private void OnExitButton()
    {
        #if !UNITY_SERVER && !UNITY_EDITOR
            Application.Quit();
        #elif UNITY_EDITOR
            EditorApplication.isPlaying = false;
        #endif
    }

    /// <summary>
    /// Creates neccessery worlds in depends of worlds mode
    /// </summary>
    /// <param name="mode"></param>
    private void CreateWorlds(WorldsMode mode)
    {
        WorldsManager.DestroyLocalSimulationWorld();
        
        switch (mode)
        {
            case WorldsMode.Host:
                WorldsManager.StartHost(
                    port: m_PortValue, 
                    address: m_AddressValue, 
                    gameMode: m_GameModeValue, 
                    maxPlayers: m_MaxPlayersValue,
                    levelMap: m_LevelValue,
                    
                    deathmatchNumberOfRounds: m_DeathmatchNumberOfRoundsValue, 
                    deathmatchRoundTime: m_DeathmatchRoundTimeValue, 
                    
                    dominationMaxScore: m_DominationMaxScoreValue,
                    dominationMatchTime: m_DominationMatchTimeValue,
                    dominationRivaleTime: m_DominationMaxScoreValue
                );
                CreateClientGameEntryRequest();
                SceneManager.LoadSceneAsync("Game", LoadSceneMode.Single);    
                SceneManager.LoadSceneAsync("UI", LoadSceneMode.Additive);   
                break;
            
            case WorldsMode.Server:
                WorldsManager.CreateServerWorld(
                    port: m_PortValue, 
                    gameMode: m_GameModeValue, 
                    maxPlayers: m_MaxPlayersValue,
                    levelMap: m_LevelValue,
 
                    deathmatchRoundTime: m_DeathmatchRoundTimeValue, 
                    deathmatchNumberOfRounds: m_DeathmatchNumberOfRoundsValue,  
 
                    dominationMaxScore: m_DominationMaxScoreValue,
                    dominationMatchTime: m_DominationMatchTimeValue,
                    dominationRivaleTime: m_DominationRevivalTimeValue
                );
                SceneManager.LoadSceneAsync("Game", LoadSceneMode.Single);
                // without UI 
                break;
            
            case WorldsMode.Client:
                WorldsManager.CreateClientWorld(
                    m_PortValue, 
                    m_AddressValue
                );
                CreateClientGameEntryRequest();
                SceneManager.LoadSceneAsync("Game", LoadSceneMode.Single);    
                SceneManager.LoadSceneAsync("UI", LoadSceneMode.Additive);  
                break;
        }
    }

    private void CreateClientGameEntryRequest()
    {
        var gameEntryRequestEntity = WorldsManager.currentClientWorld.EntityManager.CreateEntity();
        WorldsManager.currentClientWorld.EntityManager.AddComponentData(gameEntryRequestEntity, new ClientPlayerInitRequest
        {
            GameMode = m_GameModeValue,
            Nickname = m_NicknameValue,
        });
    }

    private void ShowConnectionAndHidePreviousScreen()
    {
        m_StartScreen.style.display = DisplayStyle.None;
        m_ConnnectionScreen.style.display = DisplayStyle.Flex;
    }

    private void ShowStartScreenAndHidPrevious()
    {
        m_StartScreen.style.display = DisplayStyle.Flex;
        m_ConnnectionScreen.style.display = DisplayStyle.None;
    }

    private void OnDisable()
    {
        #if !UNITY_SERVER
            m_StartButton.clicked -= OnStartButtonClick;
            m_CancelButton.clicked -= OnCancelButton;

            m_StartGameButton.clicked -= OnStartGameButtonClick;
            m_ExitButton.clicked -= OnExitButton;
        #endif
    }
}