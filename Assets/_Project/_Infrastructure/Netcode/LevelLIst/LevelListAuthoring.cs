#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;
using Unity.Entities;
using Unity.Entities.Serialization;
using UnityEngine;

#if UNITY_EDITOR
public class LevelListAuthoring : MonoBehaviour
{
    [Header("Scenes list")]
    [SerializeField] LevelListSO _levelList;

    class Baker : Baker<LevelListAuthoring>
    {
        public override void Bake(LevelListAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            var buffer = AddBuffer<LevelListData>(entity);
            
            var scenes = authoring._levelList.LevelList;
            for (int i = 0; i < scenes.Count; ++i)
            {
                buffer.Add(new LevelListData
                {
                    // level number isn't index. Level numbers start from 0 to n
                    LevelNumber = i, 
                    Scene = new EntitySceneReference(scenes[i])
                });
            }
        }
    }
}
#endif 