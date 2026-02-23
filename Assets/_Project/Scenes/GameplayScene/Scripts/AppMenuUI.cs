using Unity.Entities;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class AppMenuUI : MonoBehaviour
{
    private UIDocument document;
    private UpdatePlayerListUI playerListUI;

    private VisualElement menuScreen;

    private Button disconnectButton;
    private Button closeHostButton;
    private Button appQuitButton;

    private bool _menuEnable;
    private bool menuEnable
    {
        get => _menuEnable;
        set
        {
            _menuEnable = value;
            UpdateMenuEnable();
            if (playerListUI != null)
                playerListUI.enabled = !value;
        }
    }

    void OnEnable()
    {
        document = GetComponent<UIDocument>();
        playerListUI = GetComponent<UpdatePlayerListUI>();

        var em = World.DefaultGameObjectInjectionWorld.EntityManager;

        menuScreen = document.rootVisualElement.Query<VisualElement>("AppMenuScreen");
        disconnectButton = menuScreen.Query<Button>("disconnectButton");
        appQuitButton = menuScreen.Query<Button>("appQuitButton");
        closeHostButton = menuScreen.Query<Button>("closeHostButton");

        disconnectButton.clicked += () => WorldsManager.Disconnect();
        closeHostButton.clicked += () => WorldsManager.Disconnect();
        appQuitButton.clicked += () => Application.Quit();

        closeHostButton.style.display = WorldsManager.currentMode == WorldsMode.Client ? DisplayStyle.None : DisplayStyle.Flex;
        disconnectButton.style.display = WorldsManager.currentMode == WorldsMode.Host ? DisplayStyle.None : DisplayStyle.Flex;

        menuEnable = false;
        PlayerInput.AppMenu.performed += SetMenuEnable;
    }

    void OnDisable()
    {
        menuEnable = false;
        PlayerInput.AppMenu.performed -= SetMenuEnable;

        disconnectButton.clicked -= () => WorldsManager.Disconnect();
        closeHostButton.clicked -= () => WorldsManager.Disconnect();
        appQuitButton.clicked -= () => Application.Quit();
    }

    void SetMenuEnable(InputAction.CallbackContext callbackContext)
    {
        menuEnable = !menuEnable;
    }

    void UpdateMenuEnable()
    {
        if (menuEnable)
        {
            menuScreen.style.display = DisplayStyle.Flex; 
        }
        else
        {
            menuScreen.style.display = DisplayStyle.None;
        }
    } 
}