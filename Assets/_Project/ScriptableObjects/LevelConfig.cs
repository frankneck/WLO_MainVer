using System.Collections.Generic;
using Unity.Entities.Serialization;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelConfig", menuName = "Scriptable Objects/LevelConfig")]
public class LevelConfig : ScriptableObject
{
    public List<EntitySceneReference> Scenes;
}
