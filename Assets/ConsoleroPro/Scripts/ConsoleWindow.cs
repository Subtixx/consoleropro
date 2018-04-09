using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// TODO: Make option to only use console but no commands
// TODO: Default Position
// TODO: Make option to disallow showing/hiding log entries
// TODO: Custom timestamp format

public class ConsoleWindow : MonoBehaviour
{
    /*/// <summary>
    /// Instance/Singleton for ConsoleWindow
    /// </summary>
    public static ConsoleWindow Instance;*/

    /// <summary>
    ///     Holds the key that is used to toggle the console
    /// </summary>
    public KeyCode ToggleConsoleKey;

    /// <summary>
    ///     Holds all modifiers used to toggle the console
    /// </summary>
    public EventModifiers ToggleConsoleModifiers;

    /// <summary>
    ///     Toggles the consoles visiblity
    /// </summary>
    /// <param name="forceShow">Show console regardless of state</param>
    public void ToggleConsole(bool forceShow = false)
    {
        if (forceShow)
        {
            _isVisible = true;
        }
        else
        {
            _isVisible = !_isVisible;
            if (!_isVisible)
                _optionsIsVisible = false;
        }
    }

    public void Log(LogType type, string format, object obj1)
    {
        Log(type, string.Format(format, obj1));
    }

    public void Log(LogType type, string format, object obj1, object obj2)
    {
        Log(type, string.Format(format, obj1));
    }

    public void Log(LogType type, string format, object obj1, object obj2, object obj3)
    {
        Log(type, string.Format(format, obj1, obj2, obj3));
    }

    public void Log(LogType type, string format, params object[] arg)
    {
        Log(type, string.Format(format, arg));
    }

    /// <summary>
    ///     Logs a message to console
    /// </summary>
    /// <param name="message">The message to log</param>
    /// <param name="type">The type to log</param>
    public void Log(LogType type, string message)
    {
        if (ShowOnMessage && !_isVisible)
            _isVisible = true;

        var msg = new Message
        {
            InnerMessage = message,
            Timestamp = DateTime.Now,
            Type = type
        };
        AddMessage(msg);
        _messages.Add(msg);

        if (ScrollBottom)
            _scrollPosition = new Vector2(_scrollPosition.x, Mathf.Infinity);
    }

