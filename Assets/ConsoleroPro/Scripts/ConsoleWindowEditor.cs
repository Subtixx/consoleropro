using System.IO;
using UnityEditor;
using UnityEngine;

//[CustomEditor(typeof(ConsoleWindow))]
[CanEditMultipleObjects]
public class ConsoleWindowEditor : Editor
{
    private bool _inEditMode;

    private KeyCode _key;
    private bool _shiftModifier;
    private bool _ctrlModifier;
    private bool _altModifier;

    private GameObject _myTarget;

    private void OnEnable()
    {
        _myTarget = Selection.activeGameObject;
        var consoleWindow = target as ConsoleWindow;
        if (consoleWindow != null)
        {
            _key = consoleWindow.ToggleConsoleKey;
            _altModifier = (consoleWindow.ToggleConsoleModifiers & EventModifiers.Alt) != 0;
            _shiftModifier = (consoleWindow.ToggleConsoleModifiers & EventModifiers.Shift) != 0;
            _ctrlModifier = (consoleWindow.ToggleConsoleModifiers & EventModifiers.Control) != 0;
        }
    }

    private void OnSceneGUI()
    {
        if (!_inEditMode) return;

        // "lock" selection to this object.
        Selection.activeGameObject = _myTarget;

        var e = Event.current;
        if (e.functionKey)
        {
            Debug.Log(e.keyCode);
            if (e.keyCode == KeyCode.LeftControl ||
                e.keyCode == KeyCode.RightControl)
                _ctrlModifier = (e.type == EventType.KeyDown);

            if (e.keyCode == KeyCode.LeftAlt ||
                e.keyCode == KeyCode.RightAlt)
                _altModifier = (e.type == EventType.KeyDown);
            Repaint();
            return;
        }

        if (e.type != EventType.KeyDown || !e.isKey) return;
        Event.current.Use();

        _key = e.keyCode;
        _shiftModifier = e.shift;
        _inEditMode = false;
        Repaint();
    }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.LabelField("Consolero Pro v1.0", new GUIStyle("Label")
        {
            fontSize = 16,
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold,
            normal = new GUIStyleState()
            {
                textColor = Color.white
            }
        }, GUILayout.Height(24));

        EditorStyles.label.wordWrap = true;
        EditorGUILayout.LabelField("Notice: Unity hotkeys may trigger. Also SHIFT will only trigger after key");

        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.LabelField((_inEditMode ? "EDITING: " : "") + "Console Toggle:");
        var str = "";

        if (_ctrlModifier)
        {
            str += "CTRL";
        }

        if (_shiftModifier)
        {
            if (str != "")
                str += " + SHIFT";
            else
                str = "SHIFT";
        }
        if (_altModifier)
        {
            if (str != "")
                str += " + ALT";
            else
                str = "ALT";
        }
        if (_key != KeyCode.None)
        {
            if (str != "")
                str += " + " + _key.ToString();
            else
                str = _key.ToString();
        }

        EditorGUILayout.LabelField(str);

        if (GUILayout.Button("Edit"))
        {
            _key = KeyCode.None;
            _shiftModifier = false;
            _ctrlModifier = false;
            _altModifier = false;
            _inEditMode = true;
        }
        EditorGUILayout.EndHorizontal();

        var consoleWindow = target as ConsoleWindow;
        if (consoleWindow != null)
        {
            consoleWindow.ToggleConsoleKey = _key;
            consoleWindow.ToggleConsoleModifiers = 0;
            if (_shiftModifier)
                consoleWindow.ToggleConsoleModifiers |= EventModifiers.Shift;
            if (_ctrlModifier)
                consoleWindow.ToggleConsoleModifiers |= EventModifiers.Control;
            if (_altModifier)
                consoleWindow.ToggleConsoleModifiers |= EventModifiers.Alt;
        }
    }
}