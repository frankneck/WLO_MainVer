using UnityEngine;

public class ClientBootstrap : MonoBehaviour
{
    void OnEnable()
    {
        PlayerInput.Initialize();
    }
}