    /// <summary>
    ///     Adds a message to the console
    ///     Handles based settings
    /// </summary>
    /// <param name="message">The message to add</param>
    private void AddMessage(Message message)
    {
        if (message.Type == LogType.Error && !ShowError)
            return;
        if (message.Type == LogType.Assert && !ShowAssert)
            return;
        if (message.Type == LogType.Exception && !ShowException)
            return;
        if (message.Type == LogType.Log && !ShowLog)
            return;
        if (message.Type == LogType.Warning && !ShowWarning)
            return;

        if (_traceLog != "")
            _traceLog += "\n";

        if (Colorize)
        {
            string color;
            switch (message.Type)
            {
                case LogType.Error:
                    color = ColorUtility.ToHtmlStringRGBA(ErrorColor);
                    break;
                case LogType.Assert:
                    color = ColorUtility.ToHtmlStringRGBA(AssertColor);
                    break;
                case LogType.Exception:
                    color = ColorUtility.ToHtmlStringRGBA(ExceptionColor);
                    break;
                case LogType.Log:
                    color = ColorUtility.ToHtmlStringRGBA(LogColor);
                    break;
                case LogType.Warning:
                    color = ColorUtility.ToHtmlStringRGBA(WarningColor);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (_timestampLog)
                _traceLog += string.Format("<color=#{0}>[{1:H:mm:ss}]</color> ", color, message.Timestamp);
            if (TagType)
                _traceLog += string.Format("<color=#{0}>[{1}]</color> ", color, message.Type.ToString().ToUpper());
            _traceLog += string.Format("<color=#{0}>{1}</color>", color, message.InnerMessage);
        }
        else
        {
            if (_timestampLog)
                _traceLog += string.Format("[{0:H:mm:ss}]</color> ", message.Timestamp);
            if (TagType)
                _traceLog += string.Format("[{0}]</color> ", message.Type.ToString().ToUpper());
            _traceLog += string.Format("{0}", message.InnerMessage);
        }
    }

    /// <summary>
    ///     Refreshes the console based on the settings
    /// </summary>
    private void RefreshMessages()
    {
        _traceLog = "";
        foreach (var message in _messages)
            AddMessage(message);
    }

    /// <summary>
    ///     Unity Event to draw our GUI
    /// </summary>
    private void OnGUI()
    {
        var e = Event.current;

        if (e.type == EventType.KeyDown)
        {
            if (e.keyCode == ToggleConsoleKey && e.modifiers == ToggleConsoleModifiers)
                ToggleConsole();

            // Unity being fucking stupid again and sending a keyboard event with key "none" for RETURN / ENTER key.
            if (GUI.GetNameOfFocusedControl() == "consoleInput" &&
                Event.current.Equals(Event.KeyboardEvent("None")))
                OnSendClick();
        }

        if (_optionsIsVisible)
            _optionWindowRect = GUI.Window(1, _optionWindowRect, OnOptionWindow, "Consolero Pro Options");

        if (_isVisible)
        {
            var oldColor = GUI.backgroundColor;
            GUI.backgroundColor = Color.black;
            _windowRect = GUI.Window(0, _windowRect, OnConsoleWindow, "Consolero Pro v" + ConsoleEditor.Version);
            GUI.backgroundColor = oldColor;
        }
    }

    /// <summary>
    ///     Unity Event to init this script
    /// </summary>
    private void Awake()
    {
        Initialize();

        /*if (Instance != null)
            throw new Exception("Only one console window component allowed!");
        Instance = this;*/
    }

    /// <summary>
    ///     Unity Event to enable this script
    /// </summary>
    private void OnEnable()
    {
        Initialize();

        Application.logMessageReceived += Log;
    }

    /// <summary>
    ///     Unity Event to disable this script
    /// </summary>
    private void OnDisable()
    {
        Application.logMessageReceived -= Log;
    }

    private void Initialize()
    {
        _messages = new List<Message>();
        //CommandMgr = new CommandManager<ConsoleCommand, ConsoleCommandFunc>();

        CommandMgr.Add(new ConsoleCommand("hide", "", "Hides the console window", (command, args) =>
        {
            _isVisible = false;
            return CommandResult.Ok;
        }));

        CommandMgr.Add(new ConsoleCommand("help", "", "Shows help", (command, args) =>
        {
            if (args.Count == 0)
            {
                Log(LogType.Log, "Available commands");
                foreach (var cmd in CommandMgr.Commands.Values.OrderBy(a => a.Name))
                    Log(LogType.Log, string.Format("\t{0} - {1}", cmd.Name, cmd.Description));
            }
            else
            {
                var cmd = CommandMgr.GetCommand(args[0]);
                if (cmd == null)
                    Log(LogType.Error, "[CMD] Invalid command.");
                else
                    Log(LogType.Log, string.Format("\t{0} {1} - {2}", cmd.Name, cmd.Usage, cmd.Description));
            }
            return CommandResult.Ok;
        }));

        _traceLog = "";
    }

    /// <summary>
    ///     Unity Logging handler
    /// </summary>
    /// <param name="condition"></param>
    /// <param name="stacktrace"></param>
    /// <param name="type"></param>
    private void Log(string condition, string stacktrace, LogType type)
    {
        Log(type, condition);
    }

    private void OnSendClick()
    {
        var inputSplit = _consoleInput.Split(' ');
        if (inputSplit.Length >= 1)
        {
            var cmd = CommandMgr.GetCommand(inputSplit[0]);
            if (cmd != null)
            {
                _consoleInput = _consoleInput.Remove(0, inputSplit[0].Length);
                var result = cmd.Func(inputSplit[0], CommandManager.ParseLine(_consoleInput));
                switch (result)
                {
                    case CommandResult.InvalidArgument:
                        Log(LogType.Warning, "[CMD] Usage: " + cmd.Name + " " + cmd.Usage);
                        break;
                    case CommandResult.Fail:
                        Log(LogType.Error, "[CMD] Unknown error");
                        break;
                }
            }
            else
            {
                Log(LogType.Log, "[CMD] Invalid command: " + _consoleInput);
            }
        }
        else
        {
            Log(LogType.Log, "[CMD] Invalid command: " + _consoleInput);
        }

        _consoleInput = "";
    }

    private void OnConsoleWindow(int windowId)
    {
        GUI.DragWindow(new Rect(0, 0, _windowRect.width - 43, 24));

        GUILayout.BeginArea(new Rect(10, 20, _windowRect.width - 20, 30));
        var oldContentColor = GUI.contentColor;
        GUI.contentColor = !ShowError ? Color.gray : ErrorColor;
        if (GUI.Button(new Rect(10, 0, 80, 24), "[ERROR]"))
            ShowError = !ShowError;

        GUI.contentColor = !ShowAssert ? Color.gray : AssertColor;
        if (GUI.Button(new Rect(100, 0, 80, 24), "[ASSERT]"))
            ShowAssert = !ShowAssert;

        GUI.contentColor = !ShowException ? Color.gray : ExceptionColor;
        if (GUI.Button(new Rect(190, 0, 100, 24), "[EXCEPTION]"))
            ShowException = !ShowException;

        GUI.contentColor = !ShowWarning ? Color.gray : WarningColor;
        if (GUI.Button(new Rect(300, 0, 80, 24), "[WARNING]"))
            ShowWarning = !ShowWarning;

        GUI.contentColor = !ShowLog ? Color.gray : LogColor;
        if (GUI.Button(new Rect(390, 0, 80, 24), "[LOG]"))
            ShowLog = !ShowLog;

        GUI.contentColor = oldContentColor;
        GUILayout.EndArea();

        GUILayout.BeginArea(new Rect(10, 54, _windowRect.width - 20, _windowRect.height - 94));
        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, GUILayout.Width(_windowRect.width - 20),
            GUILayout.Height(_windowRect.height - 104));

        GUILayout.Label(_traceLog, new GUIStyle("Label")
        {
            richText = true
        });

        GUILayout.EndScrollView();
        GUILayout.EndArea();

        if (GUI.Button(new Rect(_windowRect.width - 20, 1, 16, 16), new GUIContent("x", "Close")))
        {
            _isVisible = false;
            _optionsIsVisible = false;
        }

        if (GUI.Button(new Rect(_windowRect.width - 42, 1, 20, 16), new GUIContent("o", "Options")))
            _optionsIsVisible = !_optionsIsVisible;

        GUI.SetNextControlName("consoleInput");
        _consoleInput = GUI.TextField(new Rect(10, _windowRect.height - 40, _windowRect.width - 80, 24), _consoleInput);
        if (GUI.Button(new Rect(_windowRect.width - 60, _windowRect.height - 40, 48, 24), "Send"))
            OnSendClick();

        _windowRect = ResizeWindow(_windowRect, ref _isResizing, ref _windowResizeStart, _minWindowSize);
    }

