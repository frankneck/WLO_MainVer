using System;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class ConnectionFrontend : MonoBehaviour
{
    private UIDocument document;
    private VisualElement mainmenuVE;
    private TextField NicknameField;
    private TextField AddressField;
    private IntegerField PortField;
    private DropdownField ModeDropdown;
    private DropdownField TeamDropdown;
    private Button StartButton;
    private Button appQuitButton;

    private ushort Port => (ushort)PortField.value;
    private string Address => AddressField.value;
    private FixedString32Bytes Nickname => (FixedString32Bytes)NicknameField.value;
    private TeamRequest teamRequest => (TeamRequest)TeamDropdown.index;

    public void Start()
    {
        document = GetComponent<UIDocument>();
        mainmenuVE = document.rootVisualElement.Q<VisualElement>("MainMenu");
        NicknameField = mainmenuVE.Q<TextField>("nickname");
        AddressField = mainmenuVE.Q<TextField>("IP");
        PortField = mainmenuVE.Q<IntegerField>("Port");
        ModeDropdown = mainmenuVE.Q<DropdownField>("Mode");
        TeamDropdown = mainmenuVE.Q<DropdownField>("Team");
        StartButton = mainmenuVE.Q<Button>("Start");
        appQuitButton = mainmenuVE.Q<Button>("Exit");

        StartButton.clicked += OnStartButtonClick;
        ModeDropdown.RegisterValueChangedCallback(evt =>
        {
            AddressField.style.display = ModeDropdown.index != 0 ? DisplayStyle.Flex : DisplayStyle.None;
        });
        appQuitButton.clicked += OnExitButton;

        AddressField.style.display = ModeDropdown.index != 0 ? DisplayStyle.Flex : DisplayStyle.None;

        PlayerInput.Initialize();
    }

    public void OnDisable()
    {
        StartButton.clicked -= OnStartButtonClick;
        appQuitButton.clicked -= OnExitButton;
    }

    public void OnExitButton()
    {
        Debug.Log("Exit Button Clicked.");
        Application.Quit();
    }

    public void OnStartButtonClick()
    {
        WorldsMode mode = ModeDropdown.index == 0 ? WorldsMode.Host : WorldsMode.Client;

#if UNITY_EDITOR
        Debug.Log($"Start Button Clicked. Nickname: {Nickname}, Mode: {mode}, IP: {AddressField.value}, Port: {PortField.value}, Team: {teamRequest}");
#endif

        WorldsManager.DestroyLocalSimulationWorld();
        switch (mode)
        {
            case WorldsMode.Host:
                WorldsManager.StartHost(Port, "127.0.0.1");
                CreateClientGameEntryRequest();
                break;
            case WorldsMode.Server:
                WorldsManager.StartServer(Port);
                break;
            case WorldsMode.Client:
                WorldsManager.StartClient(Port, Address);
                CreateClientGameEntryRequest();
                break;
        }
        
        mainmenuVE.SetEnabled(false);
    }

    public void CreateClientGameEntryRequest()
    {
        var gameEntryRequestEntity = WorldsManager.currentClientWorld.EntityManager.CreateEntity();
        WorldsManager.currentClientWorld.EntityManager.AddComponentData(gameEntryRequestEntity, new ClientPlayerInitRequest
        {
            Nickname = Nickname,
            Team = teamRequest
        });
    }
}

public enum TeamRequest : byte
{
    Auto = 0,
    Blue = 1,
    Red = 2,
    Spectator
}

public static class TeamRequestExtensions
{
    public static TeamType ToTeamType(this TeamRequest teamRequest)
    {
        return teamRequest switch
        {
            TeamRequest.Blue => TeamType.Blue,
            TeamRequest.Red => TeamType.Red,
            _ => TeamType.Spectator
        };
    }
}