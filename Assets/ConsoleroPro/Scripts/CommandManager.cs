using System;
using System.Collections.Generic;

public static class CommandManager
{
    /// <summary>
    ///     Returns arguments parsed from line.
    /// </summary>
    /// <remarks>
    ///     Matches words and multiple words in quotation.
    /// </remarks>
    /// <example>
    ///     arg0 arg1 arg2 -- 3 args: "arg0", "arg1", and "arg2"
    ///     arg0 arg1 "arg2 arg3" -- 3 args: "arg0", "arg1", and "arg2 arg3"
    /// </example>
    public static IList<string> ParseLine(string line)
    {
        var args = new List<string>();
        var quote = false;
        for (int i = 0, n = 0; i <= line.Length; ++i)
        {
            if ((i == line.Length || line[i] == ' ') && !quote)
            {
                if (i - n > 0)
                    args.Add(line.Substring(n, i - n).Trim(' ', '"'));
	
                n = i + 1;
                continue;
            }
	
            if (line[i] == '"')
                quote = !quote;
        }
	
        return args;
    }
}

/// <summary>
///     Generalized command manager
/// </summary>
/// <typeparam name="TCommand"></typeparam>
/// <typeparam name="TFunc"></typeparam>
public class CommandManager<TCommand, TFunc>
    where TCommand : Command<TFunc>
    where TFunc : class
{
    public readonly Dictionary<string, TCommand> Commands;

    public CommandManager()
    {
        Commands = new Dictionary<string, TCommand>();
    }

    /// <summary>
    ///     Adds command to list of command handlers.
    /// </summary>
    /// <param name="command"></param>
    public void Add(TCommand command)
    {
        Commands[command.Name] = command;
    }

    /// <summary>
    /// Removes a command from the list of command handlers.
    /// </summary>
    /// <param name="name"></param>
    public void Remove(string name)
    {
        Commands.Remove(name);
    }

    /// <summary>
    ///     Returns command or null, if the command doesn't exist.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public TCommand GetCommand(string name)
    {
        TCommand command;
        Commands.TryGetValue(name, out command);
        return command;
    }
}

/// <summary>
///     Generalized command holder
/// </summary>
/// <typeparam name="TFunc"></typeparam>
public abstract class Command<TFunc> where TFunc : class
{
    protected Command(string name, string usage, string description, TFunc func)
    {
        if (!typeof(TFunc).IsSubclassOf(typeof(Delegate)))
            throw new InvalidOperationException(typeof(TFunc).Name + " is not a delegate type");

        Name = name;
        Usage = usage;
        Description = description;
        Func = func;
    }

    protected Command(string name, string description, TFunc func)
    {
        if (!typeof(TFunc).IsSubclassOf(typeof(Delegate)))
            throw new InvalidOperationException(typeof(TFunc).Name + " is not a delegate type");

        Name = name;
        Usage = "";
        Description = description;
        Func = func;
    }

    /// <summary>
    /// The actual command
    /// </summary>
    public string Name { get; protected set; }
    
    /// <summary>
    /// How to use the command ex:
    /// [x] (y) (z)
    /// </summary>
    public string Usage { get; protected set; }
    
    /// <summary>
    /// Explanation what this command does
    /// </summary>
    public string Description { get; protected set; }
    
    /// <summary>
    /// The function that gets executed
    /// </summary>
    public TFunc Func { get; protected set; }
}

/// <summary>
/// Result of a command
/// </summary>
public enum CommandResult
{
    /// <summary>Command was executed successfully</summary>
    Ok,
    /// <summary>There was an error executing the command</summary>
    Fail,
    /// <summary>An invalid argument was supplied</summary>
    InvalidArgument
}