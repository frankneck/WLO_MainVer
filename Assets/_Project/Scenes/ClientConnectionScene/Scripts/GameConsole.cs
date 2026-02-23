using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class GameConsole : MonoBehaviour
{
    public static GameConsole instance;

    [SerializeField] int maxMessages = 1000;

    private UIDocument document;

    private readonly ConcurrentQueue<LogEntry> inbox = new ConcurrentQueue<LogEntry>();
    private readonly List<LogEntry> messages = new List<LogEntry>();

    private VisualElement consoleVE;
    private ListView logListView;

    private bool _consoleVisible;
    public bool consoleVisible
    {
        get => _consoleVisible;
        set
        {
            _consoleVisible = value;
            consoleVE.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }

    private EntityQuery menuQuery;

    class LogEntry
    {
        public string message;
        public string stackTrace;
        public LogType type;
        public DateTime time;
    }

    void Awake()
    {
        if(instance != null) { Destroy(gameObject); return; }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnEnable()
    {
        // Getting current state component query
        var em = World.DefaultGameObjectInjectionWorld.EntityManager;

        document = GetComponent<UIDocument>();
        consoleVE = document.rootVisualElement.Query<VisualElement>("console");
        logListView = consoleVE.Query<ListView>("logList");
        ListViewInitialize();

        if (PlayerInput.GameConsole != null)
            PlayerInput.GameConsole.performed += OnConsolePerformed;

        Application.logMessageReceivedThreaded += HandleLogThreaded;
        consoleVisible = false;

    }

    void ToggleConsole()
    {
        consoleVisible = !consoleVisible;
    }

    void OnDisable()
    {
        if (PlayerInput.GameConsole != null)
            PlayerInput.GameConsole.performed -= OnConsolePerformed;
            
        Application.logMessageReceivedThreaded -= HandleLogThreaded;
    }

    void OnConsolePerformed(InputAction.CallbackContext ctx) => ToggleConsole();

    void HandleLogThreaded(string condition, string stackTrace, LogType type)
    {
        // Помещаем в потокобезопасную очередь
        var entry = new LogEntry
        {
            message = condition,
            stackTrace = stackTrace,
            type = type,
            time = DateTime.Now
        };
        inbox.Enqueue(entry);
    }

    void ListViewInitialize()
    {
        logListView.makeItem = () =>
        {
            var label = new Label();
            label.name = "logLabel";
            label.style.marginBottom = new StyleLength(0f); label.style.marginLeft = new StyleLength(0f); label.style.marginRight = new StyleLength(0f); label.style.marginTop = new StyleLength(0f);
            label.style.textOverflow = TextOverflow.Ellipsis;
            label.style.whiteSpace = WhiteSpace.NoWrap;
            return label;
        };

        logListView.bindItem = (ve, index) =>
        {
            var label = (Label)ve;
            var entry = messages[index];
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
                    label.style.color = new Color(0.9f, 0.9f, 0.9f);
                    break;
            }
        };

        logListView.itemsSource = messages;
    }

    void Update()
    {
        bool added = false;
        while (inbox.TryDequeue(out var entry))
        {
            messages.Add(entry);
            added = true;
            // Ограничение по количеству
            if (messages.Count > maxMessages)
                messages.RemoveRange(0, messages.Count - maxMessages);
        }

        if (added)
        {
            logListView.Rebuild();
            if (messages.Count > 0)
            {
                logListView.ScrollToItem(messages.Count - 1);
            }
        }
    }
}