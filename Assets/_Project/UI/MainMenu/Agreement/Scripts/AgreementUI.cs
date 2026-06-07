using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class AgreementUI : MonoBehaviour
{
    private static bool isDisplayed; 
    private string AgreeButton_ElementName = "agreeButton";
    private string ExitButton_ElementName = "exitButton";
    private string AgreementContainer_ElementName = "agreementContainer";

    private UIDocument document;
    private VisualElement root;
    private VisualElement container;

    private Button m_AgreeButton;
    private Button m_ExitButton;

    void Awake()
    {
        if (isDisplayed)
        {
            gameObject.SetActive(false);
            return;
        }

        isDisplayed = true;
    }

    void Start()
    {
        document = GetComponent<UIDocument>();
        root = document.rootVisualElement;
        
        // Start state
        container = root.Q<VisualElement>(AgreementContainer_ElementName);
        container.style.display = DisplayStyle.Flex;

        m_AgreeButton = root.Q<Button>(AgreeButton_ElementName);
        m_ExitButton = root.Q<Button>(ExitButton_ElementName);
    
        m_AgreeButton.clicked += () => HideWindow();
        m_ExitButton.clicked += () => ExitFromGame();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatic()
    {
        isDisplayed = false;
    }

    void HideWindow()
    {
        container.style.display = DisplayStyle.None;
        gameObject.SetActive(false);
    }

    void ExitFromGame()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }
}
