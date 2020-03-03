using System;
using System.Collections.Generic;
using UnityEngine;

public class ConsoleScriptManager : MonoBehaviour {
    public static ConsoleScriptManager Instance;

    private Dictionary<string, string> scripts = new Dictionary<string, string>();

    /// <summary>
    /// Initializiation
    /// </summary>
    private void Start()
    {
        if (Instance != null) {
            CustomLogger.Instance.Error(LogMessages.MULTIPLE_INSTANCES);
            return;
        }
        Instance = this;
        foreach (TextAsset text_file in Resources.LoadAll<TextAsset>("console_scripts")) {
            scripts.Add(text_file.name, text_file.text);
            CustomLogger.Instance.Debug("Console script loaded: " + text_file.name);
        }
    }

    public bool Script_Exists(string script)
    {
        return scripts.ContainsKey(script);
    }

    public bool Run_Script(string script)
    {
        if (!scripts.ContainsKey(script)) {
            return false;
        }
        string[] lines = scripts[script].Split(new string[1] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < lines.Length; i++) {
            ConsoleManager.Instance.Run_Command(lines[i]);
        }
        return true;
    }
}
