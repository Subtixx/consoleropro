using System.Collections.Generic;
using UnityEngine;

public class ExampleScript : MonoBehaviour
{
    /// <summary>
    ///     Field to hold the console window script
    /// </summary>
    private ConsoleWindow _consoleWindow;

    private void Awake()
    {
        // Find the console window script
        _consoleWindow = GameObject.Find("ConsoleWindow").GetComponent<ConsoleWindow>();

        // Call our function to add the commands
        AddCommands();
    }

    /// <summary>
    ///     Unity Event to enable this script
    /// </summary>
    private void OnEnable()
    {
        // Call our function to add the commands
        AddCommands();
    }

    /// <summary>
    ///     Unity Event to disable this script
    /// </summary>
    private void OnDisable()
    {
        // This will remove the command "camera"
        _consoleWindow.CommandMgr.Remove("camera");
        // This will remove the command "setcamera"
        _consoleWindow.CommandMgr.Remove("setcamera");
    }

    /// <summary>
    ///     Function to add commands to the console
    /// </summary>
    private void AddCommands()
    {
        // Add command "camera" as an anonymous function that shows the current position of the camera
        _consoleWindow.CommandMgr.Add(new ConsoleWindow.ConsoleCommand("camera", "",
            "Shows the camera position",
            (command, args) =>
            {
                _consoleWindow.Log(LogType.Log, "{0:0.0F},{1:0.0F},{2:0.0F}", transform.position.x,
                    transform.position.y, transform.position.z);
                return CommandResult.Ok;
            }));

        // Add command "setcamera" that sets the camera position
        _consoleWindow.CommandMgr.Add(new ConsoleWindow.ConsoleCommand("setcamera", "[x] [y] [z]",
            "Sets the camera position",
            HandleSetCameraCmd));
    }

    private CommandResult HandleSetCameraCmd(string command, IList<string> args)
    {
        if (args.Count < 3)
            return CommandResult.InvalidArgument;
        float x, y, z;

        if (!float.TryParse(args[0], out x))
            return CommandResult.InvalidArgument;
        if (!float.TryParse(args[1], out y))
            return CommandResult.InvalidArgument;
        if (!float.TryParse(args[2], out z))
            return CommandResult.InvalidArgument;

        transform.position = new Vector3(x, y, z);
        _consoleWindow.Log(LogType.Log, "New position {0:0.0F},{1:0.0F},{2:0.0F}", transform.position.x,
            transform.position.y, transform.position.z);
        return CommandResult.Ok;
    }
}