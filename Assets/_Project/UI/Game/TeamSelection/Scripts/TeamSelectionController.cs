using Unity.Entities;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class TeamSelectionScreen : MonoBehaviour, IUIView
{
    private UIDocument m_Document;
    
    private VisualElement m_Root;

    private VisualElement m_Container;

    private Button m_SelectRedButton;
    private Button m_SelectBlueButton;

    public Label m_RedPlayersNumber;
    private Label m_BluePlayersNumber;

    private EntityManager m_EntityManager;

    public void Init()
    {
        m_Document = GetComponent<UIDocument>();

        m_Root = m_Document.rootVisualElement;

        m_Container = m_Root.Q<VisualElement>("teamSelectionContainer");

        m_SelectRedButton = m_Root.Q<Button>("RedTeamButton");
        m_SelectBlueButton = m_Root.Q<Button>("BlueTeamButton");

        m_RedPlayersNumber = m_Root.Q<Label>("playerNumbersRed");
        m_BluePlayersNumber = m_Root.Q<Label>("playerNumbersBlue");

        m_EntityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
    
        m_SelectRedButton.RegisterCallback<ClickEvent>(evt =>
        {
            JoinPlayerToTeam(TeamType.Red);
        });

        m_SelectBlueButton.RegisterCallback<ClickEvent>(evt =>
        {
            JoinPlayerToTeam(TeamType.Blue);
        });

        // To use in system bases
        RegisterDocumentHelper.RegisterDocument(ref m_EntityManager, this);
    }
    
    public void Show()
    {
        m_Container.style.display = DisplayStyle.Flex;
    }

    public void Hide()
    {
        m_Container.style.display = DisplayStyle.None;
    }

    public void JoinPlayerToTeam(TeamType teamType)
    {
        Entity joinPlayerTeamRequest = m_EntityManager.CreateEntity();
        m_EntityManager.AddComponentData(joinPlayerTeamRequest, new ClientJoinPlayerTeam
        {
            RequestedTeamType = teamType
        });
    }

    public void UpdatePlayersNumber(int playersNumberRed, int playersNumberBlue)
    {
        m_RedPlayersNumber.text = $"x{playersNumberRed}";
        m_BluePlayersNumber.text = $"x{playersNumberBlue}";
    }

    public void DisplayWindow()
    {
        m_Root.style.display = DisplayStyle.Flex;
    }

    public void HideWindow()
    {
        m_Root.style.display = DisplayStyle.None;
    }
}

public struct ClientJoinPlayerTeam : IComponentData
{
    public TeamType RequestedTeamType;
}

// команда это сущность
// при попытке присоединиться отправляется запрос 
// запрос принимается, смотрится команда и если она не валидна - запрос уничтожается и ничего не происходит
// если валидно - меняется статус игрока 
// статус поменялся с playerSelection на pendingStartMatch - отображается PlayerList с инфой о том, сколько еще должно присоединиться