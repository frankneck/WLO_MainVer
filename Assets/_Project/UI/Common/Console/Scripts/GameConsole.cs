using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class GameConsoleWindow : MonoBehaviour, IUIView
{
    public static GameConsoleWindow Instance;

    [SerializeField] int m_MaxMessages = 1000;

    private UIDocument m_Document;

    private readonly ConcurrentQueue<LogEntry> m_Inbox = new ConcurrentQueue<LogEntry>();
    private readonly List<LogEntry> m_Messages = new List<LogEntry>();

    private VisualElement m_Root;
    private VisualElement m_ConsoleVE;
    private ListView m_LogListView;

    private bool m_ConsoleVisible;
    public bool ConsoleVisible
    {
        get => m_ConsoleVisible;
        set
        {
            m_ConsoleVisible = value;
            UpdateConsoleEnable();
        }
    }

    private EntityQuery menuQuery;

    private class LogEntry
    {
        public string message;
        public string stackTrace;
        public LogType type;
        public DateTime time;
    }

    public void Show()
    {
        m_Root.style.display = DisplayStyle.Flex;
    }

    public void Hide()
    {
        m_Root.style.display = DisplayStyle.None;
    }

    private void Awake()
    {
        if(Instance != null) 
        { 
            Destroy(gameObject); 
            return; 
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (PlayerInput.GameConsole != null)
        {
            PlayerInput.GameConsole.performed += OnConsolePerformed;
        }
    }

    private void OnEnable()
    {
        // Getting current state component query
        var em = World.DefaultGameObjectInjectionWorld.EntityManager;

        m_Document = GetComponent<UIDocument>();
        m_Root = m_Document.rootVisualElement;
        m_ConsoleVE = m_Root.Query<VisualElement>("consoleContainer");
        m_LogListView = m_ConsoleVE.Query<ListView>("logList");
        ListViewInitialize();

        Application.logMessageReceivedThreaded += HandleLogThreaded;
        
        // Start state
        ConsoleVisible = false;
    }

    private void ToggleConsole()
    {
        ConsoleVisible = !ConsoleVisible;
    }

    void OnDisable()
    {
        if (PlayerInput.GameConsole != null)
        {
            PlayerInput.GameConsole.performed -= OnConsolePerformed;
        }
            
        Application.logMessageReceivedThreaded -= HandleLogThreaded;
    }

    private void OnConsolePerformed(InputAction.CallbackContext ctx) => ToggleConsole();

    private void HandleLogThreaded(string condition, string stackTrace, LogType type)
    {
        // Помещаем в потокобезопасную очередь
        var entry = new LogEntry
        {
            message = condition,
            stackTrace = stackTrace,
            type = type,
            time = DateTime.Now
        };
        m_Inbox.Enqueue(entry);
    }

    private void ListViewInitialize()
    {
        m_LogListView.makeItem = () =>
        {
            var label = new Label();
            label.name = "logLabel";
            label.style.marginBottom = new StyleLength(0f); label.style.marginLeft = new StyleLength(0f); label.style.marginRight = new StyleLength(0f); label.style.marginTop = new StyleLength(0f);
            label.style.textOverflow = TextOverflow.Ellipsis;
            label.style.whiteSpace = WhiteSpace.NoWrap;
            return label;
        };

        m_LogListView.bindItem = (ve, index) =>
        {
            var label = (Label)ve;
            var entry = m_Messages[index];
            var prefix = entry.type.ToString() + " [" + entry.time.ToString("HH:mm:ss") + "] ";
            label.text = prefix + entry.message;
            switch (entry.type)
            {
                case LogType.Error:
                case LogType.Exception:
                case LogType.Assert:
                    label.style.color = new Color(1f, 0.6f, 0.6f);
                    break;
                case LogType.Warning:
                    label.style.color = new Color(1f, 0.8f, 0.4f);
                    break;
                default:
                    label.style.color = Color.black;
                    break;
            }
        };

        m_LogListView.itemsSource = m_Messages;
    }

    private void Update()
    {
        bool added = false;
        while (m_Inbox.TryDequeue(out var entry))
        {
            m_Messages.Add(entry);
            added = true;
            // Ограничение по количеству
            if (m_Messages.Count > m_MaxMessages)
                m_Messages.RemoveRange(0, m_Messages.Count - m_MaxMessages);
        }

        if (added)
        {
            m_LogListView.Rebuild();
            if (m_Messages.Count > 0)
            {
                m_LogListView.ScrollToItem(m_Messages.Count - 1);
            }
        }

        if (PlayerInput.AppMenu != null && PlayerInput.AppMenu.WasCompletedThisFrame())
        {
            ConsoleVisible = false;
        }
    }

    private void UpdateConsoleEnable()
    {    
        var em = World.DefaultGameObjectInjectionWorld.EntityManager;

        if (m_ConsoleVisible)
        {
            Debug.Log("Console Flex");
            m_ConsoleVE.style.display = DisplayStyle.Flex;
        }
        else
        {
            m_ConsoleVE.style.display = DisplayStyle.None;
        }
    }
}