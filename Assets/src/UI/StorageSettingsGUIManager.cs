﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StorageSettingsGUIManager : MonoBehaviour {
    public static StorageSettingsGUIManager Instance { get; private set; }

    public GameObject Panel;

    public GameObject Scroll_Content;
    public GameObject Row_Prototype;
    public Text Total_Text;
    public InputField Set_All_Input;
    public Dropdown Set_All_Dropdown;

    private Dictionary<Resource, GameObject> rows;
    private Building building;
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
        Row_Prototype.SetActive(false);
        rows = new Dictionary<Resource, GameObject>();
        row_id = 0;
    }

    /// <summary>
    /// Per frame update
    /// </summary>
    private void Update()
    {
        if (Active) {
            if(building == null || building.Is_Deleted) {
                Active = false;
                return;
            }
            foreach(KeyValuePair<Resource, float> pair in building.Storage) {
                if (rows.ContainsKey(pair.Key)) {
                    GameObject.Find(string.Format("{0}/CurrentAmountText", rows[pair.Key].name)).GetComponentInChildren<Text>().text = Helper.Float_To_String(pair.Value, 1);
                }
            }
            Total_Text.text = string.Format("Total: {0} / {1}", Helper.Float_To_String(building.Current_Storage_Amount, 1), building.Storage_Limit);
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

        foreach(KeyValuePair<Resource, GameObject> pair in rows) {
            GameObject.Destroy(pair.Value);
        }
        rows.Clear();

        foreach(StorageSetting setting in building.Storage_Settings.Settings) {
            GameObject row = GameObject.Instantiate(
                Row_Prototype,
                new Vector3(
                    Row_Prototype.transform.position.x,
                    Row_Prototype.transform.position.y - (25.0f * rows.Count),
                    Row_Prototype.transform.position.z
                ),
                Quaternion.identity,
                Scroll_Content.transform
            );
            row.SetActive(true);
            row.name = string.Format("{0}_row_#{1}", setting.Resource.ToString().ToLower(), row_id);
            row_id++;
            if(row_id > 9999) {
                row_id = 0;
            }

            Helper.Set_Image(row.name, "IconImage", setting.Resource.Has_Sprite ? setting.Resource.Sprite_Name : "empty", setting.Resource.Has_Sprite ? setting.Resource.Sprite_Type : SpriteManager.SpriteType.UI);
            Helper.Set_Text(row.name, "ResourceNameText", setting.Resource.UI_Name);
            GameObject current_text = GameObject.Find(string.Format("{0}/CurrentAmountText", row.name));
            Helper.Set_Text(row.name, "CurrentAmountText", Helper.Float_To_String(building.Storage.ContainsKey(setting.Resource) ? building.Storage[setting.Resource] : 0.0f, 1));
            row.GetComponentInChildren<InputField>().text = setting.Limit.ToString();
            Dropdown dropdown = row.GetComponentInChildren<Dropdown>();
            dropdown.ClearOptions();
            List<Dropdown.OptionData> option_data = new List<Dropdown.OptionData>();
            foreach(StorageSetting.StoragePriority priority in Enum.GetValues(typeof(StorageSetting.StoragePriority))) {
                option_data.Add(new Dropdown.OptionData(Helper.Snake_Case_To_UI(priority.ToString(), true)));
            }
            dropdown.AddOptions(option_data);
            dropdown.value = (int)setting.Priority;

            rows.Add(setting.Resource, row);
        }
        Total_Text.text = string.Format("Total: {0} / {1}", Helper.Float_To_String(building.Current_Storage_Amount, 1), building.Storage_Limit);
        Scroll_Content.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 25.0f * rows.Count);
        Set_All_Input.text = (building.Storage_Limit / 2).ToString();
        Set_All_Dropdown.ClearOptions();
        List<Dropdown.OptionData> set_all_option_data = new List<Dropdown.OptionData>();
        foreach (StorageSetting.StoragePriority priority in Enum.GetValues(typeof(StorageSetting.StoragePriority))) {
            set_all_option_data.Add(new Dropdown.OptionData(Helper.Snake_Case_To_UI(priority.ToString(), true)));
        }
        Set_All_Dropdown.AddOptions(set_all_option_data);
        Set_All_Dropdown.value = (int)StorageSetting.StoragePriority.Medium;
    }

    public void Allow_All()
    {
        if (!Active) {
            return;
        }
        foreach (KeyValuePair<Resource, GameObject> pair in rows) {
            pair.Value.GetComponentInChildren<InputField>().text = building.Storage_Limit.ToString();
        }
    }

    public void Clear_All()
    {
        if (!Active) {
            return;
        }
        foreach (KeyValuePair<Resource, GameObject> pair in rows) {
            pair.Value.GetComponentInChildren<InputField>().text = "0";
        }
    }

    public void Set_All()
    {
        if (!Active) {
            return;
        }
        int limit = 0;
        if(!int.TryParse(Set_All_Input.text, out limit)) {
            limit = building.Storage_Limit / 2;
        }
        limit = Mathf.Clamp(limit, 0, building.Storage_Limit);
        Set_All_Input.text = limit.ToString();
        foreach (KeyValuePair<Resource, GameObject> pair in rows) {
            pair.Value.GetComponentInChildren<InputField>().text = limit.ToString();
            pair.Value.GetComponentInChildren<Dropdown>().value = Set_All_Dropdown.value;
        }
    }

    public void Apply()
    {
        if (!Active) {
            return;
        }
        foreach(KeyValuePair<Resource, GameObject> pair in rows) {
            GameObject row = pair.Value;
            int limit;
            if(!int.TryParse(row.GetComponentInChildren<InputField>().text, out limit)) {
                limit = building.Storage_Limit;
            }
            StorageSetting.StoragePriority priority = (StorageSetting.StoragePriority)row.GetComponentInChildren<Dropdown>().value;
            building.Storage_Settings.Get(pair.Key).Limit = limit;
            building.Storage_Settings.Get(pair.Key).Priority = priority;
        }
        Active = false;
    }

    public void Cancel()
    {
        Active = false;
    }
}
