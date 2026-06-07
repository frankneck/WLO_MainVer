using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class WorldSpaceHealthbar : MonoBehaviour
{
    private UIDocument m_Document;
    private Camera cam;

    public void Start()
    {
        m_Document = GetComponent<UIDocument>();
        cam = Camera.main;
    }

    private void LateUpdate()
    {
        if (cam.Equals(null)) 
            return;

        transform.forward = cam.transform.forward;
    }
}