    private void OnOptionWindow(int windowId)
    {
        GUI.DragWindow(new Rect(0, 0, _optionWindowRect.width - 43, 24));

        if (GUI.Button(new Rect(_optionWindowRect.width - 20, 1, 16, 16), new GUIContent("x", "Close")))
            _optionsIsVisible = false;

        GUILayout.BeginArea(new Rect(10, 20, _optionWindowRect.width - 20, _optionWindowRect.height - 20));
        TimestampLog = GUI.Toggle(new Rect(10, 10, _optionWindowRect.width - 20, 24), TimestampLog, "Show Timestamps");
        ScrollBottom = GUI.Toggle(new Rect(10, 34, _optionWindowRect.width - 20, 24), ScrollBottom, "Auto scroll");
        TagType = GUI.Toggle(new Rect(10, 58, _optionWindowRect.width - 20, 24), TagType, "Show tags");
        ShowOnMessage = GUI.Toggle(new Rect(10, 82, _optionWindowRect.width - 20, 24), ShowOnMessage,
            "Auto show on new message");
        ShowWarning = GUI.Toggle(new Rect(10, 106, _optionWindowRect.width - 20, 24), ShowWarning, "Show warnings");
        ShowAssert = GUI.Toggle(new Rect(10, 130, _optionWindowRect.width - 20, 24), ShowAssert, "Show asserts");
        ShowError = GUI.Toggle(new Rect(10, 154, _optionWindowRect.width - 20, 24), ShowError, "Show errors");
        ShowException = GUI.Toggle(new Rect(10, 178, _optionWindowRect.width - 20, 24), ShowException,
            "Show exception");
        ShowLog = GUI.Toggle(new Rect(10, 202, _optionWindowRect.width - 20, 24), ShowLog, "Show log");
        GUILayout.EndArea();

        _optionWindowRect = ResizeWindow(_optionWindowRect, ref _isOptionResizing, ref _windowOptionResizeStart,
            _minOptionWindowSize);
    }

