using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ConsoleWindow))]
public class ConsoleEditor : Editor
{
    public const string Version = "1.0";

    private SerializedProperty _assertColor;
    private SerializedProperty _colorizeOutput;

    private SerializedProperty _errorColor;

    private SerializedProperty _exceptionColor;

    private bool _inInputMode;

    private SerializedProperty _logColor;

    private GameObject _myTarget;

    private SerializedProperty _scrollBottom;
    private SerializedProperty _showAssert;
    private bool _showColorEditor;
    private SerializedProperty _showError;
    private SerializedProperty _showException;

    private bool _showHotkeyEditor;
    private SerializedProperty _showLog;
    private SerializedProperty _showOnMessage;
    private bool _showOutputEditor;
    private SerializedProperty _showWarning;
    private SerializedProperty _tagType;
    private SerializedProperty _timestampLog;
    
    private SerializedProperty _toggleConsoleHotkey;

    private SerializedProperty _warningColor;

    private void OnEnable()
    {
        _myTarget = Selection.activeGameObject;
        
        _toggleConsoleHotkey = serializedObject.FindProperty("ToggleConsoleKey");
        _assertColor = serializedObject.FindProperty("AssertColor");
        _errorColor = serializedObject.FindProperty("ErrorColor");
        _exceptionColor = serializedObject.FindProperty("ExceptionColor");
        _warningColor = serializedObject.FindProperty("WarningColor");
        _logColor = serializedObject.FindProperty("LogColor");

        _showAssert = serializedObject.FindProperty("_showAssert");
        _showError = serializedObject.FindProperty("_showError");
        _showException = serializedObject.FindProperty("_showException");
        _showWarning = serializedObject.FindProperty("_showWarning");
        _showLog = serializedObject.FindProperty("_showLog");

        _scrollBottom = serializedObject.FindProperty("ScrollBottom");
        _showOnMessage = serializedObject.FindProperty("ShowOnMessage");
        _tagType = serializedObject.FindProperty("TagType");
        _colorizeOutput = serializedObject.FindProperty("Colorize");
        _timestampLog = serializedObject.FindProperty("_timestampLog");
    }

    private void OnSceneGUI()
    {
        if (!_inInputMode) return;

        // "lock" selection to this object.
        Selection.activeGameObject = _myTarget;

        var e = Event.current;
        if (e.type != EventType.KeyDown || !e.isKey) return;
        var enumIndex = 0;
        for (var index = 0; index < _toggleConsoleHotkey.enumNames.Length; index++)
        {
            var enumName = _toggleConsoleHotkey.enumNames[index];
            if (string.Equals(enumName, e.keyCode.ToString(), StringComparison.CurrentCultureIgnoreCase))
                enumIndex = index;
        }
        _toggleConsoleHotkey.enumValueIndex = enumIndex;

        serializedObject.ApplyModifiedProperties();
        Repaint();
        _inInputMode = false;

        Event.current.Use();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var consoleWindow = target as ConsoleWindow;
        if (consoleWindow == null)
            return;

        EditorGUILayout.LabelField("Consolero Pro v" + Version, new GUIStyle("Label")
        {
            fontSize = 16,
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold,
            normal = new GUIStyleState
            {
                textColor = Color.white
            }
        }, GUILayout.Height(24));

        // ReSharper disable once AssignmentInConditionalExpression
        if (_showHotkeyEditor = EditorGUILayout.Foldout(_showHotkeyEditor, "Hotkey Settings"))
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.PropertyField(_toggleConsoleHotkey);
            if (!_inInputMode)
                if (GUILayout.Button(new GUIContent("Set", "Set from input")))
                    _inInputMode = true;

            EditorGUILayout.EndHorizontal();

            var ctrlMod = EditorGUILayout.Toggle(new GUIContent("CTRL", "Has to hold down CTRL with key"),
                (consoleWindow.ToggleConsoleModifiers & EventModifiers.Control) != 0);
            if ((consoleWindow.ToggleConsoleModifiers & EventModifiers.Control) != 0 != ctrlMod)
                if (ctrlMod)
                    consoleWindow.ToggleConsoleModifiers |= EventModifiers.Control;
                else
                    consoleWindow.ToggleConsoleModifiers &= ~EventModifiers.Control;

            var altMod = EditorGUILayout.Toggle(new GUIContent("ALT", "Has to hold down ALT with key"),
                (consoleWindow.ToggleConsoleModifiers & EventModifiers.Alt) != 0);
            if ((consoleWindow.ToggleConsoleModifiers & EventModifiers.Alt) != 0 != altMod)
                if (altMod)
                    consoleWindow.ToggleConsoleModifiers |= EventModifiers.Alt;
                else
                    consoleWindow.ToggleConsoleModifiers &= ~EventModifiers.Alt;

            var shiftMod = EditorGUILayout.Toggle(new GUIContent("SHIFT", "Has to hold down SHIFT with key"),
                (consoleWindow.ToggleConsoleModifiers & EventModifiers.Shift) != 0);

            if ((consoleWindow.ToggleConsoleModifiers & EventModifiers.Shift) != 0 != shiftMod)
                if (shiftMod)
                    consoleWindow.ToggleConsoleModifiers |= EventModifiers.Shift;
                else
                    consoleWindow.ToggleConsoleModifiers &= ~EventModifiers.Shift;
            EditorGUILayout.HelpBox("UNITY HOTKEYS MAY TRIGGER",
                MessageType.Warning);
        }

