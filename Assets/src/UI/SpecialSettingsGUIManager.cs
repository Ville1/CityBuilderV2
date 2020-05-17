using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SpecialSettingsGUIManager : MonoBehaviour {
    public static SpecialSettingsGUIManager Instance { get; private set; }

    public GameObject Panel;
    public GameObject Content;
    public GameObject Slider_Row_Prototype;
    public GameObject Input_Field_Row_Prototype;
    public GameObject Toggle_Row_Prototype;
    public GameObject Dropdown_Row_Prototype;
    public GameObject Button_Row_Prototype;

    private Building building;
    private Dictionary<SpecialSetting, GameObject> rows;
    private long row_id;

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
        Active = false;
        Slider_Row_Prototype.SetActive(false);
        Input_Field_Row_Prototype.SetActive(false);
        Toggle_Row_Prototype.SetActive(false);
        Dropdown_Row_Prototype.SetActive(false);
        Button_Row_Prototype.SetActive(false);
        rows = new Dictionary<SpecialSetting, GameObject>();
        row_id = 0;
    }

    /// <summary>
    /// Per frame update
    /// </summary>
    private void Update()
    {
        if (!Active) {
            return;
        }
        foreach(KeyValuePair<SpecialSetting, GameObject> pair in rows) {
            if(pair.Key.Type == SpecialSetting.SettingType.Slider) {
                GameObject.Find(string.Format("{0}/PercentText", pair.Value.name)).GetComponent<Text>().text = Helper.Float_To_String(
                    GameObject.Find(string.Format("{0}/Slider", pair.Value.name)).GetComponent<Slider>().value * 100.0f, 0) + "%";
            }
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
            }
        }
    }

    public void Show(Building building)
    {
        Active = true;
        this.building = building;

        foreach(KeyValuePair<SpecialSetting, GameObject> pair in rows) {
            GameObject.Destroy(pair.Value);
        }
        rows.Clear();

        foreach(SpecialSetting setting in building.Special_Settings) {
            GameObject prototype = null;
            switch (setting.Type) {
                case SpecialSetting.SettingType.Input:
                    prototype = Input_Field_Row_Prototype;
                    break;
                case SpecialSetting.SettingType.Slider:
                    prototype = Slider_Row_Prototype;
                    break;
                case SpecialSetting.SettingType.Toggle:
                    prototype = Toggle_Row_Prototype;
                    break;
                case SpecialSetting.SettingType.Dropdown:
                    prototype = Dropdown_Row_Prototype;
                    break;
                case SpecialSetting.SettingType.Button:
                    prototype = Button_Row_Prototype;
                    break;
            }
            GameObject row = GameObject.Instantiate(
                prototype,
                new Vector3(
                    Input_Field_Row_Prototype.transform.position.x,
                    Input_Field_Row_Prototype.transform.position.y - (22.5f * rows.Count),
                    Input_Field_Row_Prototype.transform.position.z
                ),
                Quaternion.identity,
                Content.transform
            );
            row.SetActive(true);
            row.name = string.Format("{0}_row_#{1}", setting.Name, row_id);
            row_id++;
            if (row_id > 9999) {
                row_id = 0;
            }

            //TODO: Use switch/case
            if (setting.Type == SpecialSetting.SettingType.Slider) {
                GameObject.Find(string.Format("{0}/LabelText", row.name)).GetComponent<Text>().text = setting.Label;
                GameObject.Find(string.Format("{0}/PercentText", row.name)).GetComponent<Text>().text = Helper.Float_To_String(setting.Slider_Value * 100.0f, 0) + "%";
                GameObject.Find(string.Format("{0}/Slider", row.name)).GetComponent<Slider>().value = setting.Slider_Value;
            } else if(setting.Type == SpecialSetting.SettingType.Input) {
                GameObject.Find(string.Format("{0}/LabelText", row.name)).GetComponent<Text>().text = setting.Label;
            } else if (setting.Type == SpecialSetting.SettingType.Toggle) {
                GameObject.Find(string.Format("{0}/Toggle", row.name)).GetComponentInChildren<Text>().text = setting.Label;
                GameObject.Find(string.Format("{0}/Toggle", row.name)).GetComponentInChildren<Toggle>().isOn = setting.Toggle_Value;
            } else if(setting.Type == SpecialSetting.SettingType.Dropdown) {
                GameObject.Find(string.Format("{0}/LabelText", row.name)).GetComponent<Text>().text = setting.Label;
                Dropdown dropdown = GameObject.Find(string.Format("{0}/Dropdown", row.name)).GetComponent<Dropdown>();
                List<Dropdown.OptionData> options = setting.Dropdown_Options.Select(x => new Dropdown.OptionData(x)).ToList();
                dropdown.options = options;
                dropdown.value = setting.Dropdown_Selection;
            } else if(setting.Type == SpecialSetting.SettingType.Button) {
                GameObject.Find(string.Format("{0}/Button", row.name)).GetComponentInChildren<Text>().text = setting.Label;
                GameObject.Find(string.Format("{0}/Button", row.name)).GetComponent<Button>().interactable = !setting.Button_Was_Pressed;
                Button.ButtonClickedEvent click = new Button.ButtonClickedEvent();
                click.AddListener(new UnityAction(delegate () { On_Click(setting, row.name); }));
                GameObject.Find(string.Format("{0}/Button", row.name)).GetComponent<Button>().onClick = click;
            }

            rows.Add(setting, row);
        }
        Content.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (rows.Count * 22.5f) + 5.0f);
    }

    public void Apply()
    {
        if (!Active) {
            return;
        }
        foreach(KeyValuePair<SpecialSetting, GameObject> pair in rows) {
            if(pair.Key.Type == SpecialSetting.SettingType.Slider) {
                pair.Key.Slider_Value = GameObject.Find(string.Format("{0}/Slider", pair.Value.name)).GetComponent<Slider>().value;
                pair.Key.Slider_Value = Mathf.RoundToInt(pair.Key.Slider_Value * 100.0f) / 100.0f;
            } else if(pair.Key.Type == SpecialSetting.SettingType.Input) {

            } else if(pair.Key.Type == SpecialSetting.SettingType.Toggle) {
                pair.Key.Toggle_Value = GameObject.Find(string.Format("{0}/Toggle", pair.Value.name)).GetComponentInChildren<Toggle>().isOn;
            } else if(pair.Key.Type == SpecialSetting.SettingType.Dropdown) {
                pair.Key.Dropdown_Selection = GameObject.Find(string.Format("{0}/Dropdown", pair.Value.name)).GetComponentInChildren<Dropdown>().value;
            }
        }
        Active = false;
    }

    public void Cancel()
    {
        Active = false;
    }

    private void On_Click(SpecialSetting setting, string row_name)
    {
        setting.Button_Was_Pressed = true;
        GameObject.Find(string.Format("{0}/Button", row_name)).GetComponent<Button>().interactable = false;
    }
}