    private static Rect ResizeWindow(Rect windowRect, ref bool isResizing, ref Rect resizeStart, Vector2 minWindowSize)
    {
        if (_styleWindowResize == null)
            _styleWindowResize = new GUIStyle
            {
                name = "WindowResizer",
                contentOffset = new Vector2(7, 5),
                normal = new GUIStyleState
                {
                    textColor = Color.white
                },
                active = new GUIStyleState
                {
                    textColor = Color.gray
                }
            };

        var mouse = GUIUtility.ScreenToGUIPoint(new Vector2(Input.mousePosition.x,
            Screen.height - Input.mousePosition.y));
        //var r = GUILayoutUtility.GetRect( gcDrag, styleWindowResize );
        var r = new Rect(windowRect.width - 20, windowRect.height - 20, 20, 20);
        if (Event.current.type == EventType.MouseDown && r.Contains(mouse))
        {
            isResizing = true;
            resizeStart = new Rect(mouse.x, mouse.y, windowRect.width, windowRect.height);
            //Event.current.Use();  // the GUI.Button below will eat the event, and this way it will show its active state
        }
        else if (Event.current.type == EventType.MouseUp && isResizing)
        {
            isResizing = false;
        }
        else if (!Input.GetMouseButton(0))
        {
            // if the mouse is over some other window we won't get an event, this just kind of circumvents that by checking the button state directly
            isResizing = false;
        }
        else if (isResizing)
        {
            windowRect.width = Mathf.Max(minWindowSize.x, resizeStart.width + (mouse.x - resizeStart.x));
            windowRect.height = Mathf.Max(minWindowSize.y, resizeStart.height + (mouse.y - resizeStart.y));
            windowRect.xMax = Mathf.Min(Screen.width, windowRect.xMax); // modifying xMax affects width, not x
            windowRect.yMax = Mathf.Min(Screen.height, windowRect.yMax); // modifying yMax affects height, not y
        }
        GUI.Button(r, GcDrag, _styleWindowResize);
        return windowRect;
    }

    private struct Message
    {
        public DateTime Timestamp;
        public string InnerMessage;
        public LogType Type;
    }

    #region Commands

    public CommandManager<ConsoleCommand, ConsoleCommandFunc> CommandMgr =
        new CommandManager<ConsoleCommand, ConsoleCommandFunc>();

    public class ConsoleCommand : Command<ConsoleCommandFunc>
    {
        public ConsoleCommand(string name, string usage, string description, ConsoleCommandFunc func)
            : base(name, usage, description, func)
        {
        }
    }

    public delegate CommandResult ConsoleCommandFunc(string command, IList<string> args);

    #endregion

    #region Private Fields

    private static GUIStyle _styleWindowResize;
    private static readonly GUIContent GcDrag = new GUIContent("//", "drag to resize");
    private readonly Vector2 _minOptionWindowSize = new Vector2(250, 250);
    private readonly Vector2 _minWindowSize = new Vector2(500, 250);

    /// <summary>
    ///     Holds the content of the "input" cmd line
    /// </summary>
    private string _consoleInput = "";

    /// <summary>
    ///     User trying to resize option window
    /// </summary>
    private bool _isOptionResizing;

    /// <summary>
    ///     User trying to resize console window
    /// </summary>
    private bool _isResizing;

    /// <summary>
    ///     Wether or not the window is visible
    /// </summary>
    private bool _isVisible = true;

    /// <summary>
    ///     Holds all messages in list so we can add them with different styles
    /// </summary>
    private List<Message> _messages;

    private bool _optionsIsVisible;

    private Rect _optionWindowRect = new Rect(20, 20, 250, 250);

