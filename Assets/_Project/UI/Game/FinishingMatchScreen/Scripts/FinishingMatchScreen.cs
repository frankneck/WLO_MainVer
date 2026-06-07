using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class FinishingMatchScreen : MonoBehaviour, IUIView, IGameModeView
{
    private UIDocument m_Document;
    private VisualElement m_Root;
    private VisualElement m_MainContainer;

    private VisualElement m_DominationSection;
    private VisualElement m_DeathmatchSection;

    private Label m_MainTitleTextField;

    private Label m_FooterNextMatchTimerTextField;

    private MultiColumnListView m_RedTeamListView;
    private MultiColumnListView m_BlueTeamListView;
    private MultiColumnListView m_PlayersListView;

    private List<PlayerListData> RedTeamList = new List<PlayerListData>();
    private List<PlayerListData> BlueTeamList = new List<PlayerListData>();
    private List<PlayerListData> PlayersList = new List<PlayerListData>();

    public void Init()
    {
        m_Document = GetComponent<UIDocument>();
        m_Root = m_Document.rootVisualElement;
        m_MainContainer = m_Document.rootVisualElement.Query<VisualElement>("FinishingMatchScreenContainer");

        // GameMode sections

        m_DominationSection = m_MainContainer.Q<VisualElement>("DominationSection");
        m_DeathmatchSection = m_MainContainer.Q<VisualElement>("DeathmatchSection");

        // Titles

        m_MainTitleTextField = m_MainContainer.Q<Label>("TitleSectionText");

        // Footer 

        m_FooterNextMatchTimerTextField = m_MainContainer.Q<Label>("StartNewMatchTimerText");
        
        // MultiListViews

        m_RedTeamListView = m_MainContainer.Query<MultiColumnListView>("RedTeamListView");
        m_RedTeamListView.itemsSource = RedTeamList;
        InitializeMultiColumnListView(m_RedTeamListView, RedTeamList);

        m_BlueTeamListView = m_MainContainer.Query<MultiColumnListView>("BlueTeamListView");
        m_BlueTeamListView.itemsSource = BlueTeamList;
        InitializeMultiColumnListView(m_BlueTeamListView, BlueTeamList);

        m_PlayersListView = m_MainContainer.Q<MultiColumnListView>("PlayersListView");
        m_PlayersListView.itemsSource = PlayersList;
        InitializeMultiColumnListView(m_PlayersListView, PlayersList);

        var em = World.DefaultGameObjectInjectionWorld.EntityManager;
        RegisterDocumentHelper.RegisterDocument(ref em, this);

        PrepareUI();
    }

    public void UpdateListViewsData(EntityManager em, NativeArray<Entity> playerEntities)
    {
        RedTeamList.Clear();
        BlueTeamList.Clear();
        PlayersList.Clear();

        foreach(var entity in playerEntities)
        {
            var kd = em.GetComponentData<KDCounter>(entity);
            var name = em.GetComponentData<PlayerName>(entity);
            var playerTeam = em.GetComponentData<GameTeam>(entity);

            if (playerTeam.Value == TeamType.Red)
            {
                RedTeamList.Add(new PlayerListData(name.Value.ToString(), kd.Kills, kd.Deaths));
            }
            else if (playerTeam.Value == TeamType.Blue)
            {
                BlueTeamList.Add(new PlayerListData(name.Value.ToString(), kd.Kills, kd.Deaths));
            }
            else
            {
                PlayersList.Add(new PlayerListData(name.Value.ToString(), kd.Kills, kd.Deaths));
            }
        }

        m_PlayersListView.RefreshItems();
        m_BlueTeamListView.RefreshItems();
        m_RedTeamListView.RefreshItems();
    }

    public void Hide()
    {
        m_MainContainer.style.display = DisplayStyle.None;
    }

    public void SetOnMode(GameMode mode)
    {
        switch (mode)
        {
            case GameMode.Deathmatch:
                m_DeathmatchSection.style.display = DisplayStyle.Flex;
                m_DominationSection.style.display = DisplayStyle.None;
                break;
            case GameMode.Domination:
                m_DeathmatchSection.style.display = DisplayStyle.None;
                m_DominationSection.style.display = DisplayStyle.Flex;
                break;
        }
    }

    public void Show()
    {
        m_MainContainer.style.display = DisplayStyle.Flex;
    }

    public void SetMainTitle(TeamType winnerTeam, int blueTeamScore, int redTeamScore)
    {
        string winner;

        switch (winnerTeam)
        {
            case TeamType.Red :
                winner = "Red team";
                break;
            case TeamType.Blue :
                winner = "Blue team";
                break;
            default :
                winner = "No one team";
                break;
        }
        
        m_MainTitleTextField.text = $"Red team: {redTeamScore}; Blue team: {blueTeamScore}; {winner} won!";
    }

    public void SetMainTitle(string playerName, int playerScore)
    {
        m_MainTitleTextField.text = $"{playerName} won with score {playerScore}!";
    }

    public void UpdateTimer(int seconds)
    {
        m_FooterNextMatchTimerTextField.text = $"Next match will start through {seconds} seconds";
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
        m_MainContainer.style.display = DisplayStyle.None;
    }

    private struct PlayerListData
    {
        public string name;
        public int deaths;
        public int kills;

        public PlayerListData(string name, int kills, int deaths)
        {
            this.name = name;
            this.deaths = deaths;
            this.kills = kills;    
        }
    }
}
