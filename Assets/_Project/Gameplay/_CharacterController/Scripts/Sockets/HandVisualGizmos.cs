using UnityEngine;

[RequireComponent(typeof(CharacterSocketsAuthoring))]
public class HandVisualGizmos : MonoBehaviour
{
    [Header("Hand Sockets")]
    [SerializeField] private Transform RightHandSocket;
    [SerializeField] private Transform LeftHandSocket; 
    
    [Header("Parameters")]
    [SerializeField] private Vector3 Size;

    void OnDrawGizmos()
    {
        if (RightHandSocket == null || LeftHandSocket == null) return;

        Gizmos.color = Color.softRed;
        Gizmos.DrawCube(RightHandSocket.position, Size);
        Gizmos.DrawCube(LeftHandSocket.position, Size);
    }
}
