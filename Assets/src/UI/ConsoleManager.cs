using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ConsoleManager : MonoBehaviour
{
    private static int max_lines = 10;
    private static char echo_command_start = '[';
    private static char echo_command_end = ']';
    private static char argument_variable_prefix = '$';

    public static ConsoleManager Instance;
    public GameObject Panel;
    public Text Output;
    public InputField Input;

    private List<string> output_log;
    private int scroll_position;
    private delegate string Command(string[] arguments);
    private Dictionary<string, Command> commands;
    private List<string> command_history;
    private int command_history_index;
    private Dictionary<string, string> variables;

    /// <summary>
    /// Initialization
    /// </summary>
    private void Start()
    {
        if (Instance != null) {
            CustomLogger.Instance.Error(LogMessages.MULTIPLE_INSTANCES);
            return;
        }
        Instance = this;

        output_log = new List<string>();
        output_log.Add("Console!");
        scroll_position = 0;
        command_history = new List<string>();
        command_history_index = 0;
        variables = new Dictionary<string, string>();

        commands = new Dictionary<string, Command>();
        commands.Add("exit", (string[] arguments) => {
            Close_Console();
            return "";
        });
        commands.Add("echo", (string[] arguments) => {
            if (arguments.Length == 1) {
                return "Missing argument";
            }
            StringBuilder builder = new StringBuilder();
            for (int i = 1; i < arguments.Length; i++) {
                builder.Append(arguments[i]);
                builder.Append(" ");
            }
            string full_input = builder.ToString();
            builder = new StringBuilder();
            StringBuilder command_builder = new StringBuilder();
            bool in_command = false;
            for (int i = 0; i < full_input.Length; i++) {
                if (full_input[i] == echo_command_start && !in_command) {
                    in_command = true;
                } else if (full_input[i] == echo_command_end && in_command) {
                    in_command = false;
                    if (command_builder.Length != 0) {
                        string[] parts = command_builder.ToString().Split(' ');
                        if (commands.ContainsKey(parts[0])) {
                            builder.Append(commands[parts[0]](parts));
                        } else if (variables.ContainsKey(parts[0])) {
                            builder.Append(variables[parts[0]]);
                        } else {
                            builder.Append("Invalid command!");
                        }
                    }
                    command_builder = new StringBuilder();
                } else {
                    if (in_command) {
                        command_builder.Append(full_input[i]);
                    } else {
                        builder.Append(full_input[i]);
                    }
                }
            }

            return builder.ToString();
        });
        commands.Add("echo_raw", (string[] arguments) => {
            if (arguments.Length == 1) {
                return "Missing argument";
            }
            StringBuilder builder = new StringBuilder();
            for (int i = 1; i < arguments.Length; i++) {
                builder.Append(arguments[i]);
                builder.Append(" ");
            }
            return builder.ToString();
        });
        commands.Add("version", (string[] arguments) => {
            return ("Game: " + Main.VERSION + " Unity: " + Application.unityVersion);
        });
        commands.Add("kill", (string[] arguments) => {
            Application.Quit();
            return "";
        });
        commands.Add("list", (string[] arguments) => {
            StringBuilder list = new StringBuilder();
            foreach (KeyValuePair<string, Command> command in commands) {
                list.Append(command.Key).Append(", ");
            }
            list.Remove(list.Length - 2, 2);
            return list.ToString();
        });

        commands.Add("set_variable", (string[] arguments) => {
            if (arguments.Length != 3) {
                return "Invalid number of arguments";
            }
            if (commands.ContainsKey(arguments[1]) || ConsoleScriptManager.Instance.Script_Exists(arguments[1])) {
                return "Reserved name";
            }
            if (variables.ContainsKey(arguments[1])) {
                variables[arguments[1]] = arguments[2];
            } else {
                variables.Add(arguments[1], arguments[2]);
            }
            return string.Format("{0} = {1}", arguments[1], arguments[2]);
        });
        commands.Add("remove_variable", (string[] arguments) => {
            if (arguments.Length != 2) {
                return "Invalid number of arguments";
            }
            if (!variables.ContainsKey(arguments[1])) {
                return "Variable does not exist";
            }
            variables.Remove(arguments[1]);
            return "Variable removed";
        });
        commands.Add("show_message", (string[] arguments) => {
            if (arguments.Length != 2) {
                return "Invalid number of arguments";
            }
            MessageManager.Instance.Show_Message(arguments[1]);
            Close_Console();
            return "";
        });

        commands.Add("instant_build", (string[] arguments) => {
            if (arguments.Length != 1) {
                return "Invalid number of arguments";
            }
            int count = 0;
            foreach(Building building in City.Instance.Buildings) {
                if(!building.Is_Built && !building.Is_Deconstructing) {
                    building.Instant_Build();
                    count++;
                }
            }
            return string.Format("{0} building{1} built", count, Helper.Plural(count));
        });

        commands.Add("set_grace_time", (string[] arguments) => {
            if (arguments.Length != 2) {
                return "Invalid number of arguments";
            }
            int time;
            if(!int.TryParse(arguments[1], out time)) {
                return "Invalid time";
            }
            City.Instance.Grace_Time_Remaining = time;
            return "Time set";
        });

        commands.Add("give", (string[] arguments) => {
            if (arguments.Length != 3) {
                return "Invalid number of arguments";
            }
            int amount = 0;
            if(!int.TryParse(arguments[1], out amount)) {
                return "Invalid resource amount";
            }
            if(arguments[2].ToLower() == "cash") {
                City.Instance.Add_Cash(amount);
            } else {
                Resource resource = null;
                foreach(Resource r in Resource.All) {
                    if(r.ToString().ToLower() == arguments[2].ToLower()) {
                        resource = r;
                        break;
                    }
                }
                if(resource == null) {
                    return "Invalid resource name";
                }
                City.Instance.Add_To_Storage(resource, amount);
            }
            return "There you go!";
        });

        Update_Output();
        Panel.SetActive(false);
    }

    /// <summary>
    /// Per frame update
    /// </summary>
    private void Update()
    {

    }

    /// <summary>
    /// Opens console
    /// </summary>
    public void Open_Console()
    {
        if (Panel.activeSelf) {
            return;
        }
        MasterUIManager.Instance.Close_Others(GetType().Name);
        Panel.SetActive(true);
        EventSystem.current.SetSelectedGameObject(Input.gameObject, null);
        Input.OnPointerClick(new PointerEventData(EventSystem.current));
    }

    /// <summary>
    /// Close console
    /// </summary>
    public void Close_Console()
    {
        if (Panel.activeSelf == false) {
            return;
        }
        Panel.SetActive(false);
    }

    /// <summary>
    /// Open / close console
    /// </summary>
    public void Toggle_Console()
    {
        if (Panel.activeSelf) {
            Close_Console();
        } else {
            Open_Console();
        }
    }

    /// <summary>
    /// Run command from program
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    public bool Run_Command(string command)
    {
        Input.text = command;
        return Run_Command(false);
    }

    /// <summary>
    /// Runs a command currently typed into Input
    /// </summary>
    /// <param name="add_to_history"></param>
    /// <returns></returns>
    public bool Run_Command(bool add_to_history = true)
    {
        if (Input.text == string.Empty) {
            return false;
        }
        string[] parts = Input.text.Split(' ');
        //Read arguments from variables
        for (int i = 1; i < parts.Length; i++) {
            if (parts[i][0] == argument_variable_prefix && parts[i].Length > 1) {
                if (variables.ContainsKey(parts[i].Substring(1))) {
                    parts[i] = variables[parts[i].Substring(1)];
                }
            }
        }
        if (!commands.ContainsKey(parts[0])) {
            if (variables.ContainsKey(parts[0])) {
                //Read variable
                output_log.Add(variables[parts[0]]);
                Input.text = string.Empty;
            } else if (!ConsoleScriptManager.Instance.Run_Script(parts[0])) { //Run script
                output_log.Add("Invalid command!");
            }
        } else {
            //Run command
            if (add_to_history) {
                command_history.Insert(0, Input.text);
            }
            string log = commands[parts[0]](parts);
            if (!string.IsNullOrEmpty(log)) {
                output_log.Add(log);
            }
            Input.text = string.Empty;
        }
        Update_Output();
        EventSystem.current.SetSelectedGameObject(Input.gameObject, null);
        Input.OnPointerClick(new PointerEventData(EventSystem.current));
        return true;
    }

    /// <summary>
    /// Scroll up
    /// </summary>
    /// <returns></returns>
    public bool Scroll_Up()
    {
        if (!Panel.activeSelf) {
            return false;
        }
        scroll_position++;
        if (output_log.Count - max_lines - 2 - scroll_position < 0) {
            scroll_position--;
        }
        Update_Output();
        return true;
    }

    /// <summary>
    /// Scroll down
    /// </summary>
    /// <returns></returns>
    public bool Scroll_Down()
    {
        if (!Panel.activeSelf) {
            return false;
        }
        scroll_position--;
        if (scroll_position < 0) {
            scroll_position = 0;
        }
        Update_Output();
        return true;
    }

    /// <summary>
    /// Scroll command history up
    /// </summary>
    /// <returns></returns>
    public bool Command_History_Up()
    {
        if (!Panel.activeSelf) {
            return false;
        }
        if (command_history.Count == 0) {
            return true;
        }
        command_history_index++;
        if (command_history_index > command_history.Count + 1) {
            command_history_index = command_history.Count + 1;
        }
        if (command_history.Count > command_history_index - 1) {
            Input.text = command_history[command_history_index - 1];
        } else {
            Input.text = "";
        }
        return true;
    }

    /// <summary>
    /// Scroll command history down
    /// </summary>
    /// <returns></returns>
    public bool Command_History_Down()
    {
        if (!Panel.activeSelf) {
            return false;
        }
        if (command_history.Count == 0) {
            return true;
        }
        command_history_index--;
        if (command_history_index < 0) {
            command_history_index = 0;
        }
        if (command_history.Count > command_history_index - 1 && command_history_index > 0) {
            Input.text = command_history[command_history_index - 1];
        } else {
            Input.text = "";
        }
        return true;
    }

    /// <summary>
    /// Try to autocomplete command name
    /// </summary>
    /// <returns></returns>
    public bool Auto_Complete()
    {
        if (!Panel.activeSelf) {
            return false;
        }
        if (Input.text == "") {
            return true;
        }

        string complete = "";
        foreach (KeyValuePair<string, Command> command in commands) {
            if (command.Key.StartsWith(Input.text)) {
                complete = command.Key;
                break;
            }
        }

        if (complete != "") {
            Input.text = complete;
        }

        return true;
    }

    /// <summary>
    /// Is console open?
    /// </summary>
    public bool Is_Open()
    {
        return Panel.activeSelf;
    }

    /// <summary>
    /// Updates console output field
    /// </summary>
    private void Update_Output()
    {
        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < max_lines; i++) {
            if (output_log.Count > i - scroll_position) {
                builder.Insert(0, output_log[output_log.Count - i - 1 - scroll_position] + "\n");
            }
        }
        Output.text = builder.ToString();
        command_history_index = 0;
    }
}
