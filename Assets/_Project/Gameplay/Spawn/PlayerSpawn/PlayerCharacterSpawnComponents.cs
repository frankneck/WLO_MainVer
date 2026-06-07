using Unity.Entities;
using Unity.NetCode;

/// <summary>
/// Notify the client that the player character has already spawned 
/// </summary>
public partial struct PlayerCharacterSpawned : IRpcCommand { }