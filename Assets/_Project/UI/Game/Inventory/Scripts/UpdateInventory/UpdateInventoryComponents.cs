using Unity.Entities;
using Unity.NetCode;

/// <summary>
/// Client Intention to change item place. Need to send RPC. 
/// It used to handle in apart system  
/// </summary>
public struct ClientSlotsArrayChanged : IComponentData
{
    // FROM
    public SlotType SourceType;
    public Entity SourceOwner;
    public int SourceIndex;
    // TO
    public SlotType TargetType;
    public Entity TargetOwner;
    public int TargetIndex;
}


/// <summary>
/// Notifies server thath item from slot array has moved 
/// </summary>
public struct ClientChangeItemPlace : IRpcCommand
{
    // FROM
    public int SourceIndex;
    public SlotType SourceType;
    public int TargetIndex;
    // TO
    public SlotType TargetType;
    public Entity SourceOwner;
    public Entity TargetOwner;
}
