using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class WorldSpaceUIController : MonoBehaviour, IUIView
{
    [Header("World UI Properties")]
    [SerializeField] private float m_HeightOffset;
    [SerializeField] private float m_MaxDistance;
    [SerializeField] private GameObject m_WorldSpaceHealthbarPrefab;
    [SerializeField] private string m_SceneName; 
    
    [Header("UI Elements")]
    [SerializeField] private string HealthiFill_ElementName; 
    [SerializeField] private string NameText_ElementName; 

    private Dictionary<Entity, GameObject> _worldUIMap;

    public void Init()
    {
        _worldUIMap = new();
     
        var em = World.DefaultGameObjectInjectionWorld.EntityManager;
        Entity worldSpaceController = RegisterDocumentHelper.RegisterDocument(ref em, this);
        em.AddComponentData(worldSpaceController, new WorldSpaceControllerData
        {
            MaxDistance = m_MaxDistance
        });
    }

    public void Show()
    {
        foreach (var go in _worldUIMap.Values)
        {
            if (go != null)
                go.SetActive(true);
        }
    }

    public void Hide()
    {
        foreach (var go in _worldUIMap.Values)
        {
            if (go != null)
                go.SetActive(false);
        }
    }

    public void SetHealth(Entity entity, float healthFill)
    {
        _worldUIMap.TryGetValue(entity, out GameObject playerInfo); 

        if (playerInfo == null || !playerInfo.activeSelf)
            return;
        
        if (playerInfo.TryGetComponent(out UIDocument document))
        {
            VisualElement fill = document.rootVisualElement.Q<VisualElement>(HealthiFill_ElementName);
            fill.style.width = Length.Percent(healthFill);
        }
    }

    public void SetName(Entity entity, string name)
    {
        _worldUIMap.TryGetValue(entity, out GameObject playerInfo); 

        if (playerInfo == null || !playerInfo.activeSelf)
            return;
        
        if (playerInfo.TryGetComponent(out UIDocument document))
        {
            Label nameTextField = document.rootVisualElement.Q<Label>(NameText_ElementName);
            nameTextField.text = name;
        }
    }

    public void SpawnAndSetHealthbarForEntity(
        EntityCommandBuffer ecb, 
        float3 position, 
        Entity entity
    )
    {
        // spawn only once
        if (_worldUIMap.ContainsKey(entity))
            return; 

        if (m_SceneName.Equals(null)) 
            return;
        
        float3 newPosition = new float3(position.x, position.y + m_HeightOffset, position.z);
        GameObject worldSpaceHealthbar = Instantiate(m_WorldSpaceHealthbarPrefab, newPosition, Quaternion.identity);
     
        Scene scene = SceneManager.GetSceneByName(m_SceneName);
        SceneManager.MoveGameObjectToScene(worldSpaceHealthbar, scene);

        _worldUIMap.Add(entity, worldSpaceHealthbar);

        if (!worldSpaceHealthbar.TryGetComponent<UIDocument>(out var document)) 
            return;
        
        VisualElement fill = document.rootVisualElement.Q<VisualElement>(HealthiFill_ElementName);
        Label playerName = document.rootVisualElement.Q<Label>(NameText_ElementName);

        // Create Entity to proccess it in WorldSpaceUISystems. This entity will be destroyed if target deesn't exist
        CreatePlayerInfoEntity(ecb, worldSpaceHealthbar, fill, playerName, entity);
    } 

    public void SetHealthbarOnRelationship(      
        EntityCommandBuffer ecb,
        Entity entity, 
        TeamRelationship relationship,
        Entity worldUiEntity
    )
    {
        _worldUIMap.TryGetValue(entity, out GameObject playerInfo); 

        if (playerInfo == null || !playerInfo.activeSelf)
            return;

        if (playerInfo == null)
        {
            UnityEngine.Debug.Log("[ShowWorldUIForEntity] Attention: player info equals null");
            return;
        }

        if (!playerInfo.TryGetComponent<UIDocument>(out var document)) 
            return;
    
        VisualElement fill = document.rootVisualElement.Q<VisualElement>(HealthiFill_ElementName);
        Label playerName = document.rootVisualElement.Q<Label>(NameText_ElementName);

        switch (relationship)
        {
            case TeamRelationship.Friend :
                fill.AddToClassList("friend-team__fill");
                playerName.AddToClassList("friend-team__name");
                fill.RemoveFromClassList("enemy-team__fill");
                playerName.RemoveFromClassList("enemy-team__name");
                break;
            case TeamRelationship.Enemy :
                fill.AddToClassList("enemy-team__fill");
                playerName.AddToClassList("enemy-team__name");
                fill.RemoveFromClassList("friend-team__fill");
                playerName.RemoveFromClassList("friend-team__name");
                break;
            default:
                // I don't know who is this
                break;
        }

        ecb.AddComponent<RelationShipInitialized>(worldUiEntity);
    }

    private void CreatePlayerInfoEntity(
        EntityCommandBuffer ecb, 
        GameObject go, 
        VisualElement fill, 
        Label playerName, 
        Entity targetEntity)
    {
        var playerInfoEntity = ecb.CreateEntity();

        ecb.AddComponent(playerInfoEntity, new WorldUIHeightOffset 
        { 
            Value = m_HeightOffset 
        });

        ecb.AddComponent(playerInfoEntity, new CashedWorldUITargetEntityInfo 
        { 
            Position = go.transform.position,
            Name = playerName.text,
            FillLength = fill.style.width
        }); 

        ecb.AddComponent(playerInfoEntity, new WorldUITargetEntity
        {
            Entity = targetEntity
        });

        if (!go.TryGetComponent<Transform>(out var goTransform))
            return;

        ecb.AddComponent(playerInfoEntity, goTransform);
    }

    public void RemoveWorldUIForEntity(Entity entity)
    {
        _worldUIMap.TryGetValue(entity, out GameObject playerInfo); 
        Destroy(playerInfo);
        
        _worldUIMap.Remove(entity);
    }

    public void ShowWorldUIForEntity(Entity entity)
    {
        _worldUIMap.TryGetValue(entity, out GameObject playerInfo); 

        if (playerInfo == null)
        {
            UnityEngine.Debug.Log("[ShowWorldUIForEntity] Attention: player info equals null");
            return;
        }

        if (playerInfo.TryGetComponent<UIDocument>(out var document))
        {
            if (document != null && document.rootVisualElement != null)
                document.rootVisualElement.style.display = DisplayStyle.Flex;
        }
    }

    public void HideWorldUIForEntity(Entity entity)
    {
        _worldUIMap.TryGetValue(entity, out GameObject playerInfo); 

        if (playerInfo == null)
        {
            UnityEngine.Debug.Log("[HideWorldUIForEntity] Attention: player info equals null");
            return;
        }

        if (playerInfo.TryGetComponent<UIDocument>(out var document))
        {
            if (document != null && document.rootVisualElement != null)
                document.rootVisualElement.style.display = DisplayStyle.None;
        }
    }
}


