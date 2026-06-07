// using System.Collections;
// using System.Collections.Generic;
// using Unity.Entities;
// using Unity.Mathematics;
// using Unity.Transforms;
// using Unity.VisualScripting;
// using UnityEngine;
// using UnityEngine.SceneManagement;
// using UnityEngine.UIElements;

// public class NewWorldSpaceUIController : MonoBehaviour
// {
//     [Header("World UI Properties")]
//     [SerializeField] private float HeightOffset;
//     [SerializeField] private GameObject PlayerInfoPrefab;
//     [SerializeField] private string SceneName; 
    
//     [Header("UI Elements")]
//     [SerializeField] private string HealthiFill_ElementName; 
//     [SerializeField] private string NameText_ElementName; 

//     private float3 _cashedPos;
//     private float _casehdHealth;
//     private string _cashedPlayerName;


//     void SpawnWorldUIForEntity(float3 position, Entity entity, EntityCommandBuffer ecb)
//     {
//         // spawn only once
//         if (_worldUIMap.ContainsKey(entity))
//             return; 
        
//         float3 newPosition = new float3(position.x, position.y + HeightOffset, position.z);
//         GameObject playerInfo = Instantiate(PlayerInfoPrefab, newPosition, Quaternion.identity);
     
//         if (SceneName.Equals(null)) return;
     
//         Scene scene = SceneManager.GetSceneByName(SceneName);
//         SceneManager.MoveGameObjectToScene(playerInfo, scene);

//         _worldUIMap.Add(entity, playerInfo);

//         if (!playerInfo.TryGetComponent<UIDocument>(out var document)) return;
//         VisualElement fill = document.rootVisualElement.Q<VisualElement>(HealthiFill_ElementName);
//         Label playerName = document.rootVisualElement.Q<Label>(NameText_ElementName);

//         // Create Entity to proccess it in WorldSpaceUISystems. This entity will be destroyed if target deesn't exist
//         var playerInfoEntity = ecb.CreateEntity();
//         ecb.AddComponent(playerInfoEntity, playerInfo);
//         ecb.AddComponent(playerInfoEntity, new WorldUIElements
//         {
//             HealthFill = fill,
//             PlayerName = playerName 
//         });
//         ecb.AddComponent(playerInfoEntity, new WorldUIHeightOffset 
//         { 
//             Value = HeightOffset 
//         });
//         ecb.AddComponent(playerInfoEntity, new CashedWorldUIData 
//         { 
//             Position = playerInfo.transform.position,
//             Name = playerName.text,
//             FillLength = fill.style.width
//         }); 
//         ecb.AddComponent(playerInfoEntity, new WorldUITargetEntity
//         {
//             Entity = entity
//         });
//     } 

//     private void RemoveWorldUIForEntity(Entity entity)
//     {
//         Destroy(playerInfo);
//     }
// }
