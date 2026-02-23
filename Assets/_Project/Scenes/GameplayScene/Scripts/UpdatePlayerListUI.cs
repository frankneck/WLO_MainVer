using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class UpdatePlayerListUI : MonoBehaviour
{
    public static UpdatePlayerListUI instance;

    private UIDocument document;
    private VisualElement PlayerListScreen;
    private MultiColumnListView RedTeamPlayerListView;
    private MultiColumnListView BlueTeamPlayerListView;

    private List<PlayerListData> RedTeamPlayerList = new List<PlayerListData>();
    private List<PlayerListData> BlueTeamPlayerList = new List<PlayerListData>();

    private void OnEnable()
    {
        instance = this;
     
        document = GetComponent<UIDocument>();
     
        if (document == null) { Debug.LogWarning("UIDocument not found"); return; }


        UpdatePlayerListData();

        document = GetComponent<UIDocument>();
        PlayerListScreen = document.rootVisualElement.Query<VisualElement>("PlayerListScreen");

        RedTeamPlayerListView = PlayerListScreen.Query<MultiColumnListView>("RedTeamPlayerListView");
        RedTeamPlayerListView.itemsSource = RedTeamPlayerList;
        InitializeMultiColumnListView(RedTeamPlayerListView, RedTeamPlayerList);

        BlueTeamPlayerListView = PlayerListScreen.Query<MultiColumnListView>("BlueTeamPlayerListView");
        BlueTeamPlayerListView.itemsSource = BlueTeamPlayerList;
        InitializeMultiColumnListView(BlueTeamPlayerListView, BlueTeamPlayerList);

        PlayerListScreen.style.display = DisplayStyle.None;

         PlayerInput.PlayersList.performed += PlayerListScreenEnable;
        PlayerInput.PlayersList.canceled += PlayerListScreenDisable;
    }

    void OnDisable()
    {
        if (document == null) return; 

        PlayerListScreenDisable();
        PlayerInput.PlayersList.performed -= PlayerListScreenEnable;
        PlayerInput.PlayersList.canceled -= PlayerListScreenDisable;
    }

    void UpdatePlayerListData()
    {
        RedTeamPlayerList.Clear();
        BlueTeamPlayerList.Clear();

        World world = World.DefaultGameObjectInjectionWorld;
        EntityManager em = world.EntityManager;
        var query = em.CreateEntityQuery(ComponentType.ReadOnly<PlayerName>(), ComponentType.ReadOnly<PlayerTeam>(), ComponentType.ReadOnly<PlayerPing>());

        foreach(var entity in query.ToEntityArray(Allocator.Temp))
        {
            var name = em.GetComponentData<PlayerName>(entity);
            var playerTeam = em.GetComponentData<PlayerTeam>(entity);
            var ping = em.GetComponentData<PlayerPing>(entity);

            if (playerTeam.Value == TeamType.Red)
            {
                RedTeamPlayerList.Add(new PlayerListData(name.Value.ToString(), ping.Value));
            }
            else if (playerTeam.Value == TeamType.Blue)
            {
                BlueTeamPlayerList.Add(new PlayerListData(name.Value.ToString(), ping.Value));
            }
        }

        query.Dispose();
    }

    public void PlayerListScreenRefreshItems()
    {
        if(PlayerListScreen.style.display == DisplayStyle.None)
            return;
        
        UpdatePlayerListData();
        RedTeamPlayerListView.RefreshItems();
        BlueTeamPlayerListView.RefreshItems();
    }

    void PlayerListScreenEnable(InputAction.CallbackContext callbackContext)
    {
        UpdatePlayerListData();
        RedTeamPlayerListView.RefreshItems();
        BlueTeamPlayerListView.RefreshItems();
        PlayerListScreen.style.display = DisplayStyle.Flex;
    }

    void PlayerListScreenDisable(InputAction.CallbackContext callbackContext) => PlayerListScreenDisable();

    void PlayerListScreenDisable() => PlayerListScreen.style.display = DisplayStyle.None;

    void InitializeMultiColumnListView(MultiColumnListView listView, List<PlayerListData> values)
    {
        listView.columns["player-name"].makeCell = () =>
        {
            Label nameLabel = new Label() { name = "nameLabel" };
            nameLabel.style.color = new StyleColor(Color.white);
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
            pingLabel.style.color = new StyleColor(Color.white);
            pingLabel.style.paddingLeft = new StyleLength(10);
            return pingLabel;
        };
        listView.columns["player-ping"].bindCell = (element, i) =>
        {
            Label pingLabel = element.Q<Label>("pingLabel");
            pingLabel.text = values[i].ping.ToString();
        };
    }

    struct PlayerListData
    {
        public string name;
        public ushort ping;

        public PlayerListData(string name, ushort ping)
        {
            this.name = name;
            this.ping = ping;
        }
    }
}