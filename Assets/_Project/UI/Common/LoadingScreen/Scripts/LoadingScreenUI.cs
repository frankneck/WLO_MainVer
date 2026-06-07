using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class LoadingScreenUI : MonoBehaviour
{
    static LoadingScreenUI instance;

    private UIDocument document;
    private VisualElement screenVE;
    private Label statusLabel;

    void Awake()
    {
        if (instance != null) 
        { 
            Destroy(gameObject); 
            return; 
        }
        
        instance = this;
        document = GetComponent<UIDocument>();
        screenVE = document.rootVisualElement.Query<VisualElement>("loadingScreen");
        statusLabel = screenVE.Query<Label>("status");
        DontDestroyOnLoad(gameObject);
        screenVE.style.display = DisplayStyle.None;
    }

    void Update()
    {
        if(screenVE.style.display == DisplayStyle.None)
            return;
        
        statusLabel.style.translate = new Translate(statusLabel.style.translate.value.x, new Length(Mathf.Sin(Time.time * 3) * 13, LengthUnit.Percent));
    }

    public static void Set(LoadingScreenState state)
    {
        if(instance == null)
            return;

        switch (state)
        {
            case LoadingScreenState.Connecting:
                instance.screenVE.style.display = DisplayStyle.Flex;
                instance.statusLabel.text = "CONNECTING";
                break;
            case LoadingScreenState.Loading:
                instance.screenVE.style.display = DisplayStyle.Flex;
                instance.statusLabel.text = "LOADING";
                break;
            case LoadingScreenState.None:
                instance.screenVE.style.display = DisplayStyle.None;
                break;
        }
    }
}

public enum LoadingScreenState
{
    None,
    Connecting,
    Loading
}