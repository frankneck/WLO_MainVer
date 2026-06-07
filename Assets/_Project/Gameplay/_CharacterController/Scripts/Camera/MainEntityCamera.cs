using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public struct MainCameraEntity : IComponentData
{
    public Entity Character;
}

// public class MainCamera : IComponentData
// {
//     public Camera Camera;
// }
