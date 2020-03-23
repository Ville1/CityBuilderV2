using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class LoadGUIManager : MonoBehaviour
{
    public static LoadGUIManager Instance { get; private set; }

    public GameObject Panel;
    public Text Location_Text;
    public Button File_Button_Prototype;
    public GameObject Scroll_View_Content;
    public InputField Input;
    public Button Load_Button;

    private List<Button> file_buttons;
    private string path;

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
        File_Button_Prototype.gameObject.SetActive(false);
        file_buttons = new List<Button>();
        Load_Button.interactable = false;
        Button.ButtonClickedEvent click = new Button.ButtonClickedEvent();
        click.AddListener(new UnityAction(Load));
        Load_Button.onClick = click;
        Active = false;
    }

    /// <summary>
    /// Per frame update
    /// </summary>
    private void Update()
    {
        if (Active) {
            Load_Button.interactable = !string.IsNullOrEmpty(Input.text);
        }
    }

    public bool Active
    {
        get {
            return Panel.activeSelf;
        }
        set {
            Panel.SetActive(value);
            if (Active) {
                MasterUIManager.Instance.Close_Others(GetType().Name);
                Update_GUI();
            }
        }
    }

    private void Update_GUI()
    {
        foreach (Button b in file_buttons) {
            GameObject.Destroy(b.gameObject);
        }
        file_buttons.Clear();

        path = SaveManager.DEFAULT_SAVE_LOCATION;
        Location_Text.text = path;
        if (!Directory.Exists(path)) {
            MessageManager.Instance.Show_Message(string.Format("Directory {0} does not exit", path));
            Active = false;
            return;
        }
        foreach (string file_path in Directory.GetFiles(path)) {
            FileInfo info = new FileInfo(file_path);
            Button button = GameObject.Instantiate(
                File_Button_Prototype,
                new Vector3(
                    File_Button_Prototype.transform.position.x,
                    File_Button_Prototype.transform.position.y - (20.0f * file_buttons.Count),
                    File_Button_Prototype.transform.position.z
                ),
                Quaternion.identity,
                Scroll_View_Content.transform
            );
            button.gameObject.SetActive(true);
            button.name = string.Format("file_{0}", file_buttons.Count);
            button.GetComponentInChildren<Text>().text = info.Name;
            Button.ButtonClickedEvent click = new Button.ButtonClickedEvent();
            click.AddListener(new UnityAction(delegate () { Select_File(info.Name); }));
            button.onClick = click;
            file_buttons.Add(button);
        }
        Scroll_View_Content.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 20.0f * file_buttons.Count);
    }

    public void Select_File(string file)
    {
        Input.text = file;
    }

    private void Load()
    {
        if (string.IsNullOrEmpty(Input.text)) {
            return;
        }
        Active = false;
        Map.Instance.Start_Loading(Path.Combine(path, Input.text));
    }
}