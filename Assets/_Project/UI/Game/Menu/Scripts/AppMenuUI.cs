using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class AppMenuWindow : MonoBehaviour, IUIView
{
    private UIDocument document;
    
    private VisualElement m_Root;

    private VisualElement m_MenuScreen;

    private Button disconnectButton;
    private Button closeHostButton;
    private Button appQuitButton;

    public void Init()
    {
        document = GetComponent<UIDocument>();
        m_Root = document.rootVisualElement;

        m_MenuScreen = m_Root.Query<VisualElement>("AppMenuScreen");

        disconnectButton = m_MenuScreen.Query<Button>("disconnectButton");
        appQuitButton = m_MenuScreen.Query<Button>("appQuitButton");
        closeHostButton = m_MenuScreen.Query<Button>("closeHostButton");

        disconnectButton.clicked += OnDisconnect;
        closeHostButton.clicked += () => WorldsManager.Disconnect();
        appQuitButton.clicked += () => Application.Quit();

        closeHostButton.style.display = WorldsManager.currentMode == WorldsMode.Client 
            ? DisplayStyle.None : DisplayStyle.Flex;
        
        disconnectButton.style.display = WorldsManager.currentMode == WorldsMode.Host 
            ? DisplayStyle.None : DisplayStyle.Flex;
    
        PrepareUI();
    }

    #region Public API

    public void Show()
    {
        m_MenuScreen.style.display = DisplayStyle.Flex;
    }

    public void Hide()
    {
        m_MenuScreen.style.display = DisplayStyle.None;
    }

    #endregion

    private void OnDisconnect()
    {
        var em = World.DefaultGameObjectInjectionWorld.EntityManager;
        var request = em.CreateEntity();
        em.AddComponent<ClientOnDisconnectButtonRequest>(request);
    }

    private void PrepareUI()
    {
        m_MenuScreen.style.display = DisplayStyle.None;   
    }

    private void OnDisable()
    {
        disconnectButton.clicked -= () => WorldsManager.Disconnect();
        closeHostButton.clicked -= () => WorldsManager.Disconnect();
        appQuitButton.clicked -= () => Application.Quit();
    }
}

