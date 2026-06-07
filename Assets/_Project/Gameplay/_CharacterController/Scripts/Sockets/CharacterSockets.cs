using Unity.Entities;
using Unity.NetCode;

public struct FirstPersonCharacterSocket : IComponentData
{
    public Entity Entity;
}

public struct ThirdPersonCharacterSocket : IComponentData
{
    public Entity Entity;
}