        // ReSharper disable once AssignmentInConditionalExpression
        if (_showColorEditor = EditorGUILayout.Foldout(_showColorEditor, "Color Settings"))
        {
            EditorGUILayout.PropertyField(_assertColor);
            EditorGUILayout.PropertyField(_errorColor);
            EditorGUILayout.PropertyField(_exceptionColor);
            EditorGUILayout.PropertyField(_warningColor);
            EditorGUILayout.PropertyField(_logColor);
        }

        // ReSharper disable once AssignmentInConditionalExpression
        if (_showOutputEditor = EditorGUILayout.Foldout(_showOutputEditor, "Output Settings"))
        {
            GUILayout.BeginHorizontal("box");
            GUILayout.Space(20);
            GUILayout.BeginVertical();

            var oldContentColor = GUI.contentColor;
            GUI.contentColor = _assertColor.colorValue;
            EditorGUILayout.PropertyField(_showAssert, new GUIContent("Output Asserts", "Show asserts in console"));

            GUI.contentColor = _errorColor.colorValue;
            EditorGUILayout.PropertyField(_showError, new GUIContent("Output Errors", "Show errors in console"));

            GUI.contentColor = _exceptionColor.colorValue;
            EditorGUILayout.PropertyField(_showException,
                new GUIContent("Output Exceptions", "Show exceptions in console"));

            GUI.contentColor = _warningColor.colorValue;
            EditorGUILayout.PropertyField(_showWarning, new GUIContent("Output Warnings", "Show warnings in console"));

            GUI.contentColor = _logColor.colorValue;
            EditorGUILayout.PropertyField(_showLog, new GUIContent("Output Log", "Show log in console"));

            GUI.contentColor = oldContentColor;

            GUILayout.EndVertical();
            GUILayout.Space(20);
            GUILayout.EndHorizontal();
        }

        EditorGUILayout.PropertyField(_scrollBottom,
            new GUIContent("Auto-Scroll",
                "If enabled the console will auto-scroll to the bottom when new messages are received"));

        EditorGUILayout.PropertyField(_showOnMessage,
            new GUIContent("Auto-Show",
                "If enabled the console will auto-show when new messages are received"));

        EditorGUILayout.PropertyField(_tagType,
            new GUIContent("Show Tags",
                "Shows [TAG] in front of the message"));

        EditorGUILayout.PropertyField(_colorizeOutput,
            new GUIContent("Color output",
                "Shows messages in color depending on the LogType"));

        EditorGUILayout.PropertyField(_timestampLog,
            new GUIContent("Show timestamp",
                "Shows timestamps in front of the message"));

        serializedObject.ApplyModifiedProperties();


        /*serializedObject.Update();
        EditorGUILayout.PropertyField(lookAtPoint);
        serializedObject.ApplyModifiedProperties();
        if (lookAtPoint.vector3Value.y > (target as LookAtPoint).transform.position.y)
        {
            EditorGUILayout.LabelField("(Above this object)");
        }
        if (lookAtPoint.vector3Value.y < (target as LookAtPoint).transform.position.y)
        {
            EditorGUILayout.LabelField("(Below this object)");
        }*/
    }
}