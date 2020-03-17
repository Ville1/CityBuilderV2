using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpecialSettingsGUIManager : MonoBehaviour {
    public static SpecialSettingsGUIManager Instance { get; private set; }

    public GameObject Panel;
    public GameObject Content;
    public GameObject Slider_Row_Prototype;
    public GameObject Input_Field_Row_Prototype;

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
            GameObject row = GameObject.Instantiate(
                setting.Type == SpecialSetting.SettingType.Input ? Input_Field_Row_Prototype : Slider_Row_Prototype,
                new Vector3(
                    Input_Field_Row_Prototype.transform.position.x,
                    Input_Field_Row_Prototype.transform.position.y - (20.0f * rows.Count),
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

            if (setting.Type == SpecialSetting.SettingType.Slider) {
                GameObject.Find(string.Format("{0}/LabelText", row.name)).GetComponent<Text>().text = setting.Label;
                GameObject.Find(string.Format("{0}/PercentText", row.name)).GetComponent<Text>().text = Helper.Float_To_String(setting.Slider_Value * 100.0f, 0) + "%";
                GameObject.Find(string.Format("{0}/Slider", row.name)).GetComponent<Slider>().value = setting.Slider_Value;
            } else if(setting.Type == SpecialSetting.SettingType.Input) {
                GameObject.Find(string.Format("{0}/LabelText", row.name)).GetComponent<Text>().text = setting.Label;
            }

            rows.Add(setting, row);
        }
        Content.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, rows.Count * 20.0f);
    }

    public void Apply()
    {
        if (!Active) {
            return;
        }
        foreach(KeyValuePair<SpecialSetting, GameObject> pair in rows) {
            if(pair.Key.Type == SpecialSetting.SettingType.Slider) {
                var a = GameObject.Find(string.Format("{0}/Slider", pair.Value.name)).GetComponent<Slider>();
                pair.Key.Slider_Value = GameObject.Find(string.Format("{0}/Slider", pair.Value.name)).GetComponent<Slider>().value;
                pair.Key.Slider_Value = Mathf.RoundToInt(pair.Key.Slider_Value * 100.0f) / 100.0f;
            } else if(pair.Key.Type == SpecialSetting.SettingType.Input) {

            }
        }
        Active = false;
    }

    public void Cancel()
    {
        Active = false;
    }
}
