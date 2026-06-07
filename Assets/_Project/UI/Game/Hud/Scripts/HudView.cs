using System.Collections;
using System.Linq;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class HudScreen : MonoBehaviour, IUIView, IGameModeView
{    
    private UIDocument m_Document;
    private VisualElement m_Root;
    private VisualElement m_EquipmentContainer;

    private Slot[] m_Slots;
    private ItemManagedDatabase m_ItemManagedDatabase;

    private VisualElement m_Healthbar;
    private VisualElement m_Manabar;
    private VisualElement m_HealthbarFill;
    private VisualElement m_ManabarFill;    
    private VisualElement m_HudContainer;
    private VisualElement m_RespawnWindowContainer;
    private VisualElement m_Tooltip;
    private Label m_TooltipText;
    
    private Label m_TimerText;

    private Label m_DeathmatchPlayedRoundsTextField;
    private Label m_DeathmatchRedScoreTextField;
    private Label m_DeathmatchBlueScoreTextField;

    private Label m_WinnerContainerTextField;
    private VisualElement m_WinnerContainer;
    private Label m_DeathmatchWinnerTextField;
    private Label m_DominationWinnerTextField;
    
    private VisualElement m_DeadInfoWindow;
    private Label m_DeadInfoTextField;

    private VisualElement m_RespawnInfoWindow;
    private Label m_RespawnInfoTextField;
    
    private VisualElement m_HelpSection;

    private VisualElement m_CrosshairSection;

    private VisualElement m_Header;
    private TemplateContainer m_DeathmatchHeader;
    private TemplateContainer m_DominationHeader;

    private float m_CashedHealth;
    private float m_CashedMana;

    private int m_WeaponEquipmentMaxCapacity;
    private int m_ConsumableEquipmentMaxCapacity;

    public void Init(InventoryConfig config, ItemManagedDatabase itemManagedDatabase)
    {
        m_WeaponEquipmentMaxCapacity = config.WeaponEquipmentMaxCapacity;
        m_ConsumableEquipmentMaxCapacity = config.ConsumableEquipmentMaxCapacity;

        m_Document = GetComponent<UIDocument>();
        m_Root = m_Document.rootVisualElement;
        
        // Create slots
        var weaponSlots = new Slot[m_WeaponEquipmentMaxCapacity];    
        var consumableSlots = new Slot[m_ConsumableEquipmentMaxCapacity]; 
        
        m_EquipmentContainer = m_Root.Q<VisualElement>("equipment");

        // Respawn Window
        m_HudContainer = m_Root.Q<VisualElement>("hud__container");
        m_RespawnWindowContainer = m_Root.Q<VisualElement>("respawnWindow");

        // Creating slots container
        var weaponUISlots = m_EquipmentContainer.Q<VisualElement>("WeaponSlots");
        var consumableUISlots = m_EquipmentContainer.Q<VisualElement>("ConsumableSlots");

        // Headers
        m_Header = m_Root.Q<VisualElement>("Header");
        m_DeathmatchHeader = m_Root.Q<TemplateContainer>("DeathmatchHeader");
        m_DominationHeader = m_Root.Q<TemplateContainer>("DominationHeader");

        // Winner container
        m_WinnerContainer = m_Root.Q<VisualElement>("winnerContainer");
        m_WinnerContainerTextField = m_Root.Q<Label>("WinnerText");
        m_DeathmatchWinnerTextField = m_Root.Q<Label>("DeathmatchText");
        m_DominationWinnerTextField = m_Root.Q<Label>("DominationText");

        // Death info
        m_DeadInfoWindow = m_Root.Q<VisualElement>("DeadInfoSection");
        m_DeadInfoTextField = m_Root.Q<Label>("DeadInfoText");

        // Respawn info 
        m_RespawnInfoWindow = m_Root.Q<VisualElement>("RespawnInfoSection");
        m_RespawnInfoTextField = m_Root.Q<Label>("RespawnInfoText");

        m_HelpSection = m_Root.Q<VisualElement>("HelpSection");

        // Crosshair
        m_CrosshairSection = m_Root.Q<VisualElement>("container__crosshair-section");

        m_DeathmatchPlayedRoundsTextField = m_Root.Q<Label>("PlayedRounds");
        
        m_DeathmatchRedScoreTextField = m_Root.Q<Label>("RedScore");
        m_DeathmatchBlueScoreTextField = m_Root.Q<Label>("BlueScore");
        
        // Clear
        weaponUISlots.Clear();
        consumableUISlots.Clear();

        // Unite in one array - m_Slots
        m_Slots = weaponSlots.Concat(consumableSlots).ToArray();
        int commonCapacity = m_WeaponEquipmentMaxCapacity + m_ConsumableEquipmentMaxCapacity;

        for (int i = 0; i < commonCapacity; i++) {
            var slot = weaponUISlots.CreateChild<Slot>("slot");
            slot.m_Type = SlotType.WeaponEquipmentSlot; 
            m_Slots[i] = slot;

            // Adding keys
            var keyLabel = new Label((i + 1).ToString());
            keyLabel.AddToClassList("slotKeyLabel");
            slot.Add(keyLabel);
        }

        m_Healthbar = m_Document.rootVisualElement.Q<VisualElement>("healthbar__container");
        m_Manabar = m_Document.rootVisualElement.Q<VisualElement>("manabar__container");

        m_HealthbarFill = m_Healthbar.Q<VisualElement>("healthbar__fill");
        m_ManabarFill = m_Manabar.Q<VisualElement>("manabar__fill");

        m_Tooltip = m_HudContainer.Q<VisualElement>("tooltip");
        m_TooltipText = m_Tooltip.Q<Label>("tooltipText");

        m_TimerText = m_Root.Q<Label>("TimerText");

        // Init
        HideManabar();

        // Getting data base with item icons, name etc.
        m_ItemManagedDatabase = itemManagedDatabase;

        RegisterDocument();
    }

    /// <summary>
    /// Creates Managed GameUIAssets Entity 
    /// </summary>
    private void RegisterDocument()
    {
        var em = World.DefaultGameObjectInjectionWorld.EntityManager;
        var entity = em.CreateEntity();
        em.AddComponentObject(entity, this);
    }
    
#region Public methods
    
    public void Show()
    {
        m_HudContainer.style.display = DisplayStyle.Flex;
    }

    public void Hide()
    {
        m_HudContainer.style.display = DisplayStyle.None;
    }

    public void MatchHeaderShow()
    {
        m_Header.style.display = DisplayStyle.Flex;
    }

    public void MatchHeaderHide()
    {
        m_Header.style.display = DisplayStyle.None;
    }

    public void DeadInfoShow()
    {
        m_DeadInfoWindow.style.display = DisplayStyle.Flex;
    } 

    public void SetDeadInfo(string killerName)
    {
        m_DeadInfoTextField.text = $"You have killed by {killerName}";
    }

    public void DeadInfoHide()
    {
        m_DeadInfoWindow.style.display = DisplayStyle.None;
    }

    public void SetWinnerContainer(string name)
    {
        m_WinnerContainerTextField.text = $"{name}";
    }

    public void SetWinnerContainer(TeamType team)
    {
        string teamName;

        switch (team)
        {
            case TeamType.Red:
                m_WinnerContainerTextField.AddToClassList("red-winner__text");
                m_WinnerContainerTextField.RemoveFromClassList("blue-winner__text");
                teamName = "Red team";
                break;
            case TeamType.Blue:
                m_WinnerContainerTextField.AddToClassList("blue-winner__text");
                m_WinnerContainerTextField.RemoveFromClassList("red-winner__text");
                teamName = "Blue team";
                break;
            default:
                m_WinnerContainerTextField.RemoveFromClassList("blue-winner__text");
                m_WinnerContainerTextField.RemoveFromClassList("red-winner__text");
                teamName = "No one team";
                break;
        }

        m_WinnerContainerTextField.text = $"{teamName}";
    }

    public void WinnerContainerShow()
    {
        m_WinnerContainer.style.display = DisplayStyle.Flex;
    }

    public void HelpSectionShow()
    {
        m_HelpSection.style.display = DisplayStyle.Flex;
    }

    public void HelpSectionHide()
    {
        m_HelpSection.style.display = DisplayStyle.None;
    }

    public void CrosshairHide()
    {
        m_CrosshairSection.style.display = DisplayStyle.None;
    }

    public void CrosshairShow()
    {
        m_CrosshairSection.style.display = DisplayStyle.Flex;
    }
    
    public void WinnerContainerHide()
    {
        m_WinnerContainer.style.display = DisplayStyle.None;
    }

    public void ShowRespawnInfo()
    {
        m_RespawnInfoWindow.style.display = DisplayStyle.Flex;
    }

    public void SetRespawnInfo(int seconds)
    {
        m_RespawnInfoTextField.text = $"{seconds}";
    }

    public void RespawnInfoHide()
    {
        m_RespawnInfoWindow.style.display = DisplayStyle.None;
    }

    public void SetOnMode(GameMode mode)
    {
        switch (mode)
        {
            case GameMode.Deathmatch:
                m_DeathmatchHeader.style.display = DisplayStyle.Flex;
                m_DeathmatchWinnerTextField.style.display = DisplayStyle.Flex;
                m_DominationWinnerTextField.style.display = DisplayStyle.None;
                m_DominationHeader.style.display = DisplayStyle.None;
                break;
            case GameMode.Domination:
                m_DeathmatchHeader.style.display = DisplayStyle.None;
                m_DeathmatchWinnerTextField.style.display = DisplayStyle.None;
                m_DominationWinnerTextField.style.display = DisplayStyle.Flex;
                m_DominationHeader.style.display = DisplayStyle.Flex;
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Updates equipment slots in HUD for each player.
    /// If item isn't null and has ID set slot with DTO.
    /// Else just set slot with DEFAULT values.
    /// </summary>
    public void RefreshHudEquipment(
        EntityManager em,
        Entity container, 
        DynamicBuffer<CharacterEquipment> buffer
    )
    {
        for (int i = 0; i < buffer.Length; i++)
        {
            var item = buffer[i].Item;
            var qty = buffer[i].Quantity;

            var dataForSlot = new SlotViewData();

            if (item != Entity.Null)
            {
                ItemId itemId = em.GetComponentData<CurrentItemId>(item).Value;
                
                dataForSlot = new SlotViewData
                {
                    Owner = container,
                    ItemId = itemId,
                    Sprite = m_ItemManagedDatabase.Get(itemId).Sprite,
                    Quantity = qty
                };

                m_Slots[i].Set(dataForSlot, SlotState.Disactive);

                if (!em.HasComponent<CurrentWeaponLevel>(item))
                    continue;
                
                var level = em.GetComponentData<CurrentWeaponLevel>(item);
                
                m_Slots[i].SetColor(level.Value);
            }
            else
            {
                m_Slots[i].Set(dataForSlot, SlotState.Disactive);
                
                m_Slots[i].RemoveCurrentLevelClass();
            }
        }
    }
    
    public void SetHealthbar(float current, float max)
    {
        if (m_CashedHealth == current) return;

        float percent = Mathf.Clamp01(current / max);
        
        m_HealthbarFill.style.width = Length.Percent(percent * 100f);
        m_Healthbar.style.display = DisplayStyle.Flex;
    }

    public void SetManabar(float current, float max)
    {
        if (m_CashedMana == current) return;

        float percent = Mathf.Clamp01(current / max);
        
        m_ManabarFill.style.width = Length.Percent(percent * 100f);
        m_Manabar.style.display = DisplayStyle.Flex;
    }

    public void SelectSlot(int selectedIndex)
    {
        for (int i = 0; i < m_Slots.Length; i++)
        {
            if (i == selectedIndex)
            {
                m_Slots[selectedIndex].SlotFrame.AddToClassList("slotFrame__choosed");
            }
            else
            {
                m_Slots[i].SlotFrame.RemoveFromClassList("slotFrame__choosed");
            }
        }
    }

    public void DisplayRespawnWindow()
    {
        m_HudContainer.style.display = DisplayStyle.None;
        m_RespawnWindowContainer.style.display = DisplayStyle.Flex;
    } 

    public void HideRespawnWindow()
    {
        m_RespawnWindowContainer.style.display = DisplayStyle.None;
        m_HudContainer.style.display = DisplayStyle.Flex;
    }

    public void EquipmentShow()
    {
        m_EquipmentContainer.style.display = DisplayStyle.Flex;
    }

    public void EquipmentHide()
    {
        m_EquipmentContainer.style.display = DisplayStyle.None;
    }

    public void HealthbarShow()
    {
        m_Healthbar.style.visibility = Visibility.Visible;
    }

    public void HealthbarHide()
    {
        m_Healthbar.style.visibility = Visibility.Hidden;
    }

    public void ShowManabar()
    {
        m_Manabar.style.visibility = Visibility.Visible;
    }

    public void HideManabar()
    {
        m_Manabar.style.visibility = Visibility.Hidden;
    }

    public void TogglePickupTooltip(GameplayDataForHudTooltip data, bool isVisible)
    {
        if (isVisible)
        {
            // TODO: Information about collecatable Item
            m_TooltipText.text = $"[E] Pickup {m_ItemManagedDatabase.Get(data.Id).Name}";
            m_Tooltip.style.display = DisplayStyle.Flex;
        }
        else
        {
            m_Tooltip.style.display = DisplayStyle.None;
        }
    }
    
    public void ToggleInteractionTooltip(bool isVisible)
    {
        if (isVisible)
        {
            m_Tooltip.style.display = DisplayStyle.Flex;
        }
        else
        {
            m_Tooltip.style.display = DisplayStyle.None;
        }
    }

    public void UpdateFinishRoundTimer(int seconds)
    {
        m_TimerText.text = seconds.ToString();
    }

    public void UpdatePlayedRounds( int maxRounds, int playedRounds)
    {
        m_DeathmatchPlayedRoundsTextField.text = $"{playedRounds}/{maxRounds}";
    }

    public void UpdateDeathmatchStatistics(int redWinds, int blueWins)
    {
        m_DeathmatchRedScoreTextField.text = $"{redWinds}";
        m_DeathmatchBlueScoreTextField.text = $"{blueWins}";
    }

    #endregion
}


    // teams.BluePlayersWins, 
    // teams.RedPlayersWins, 
    // deathmachSettings.RoundsNumber,
    // playedRoundsNumber