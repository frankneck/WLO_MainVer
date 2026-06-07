using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class PlayerListWindow : MonoBehaviour, IUIView
{
    public static PlayerListWindow Instance;
    private UIDocument m_Document;
    private VisualElement m_Root;
    private VisualElement m_Container;
    private MultiColumnListView m_RedTeamPlayerListView;
    private MultiColumnListView m_BlueTeamPlayerListView;

    private List<PlayerListData> RedTeamPlayerList = new List<PlayerListData>();
    private List<PlayerListData> BlueTeamPlayerList = new List<PlayerListData>();

    public void Init()
    {
        Instance = this;
     
        m_Document = GetComponent<UIDocument>();

        m_Root = m_Document.rootVisualElement;
     
        if (m_Document == null) 
        { 
            Debug.LogWarning("UIDocument not found"); 
            return; 
        }

        UpdatePlayerListData();

        m_Container = m_Document.rootVisualElement.Query<VisualElement>("PlayerListContainer");

        m_RedTeamPlayerListView = m_Container.Query<MultiColumnListView>("RedTeamPlayerListView");
        m_RedTeamPlayerListView.itemsSource = RedTeamPlayerList;
        InitializeMultiColumnListView(m_RedTeamPlayerListView, RedTeamPlayerList);

        m_BlueTeamPlayerListView = m_Container.Query<MultiColumnListView>("BlueTeamPlayerListView");
        m_BlueTeamPlayerListView.itemsSource = BlueTeamPlayerList;
        InitializeMultiColumnListView(m_BlueTeamPlayerListView, BlueTeamPlayerList);

        PlayerInput.PlayersList.performed += PlayerListScreenEnable;
        // PlayerInput.PlayersList.canceled += PlayerListScreenDisable;

        PrepareUI();
    }

    public void Show()
    {
        m_Container.style.display = DisplayStyle.Flex;
    }

    public void Hide()
    {
        m_Container.style.display = DisplayStyle.None;
    }

    private void OnDisable()
    {
        if (m_Document == null) 
            return; 

        // PlayerListScreenDisable();
        PlayerInput.PlayersList.performed -= PlayerListScreenEnable;
        // PlayerInput.PlayersList.canceled -= PlayerListScreenDisable;
    }

    private void UpdatePlayerListData()
    {
        RedTeamPlayerList.Clear();
        BlueTeamPlayerList.Clear();

        World world = World.DefaultGameObjectInjectionWorld;
        EntityManager em = world.EntityManager;
        
        var query = em.CreateEntityQuery(ComponentType.ReadOnly<PlayerName>(), 
            ComponentType.ReadOnly<GameTeam>(), 
            ComponentType.ReadOnly<PlayerPing>()
        );

        foreach(var entity in query.ToEntityArray(Allocator.Temp))
        {
            var kd = em.GetComponentData<KDCounter>(entity);
            var name = em.GetComponentData<PlayerName>(entity);
            var playerTeam = em.GetComponentData<GameTeam>(entity);
            var ping = em.GetComponentData<PlayerPing>(entity);

            if (playerTeam.Value == TeamType.Red)
            {
                RedTeamPlayerList.Add(new PlayerListData(name.Value.ToString(), ping.Value, kd.Kills, kd.Deaths));
            }
            else if (playerTeam.Value == TeamType.Blue)
            {
                BlueTeamPlayerList.Add(new PlayerListData(name.Value.ToString(), ping.Value, kd.Kills, kd.Deaths));
            }
        }

        query.Dispose();
    }

    private void PlayerListScreenEnable(InputAction.CallbackContext callbackContext)
    {
        UpdatePlayerListData();
        m_RedTeamPlayerListView.RefreshItems();
        m_BlueTeamPlayerListView.RefreshItems();
    }

    private void InitializeMultiColumnListView(MultiColumnListView listView, List<PlayerListData> values)
    {
        listView.columns["player-name"].makeCell = () =>
        {
            Label nameLabel = new Label() { name = "nameLabel" };
            nameLabel.style.paddingLeft = new StyleLength(10);
            return nameLabel;
        };
        listView.columns["player-name"].bindCell = (element, i) =>
        {
            Label nicknameLabel = element.Q<Label>("nameLabel");
            nicknameLabel.text = values[i].name;
        };

        listView.columns["player-ping"].makeCell = () =>
        {
            Label pingLabel = new Label() { name = "pingLabel" };
            pingLabel.style.paddingLeft = new StyleLength(10);
            return pingLabel;
        };
        listView.columns["player-ping"].bindCell = (element, i) =>
        {
            Label pingLabel = element.Q<Label>("pingLabel");
            pingLabel.text = values[i].ping.ToString();
        };

        listView.columns["player-kills"].makeCell = () =>
        {
            Label killsLabel = new Label() { name = "killsLabel" };
            killsLabel.style.paddingLeft = new StyleLength(10);
            return killsLabel;
        };
        listView.columns["player-kills"].bindCell = (element, i) =>
        {
            Label killsLabel = element.Q<Label>("killsLabel");
            killsLabel.text = values[i].kills.ToString();
        };
        
        listView.columns["player-deaths"].makeCell = () =>
        {
            Label deathsLabel = new Label() { name = "deathsLabel" };
            deathsLabel.style.paddingLeft = new StyleLength(10);
            return deathsLabel;
        };
        listView.columns["player-deaths"].bindCell = (element, i) =>
        {
            Label deathsLabel = element.Q<Label>("deathsLabel");
            deathsLabel.text = values[i].deaths.ToString();
        };
    }

    private void PrepareUI()
    {
        m_Container.style.display = DisplayStyle.None;
    }

    private struct PlayerListData
    {
        public string name;
        public ushort ping;
        public int deaths;
        public int kills;

        public PlayerListData(string name, ushort ping, int kills, int deaths)
        {
            this.name = name;
            this.ping = ping;
            this.deaths = deaths;
            this.kills = kills;    
        }
    }
}