    /// <summary>
    ///     Current scroll position of the log
    /// </summary>
    private Vector2 _scrollPosition;

    [SerializeField] private bool _showAssert = true;
    [SerializeField] private bool _showError = true;
    [SerializeField] private bool _showException = true;
    [SerializeField] private bool _showLog = true;
    [SerializeField] private bool _showWarning = true;

    /// <summary>
    ///     Should we display timestamps on log entries?
    /// </summary>
    [SerializeField] private bool _timestampLog = true;

    /// <summary>
    ///     Holds the current messages in console
    /// </summary>
    private string _traceLog;

    private Rect _windowOptionResizeStart;

    /// <summary>
    ///     Holds the current size & position of the window
    /// </summary>
    private Rect _windowRect = new Rect(20, 20, 500, 250);

    private Rect _windowResizeStart;

    #endregion

    #region Public Settings

    /// <summary>
    ///     Color of Assert entries
    /// </summary>
    [SerializeField] public Color AssertColor = new Color(139, 0, 0);

    /// <summary>
    ///     Color of Error entries
    /// </summary>
    [SerializeField] public Color ErrorColor = new Color(255, 0, 0);

    /// <summary>
    ///     Color of Exception entries
    /// </summary>
    [SerializeField] public Color ExceptionColor = new Color(255, 140, 0);

    /// <summary>
    ///     Color of Warning entries
    /// </summary>
    [SerializeField] public Color WarningColor = new Color(128, 128, 0);

    /// <summary>
    ///     Color of Log entries
    /// </summary>
    [SerializeField] public Color LogColor = new Color(0, 100, 0);

    /// <summary>
    ///     Should the console scroll to bottom on new messages?
    /// </summary>
    [SerializeField] public bool ScrollBottom = true;

    [SerializeField] public bool ShowOnMessage = true;
    [SerializeField] public bool TagType = true;

    /// <summary>
    ///     Show colors in console
    /// </summary>
    [SerializeField] public bool Colorize = true;

    /// <summary>
    ///     Show timestamps before logs
    /// </summary>
    [SerializeField]
    public bool TimestampLog
    {
        get { return _timestampLog; }
        set
        {
            if (_timestampLog != value)
            {
                _timestampLog = value;
                RefreshMessages();
                _scrollPosition = new Vector2(_scrollPosition.x, Mathf.Infinity);
            }
        }
    }

    /// <summary>
    ///     Show Warning in console
    /// </summary>
    [SerializeField]
    public bool ShowWarning
    {
        get { return _showWarning; }
        set
        {
            if (_showWarning == value) return;
            _showWarning = value;
            RefreshMessages();
            _scrollPosition = new Vector2(_scrollPosition.x, Mathf.Infinity);
        }
    }

    /// <summary>
    ///     Show Log in console
    /// </summary>
    [SerializeField]
    public bool ShowLog
    {
        get { return _showLog; }
        set
        {
            if (_showLog == value) return;
            _showLog = value;
            RefreshMessages();
            _scrollPosition = new Vector2(_scrollPosition.x, Mathf.Infinity);
        }
    }

    /// <summary>
    ///     Show Exceptions in console
    /// </summary>
    [SerializeField]
    public bool ShowException
    {
        get { return _showException; }
        set
        {
            if (_showException == value) return;
            _showException = value;
            RefreshMessages();
            _scrollPosition = new Vector2(_scrollPosition.x, Mathf.Infinity);
        }
    }

    /// <summary>
    ///     Show Asserts in console
    /// </summary>
    [SerializeField]
    public bool ShowAssert
    {
        get { return _showAssert; }
        set
        {
            if (_showAssert == value) return;
            _showAssert = value;
            RefreshMessages();
            _scrollPosition = new Vector2(_scrollPosition.x, Mathf.Infinity);
        }
    }

    /// <summary>
    ///     Show Errors in console
    /// </summary>
    [SerializeField]
    public bool ShowError
    {
        get { return _showError; }
        set
        {
            if (_showError == value) return;
            _showError = value;
            RefreshMessages();
            _scrollPosition = new Vector2(_scrollPosition.x, Mathf.Infinity);
        }
    }

    #endregion
}