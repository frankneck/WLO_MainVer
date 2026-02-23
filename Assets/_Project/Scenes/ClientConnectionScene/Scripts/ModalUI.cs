using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class ModalUI : MonoBehaviour
{
    public static ModalUI instance;

    UIDocument document;

    VisualElement screenVE;
    Label label;
    Label message;
    Button closeButton;

    void Awake()
    {
        if(instance != null) { Destroy(gameObject); return; }
        instance = this;
        DontDestroyOnLoad(gameObject);

        document = GetComponent<UIDocument>();

        screenVE = document.rootVisualElement.Query<VisualElement>("Screen");

        label = screenVE.Q<Label>("ModalLabel");
        message = screenVE.Q<Label>("Message");
        closeButton = screenVE.Q<Button>("CloseButton");

        screenVE.style.display = DisplayStyle.None;

        closeButton.clicked += () => screenVE.style.display = DisplayStyle.None;
    }

    public static void OpenModal(string labelText, string messageText)
    {
        if(instance == null) { Debug.LogWarning("[Modal UI] Can't open Modal: not instance"); return; }

        instance.label.text = labelText;
        instance.message.text = messageText;
        instance.screenVE.style.display = DisplayStyle.Flex;
    }
}
