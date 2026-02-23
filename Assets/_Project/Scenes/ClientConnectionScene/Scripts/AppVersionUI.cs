using UnityEngine;
using UnityEngine.UIElements;

public class AppVersionUI : MonoBehaviour
{
    static bool initialized = false;

    public UIDocument document;
    private Label verL;

    void Awake()
    {
        if (initialized) { Destroy(gameObject); return; }
        initialized = true;
        DontDestroyOnLoad(gameObject);
        verL = document.rootVisualElement.Query<Label>("Version");
#if !UNITY_EDITOR
        verL.text = Application.version;
#endif
    }
}
