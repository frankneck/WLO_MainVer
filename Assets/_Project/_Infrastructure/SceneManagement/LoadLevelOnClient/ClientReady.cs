using Unity.NetCode;

/// <summary>
/// Marks server if all tracked levels are loaded on a client  
/// </summary>
public struct ClientReady : IRpcCommand { }