using System.Diagnostics;
using System.Reflection;

public class CustomLogger
{
    private static CustomLogger instance;

    private CustomLogger()
    { }

    /// <summary>
    /// Accessor for singleton instance
    /// </summary>
    public static CustomLogger Instance
    {
        get {
            if (instance == null) {
                instance = new CustomLogger();
            }
            return instance;
        }
    }

    /// <summary>
    /// Log debug message
    /// </summary>
    /// <param name="message"></param>
    public void Debug(string message)
    {
        StackTrace trace = new StackTrace();
        StackFrame frame = trace.GetFrame(1);
        UnityEngine.Debug.Log("DEBUG - " + frame.GetMethod().ReflectedType.Name + " -> " + Parse_Method_Name(frame.GetMethod()) + ": " + message);
    }

    /// <summary>
    /// Log warning message
    /// </summary>
    /// <param name="message"></param>
    public void Warning(string message)
    {
        StackTrace trace = new StackTrace();
        StackFrame frame = trace.GetFrame(1);
        string log = "WARNING - " + frame.GetMethod().ReflectedType.Name + " -> " + Parse_Method_Name(frame.GetMethod()) + ": " + message;
        UnityEngine.Debug.Log(log);
        if (ConsoleManager.Instance != null) {
            ConsoleManager.Instance.Run_Command("echo " + log);
        }
    }

    /// <summary>
    /// Log error message
    /// </summary>
    /// <param name="message"></param>
    public void Error(string message)
    {
        StackTrace trace = new StackTrace();
        StackFrame frame = trace.GetFrame(1);
        string log = "ERROR - " + frame.GetMethod().ReflectedType.Name + " -> " + Parse_Method_Name(frame.GetMethod()) + ": " + message;
        UnityEngine.Debug.Log(log);
        if (ConsoleManager.Instance != null) {
            ConsoleManager.Instance.Run_Command("echo " + log);
        }
    }

    /// <summary>
    /// Replaces constructor abreviation
    /// </summary>
    /// <param name="method_base"></param>
    /// <returns></returns>
    private string Parse_Method_Name(MethodBase method_base)
    {
        string name = method_base.Name;
        if (name == ".ctor") {
            name = "Constructor";
        }
        return name;
    }
}

