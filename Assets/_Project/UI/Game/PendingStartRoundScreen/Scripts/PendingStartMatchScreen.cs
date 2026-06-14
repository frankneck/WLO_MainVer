using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class PendingStartMatchScreen : MonoBehaviour, IUIView, IGameModeView
{
    private UIDocument m_Document;
    private VisualElement m_Root;
    private VisualElement m_MainContainer;
    
    private VisualElement m_DeathmatchScreenContainer;
    private VisualElement m_DominationScreenContainer;

    private MultiColumnListView m_RedTeamPlayerListView;
    private MultiColumnListView m_BlueTeamPlayerListView;
    private MultiColumnListView m_DominationPlayerListView;

    private Label m_Title;
    private Label m_RedTeamTitle;
    private Label m_BlueTeamTitle;
    private Label m_PlayersNumberTitle;

    private List<PlayerListData> RedTeamPlayerList = new List<PlayerListData>();
    private List<PlayerListData> BlueTeamPlayerList = new List<PlayerListData>();
    private List<PlayerListData> DominationPlayerList = new List<PlayerListData>();

    public void Init()
    {
        m_Document = GetComponent<UIDocument>();

        m_Root = m_Document.rootVisualElement;

        if (m_Document == null) 
        { 
            Debug.LogWarning("UIDocument not found"); 
            return; 
        }

        m_MainContainer = m_Document.rootVisualElement.Q<VisualElement>("PendingsStartMatchScreenContainer");

        m_DeathmatchScreenContainer = m_MainContainer.Q<VisualElement>("DeathmatchSection");
        m_DominationScreenContainer = m_MainContainer.Q<VisualElement>("DominationSection");

        m_Title = m_MainContainer.Q<Label>("Title");
        
        m_RedTeamTitle = m_MainContainer.Q<Label>("RedTeamTitle");
        m_BlueTeamTitle = m_MainContainer.Q<Label>("BlueTeamTitle");
        m_PlayersNumberTitle = m_MainContainer.Q<Label>("PlayersNumber");

        m_RedTeamPlayerListView = m_MainContainer.Q<MultiColumnListView>("RedTeamListView");
        m_RedTeamPlayerListView.itemsSource = RedTeamPlayerList;
        InitializeMultiColumnListView(m_RedTeamPlayerListView, RedTeamPlayerList);

        m_BlueTeamPlayerListView = m_MainContainer.Q<MultiColumnListView>("BlueTeamListView");
        m_BlueTeamPlayerListView.itemsSource = BlueTeamPlayerList;
        InitializeMultiColumnListView(m_BlueTeamPlayerListView, BlueTeamPlayerList);

        m_DominationPlayerListView = m_MainContainer.Q<MultiColumnListView>("DominationListView");
        m_DominationPlayerListView.itemsSource = DominationPlayerList;
        InitializeMultiColumnListView(m_DominationPlayerListView, DominationPlayerList);

        var em = World.DefaultGameObjectInjectionWorld.EntityManager;
        RegisterDocumentHelper.RegisterDocument(ref em, this);

        PrepareUI();
    }

    public void Show()
    {
        m_MainContainer.style.display = DisplayStyle.Flex;
    }   

    public void Hide()
    {
        m_MainContainer.style.display = DisplayStyle.None;
    }

    public void UpdatePlayerListDataForDomination(List<PlayerListData> players)
    {
        DominationPlayerList.Clear();
        DominationPlayerList.AddRange(players);
        m_DominationPlayerListView.RefreshItems();
    }

    public void UpdatePlayerListDataForDeathmatch(
        List<PlayerListData> red, 
        List<PlayerListData> blue)
    {
        UnityEngine.Debug.Log("UpdatePlayerListData");

        RedTeamPlayerList.Clear();
        RedTeamPlayerList.AddRange(red);

        BlueTeamPlayerList.Clear();
        BlueTeamPlayerList.AddRange(blue);

        m_RedTeamPlayerListView.RefreshItems();
        m_BlueTeamPlayerListView.RefreshItems();
    }

    public void UpdateMainTitle(int total, int max)
    {
        var remaining = max - total;

        if (remaining < 0)
        {
            // UnityEngine.Debug.LogWarning("Warning. Invalid value");
            return;
        }

        m_Title.text = $"Remaining {remaining} players to start match";
    }

    public void UpdatePlayersTitle(int total, int max)
    {
        m_PlayersNumberTitle.text = $"Players {total}/{max}";
    }

    public void UpdateBlueTitle(int total, int max)
    {
        m_BlueTeamTitle.text = $"Blue Team {total}/{max}";
    }

    public void UpdateRedTitle(int total, int max)
    {
        m_RedTeamTitle.text = $"Red Team {total}/{max}";
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
            nicknameLabel.text = values[i].Name;
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
            pingLabel.text = values[i].Ping.ToString();
        };
    }

    private void PrepareUI()
    {
        m_MainContainer.style.display = DisplayStyle.None;
    }

    public void SetOnMode(GameMode mode)
    {
        switch (mode)
        {
            case GameMode.Deathmatch :
                m_DeathmatchScreenContainer.style.display = DisplayStyle.Flex;
                m_DominationScreenContainer.style.display = DisplayStyle.None;
                break;
            case GameMode.Domination :
                m_DeathmatchScreenContainer.style.display = DisplayStyle.None;
                m_DominationScreenContainer.style.display = DisplayStyle.Flex;
                break;
        }
    }

    public struct PlayerListData
    {
        public string Name;
        public uint Ping;

        public PlayerListData(string name, uint ping)
        {
            Ping = ping;
            Name = name;
        }
    }
}
