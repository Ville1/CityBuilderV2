﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class InspectorManager : MonoBehaviour {
    public static InspectorManager Instance { get; private set; }

    public GameObject Panel;

    public Text Name_Text;
    public Text Id_Text;
    public Image Image;

    public GameObject Instance_Container;
    public Text HP_Text;
    public Text Status_Text;
    public Text Efficency_Text;
    public Text Taxed_By_Text;
    public Text Special_Status_1_Text;
    public Text Special_Status_2_Text;
    public Text Storage_Text;
    public GameObject Delta_Content;
    public GameObject Delta_Row_Prototype;
    public GameObject Storage_Container;
    public GameObject Storage_Content;
    public GameObject Storage_Row_Prototype;
    public GameObject Services_Container;
    public GameObject Services_Content;
    public GameObject Services_Row_Prototype;
    public Button Pause_Button;
    public Button Delete_Button;
    public Button Settings_Button;

    public GameObject Workers_Container;
    public Text Worker_Peasant_Current;
    public Text Worker_Peasant_Max;
    public Button Worker_Peasant_Plus_Button;
    public Button Worker_Peasant_Minus_Button;
    public Text Worker_Citizen_Current;
    public Text Worker_Citizen_Max;
    public Button Worker_Citizen_Plus_Button;
    public Button Worker_Citizen_Minus_Button;
    public Text Worker_Noble_Current;
    public Text Worker_Noble_Max;
    public Button Worker_Noble_Plus_Button;
    public Button Worker_Noble_Minus_Button;
    public Text Worker_Allocated_Current;
    public Text Worker_Allocated_Max;

    public GameObject Residents_Container;
    public Text Residents_Peasant_Current;
    public Text Residents_Peasant_Max;
    public Text Residents_Peasant_Happiness;
    public Text Residents_Citizen_Current;
    public Text Residents_Citizen_Max;
    public Text Residents_Citizen_Happiness;
    public Text Residents_Noble_Current;
    public Text Residents_Noble_Max;
    public Text Residents_Noble_Happiness;

    public GameObject Prototype_Container;
    public Text Size_Text;
    public Text Appeal_Text;
    public Text Appeal_Range_Text;
    public Text Description_Text;
    public GameObject Cost_Content;
    public GameObject Cost_Row_Prototype;
    public GameObject Upkeep_Content;
    public GameObject Upkeep_Row_Prototype;

    private Building building;
    private List<GameObject> cost_rows;
    private List<GameObject> upkeep_rows;
    private List<GameObject> delta_rows;
    private List<GameObject> storage_rows;
    private List<GameObject> service_rows;
    private List<Tile> highlighted_tiles;
    private long current_row_id;

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
        Cost_Row_Prototype.SetActive(false);
        Upkeep_Row_Prototype.SetActive(false);
        Delta_Row_Prototype.SetActive(false);
        Storage_Row_Prototype.SetActive(false);
        Services_Row_Prototype.SetActive(false);
        cost_rows = new List<GameObject>();
        upkeep_rows = new List<GameObject>();
        delta_rows = new List<GameObject>();
        storage_rows = new List<GameObject>();
        service_rows = new List<GameObject>();
        highlighted_tiles = new List<Tile>();
        current_row_id = 0;

        Button.ButtonClickedEvent click = new Button.ButtonClickedEvent();
        click.AddListener(new UnityAction(Pause));
        Pause_Button.onClick = click;

        Button.ButtonClickedEvent click2 = new Button.ButtonClickedEvent();
        click2.AddListener(new UnityAction(Delete));
        Delete_Button.onClick = click2;

        Button.ButtonClickedEvent click3 = new Button.ButtonClickedEvent();
        click3.AddListener(new UnityAction(Add_Peasant_Worker));
        Worker_Peasant_Plus_Button.onClick = click3;

        Button.ButtonClickedEvent click4 = new Button.ButtonClickedEvent();
        click4.AddListener(new UnityAction(Add_Citizen_Worker));
        Worker_Citizen_Plus_Button.onClick = click4;

        Button.ButtonClickedEvent click5 = new Button.ButtonClickedEvent();
        click5.AddListener(new UnityAction(Add_Noble_Worker));
        Worker_Noble_Plus_Button.onClick = click5;

        Button.ButtonClickedEvent click6 = new Button.ButtonClickedEvent();
        click6.AddListener(new UnityAction(Remove_Peasant_Worker));
        Worker_Peasant_Minus_Button.onClick = click6;

        Button.ButtonClickedEvent click7 = new Button.ButtonClickedEvent();
        click7.AddListener(new UnityAction(Remove_Citizen_Worker));
        Worker_Citizen_Minus_Button.onClick = click7;

        Button.ButtonClickedEvent click8 = new Button.ButtonClickedEvent();
        click8.AddListener(new UnityAction(Remove_Noble_Worker));
        Worker_Noble_Minus_Button.onClick = click8;

        Button.ButtonClickedEvent click9 = new Button.ButtonClickedEvent();
        click9.AddListener(new UnityAction(Show_Settings));
        Settings_Button.onClick = click9;
    }

    /// <summary>
    /// Per frame update
    /// </summary>
    private void Update()
    {
        if (highlighted_tiles.Count != 0) {
            foreach (Tile t in highlighted_tiles) {
                t.Clear_Highlight();
                t.Hide_Text();
            }
            highlighted_tiles.Clear();
        }
        if (Active) {
            Update_GUI();
        }
    }

    public bool Active
    {
        get {
            return Panel.activeSelf;
        }
        set {
            Panel.SetActive(value);
        }
    }

    public Building Building
    {
        get {
            return building;
        }
        set {
            building = value;
            Active = building != null;
        }
    }

    private void Update_GUI()
    {
        if (building.Is_Deleted) {
            Active = false;
            return;
        }
        Prototype_Container.SetActive(building.Is_Prototype);
        Instance_Container.SetActive(!building.Is_Prototype);
        Name_Text.text = building.Name;
        Image.sprite = SpriteManager.Instance.Get(building.Sprite.Name, building.Sprite.Type);
        if(current_row_id > 99999) {
            current_row_id = 0;
        }
        if (building.Is_Prototype) {
            foreach(GameObject row in cost_rows) {
                GameObject.Destroy(row);
            }
            cost_rows.Clear();
            foreach (GameObject row in upkeep_rows) {
                GameObject.Destroy(row);
            }
            upkeep_rows.Clear();

            Id_Text.text = string.Empty;
            Size_Text.text = string.Format("Size: {0}x{1}", building.Width, building.Height);
            Appeal_Text.text = string.Format("Appeal: {0}", Helper.Float_To_String(building.Appeal * (building.Width * building.Height), 2, true));
            Appeal_Range_Text.text = string.Format("Appeal range: {0}", Helper.Float_To_String(building.Appeal_Range, 1));

            //Cost
            if (building.Cash_Cost != 0) {
                GameObject cash_cost_row = GameObject.Instantiate(
                    Cost_Row_Prototype,
                    new Vector3(
                        Cost_Row_Prototype.transform.position.x,
                        Cost_Row_Prototype.transform.position.y,
                        Cost_Row_Prototype.transform.position.z
                    ),
                    Quaternion.identity,
                    Cost_Content.transform
                );
                cash_cost_row.name = "cash_cost_row_#" + current_row_id;
                current_row_id++;
                cash_cost_row.SetActive(true);
                Update_Cash_Row(cash_cost_row, building.Cash_Cost, 0, false, false);
                cost_rows.Add(cash_cost_row);
            }
            foreach(KeyValuePair<Resource, int> cost in building.Cost.OrderByDescending(x => x.Value).ToArray()) {
                GameObject cost_row = GameObject.Instantiate(
                    Cost_Row_Prototype,
                    new Vector3(
                        Cost_Row_Prototype.transform.position.x,
                        Cost_Row_Prototype.transform.position.y - (15.0f * cost_rows.Count),
                        Cost_Row_Prototype.transform.position.z
                    ),
                    Quaternion.identity,
                    Cost_Content.transform
                );
                cost_row.name = string.Format("{0}_cost_row_#{1}", cost.Key.ToString().ToLower(), current_row_id);
                current_row_id++;
                cost_row.SetActive(true);
                Update_Resource_Row(cost_row, cost.Key, cost.Value, 0, false, false);
                cost_rows.Add(cost_row);
            }
            if(cost_rows.Count == 0) {
                GameObject free_cost_row = GameObject.Instantiate(
                    Cost_Row_Prototype,
                    new Vector3(
                        Cost_Row_Prototype.transform.position.x,
                        Cost_Row_Prototype.transform.position.y,
                        Cost_Row_Prototype.transform.position.z
                    ),
                    Quaternion.identity,
                    Cost_Content.transform
                );
                free_cost_row.name = "free_cost_row_#" + current_row_id;
                current_row_id++;
                free_cost_row.SetActive(true);
                GameObject.Find(string.Format("{0}/AmountText", free_cost_row.name)).GetComponent<Text>().text = "none";
                GameObject.Find(string.Format("{0}/ResourceText", free_cost_row.name)).GetComponent<Text>().text = string.Empty;
                GameObject.Find(string.Format("{0}/IconImage", free_cost_row.name)).GetComponent<Image>().gameObject.SetActive(false);
                cost_rows.Add(free_cost_row);
            }
            Cost_Content.GetComponentInChildren<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (15.0f * cost_rows.Count) + 5.0f);

            //Upkeep
            if (building.Cash_Upkeep != 0.0f) {
                GameObject cash_upkeep_row = GameObject.Instantiate(
                    Upkeep_Row_Prototype,
                    new Vector3(
                        Upkeep_Row_Prototype.transform.position.x,
                        Upkeep_Row_Prototype.transform.position.y,
                        Upkeep_Row_Prototype.transform.position.z
                    ),
                    Quaternion.identity,
                    Upkeep_Content.transform
                );
                cash_upkeep_row.name = "cash_upkeep_row_#" + current_row_id;
                current_row_id++;
                cash_upkeep_row.SetActive(true);
                Update_Cash_Row(cash_upkeep_row, building.Cash_Upkeep, 2, true, true);
                upkeep_rows.Add(cash_upkeep_row);
            }
            foreach (KeyValuePair<Resource, float> upkeep in building.Upkeep.OrderByDescending(x => x.Value).ToArray()) {
                GameObject upkeep_row = GameObject.Instantiate(
                    Upkeep_Row_Prototype,
                    new Vector3(
                        Upkeep_Row_Prototype.transform.position.x,
                        Upkeep_Row_Prototype.transform.position.y - (15.0f * upkeep_rows.Count),
                        Upkeep_Row_Prototype.transform.position.z
                    ),
                    Quaternion.identity,
                    Upkeep_Content.transform
                );
                upkeep_row.name = string.Format("{0}_upkeep_row_#{1}", upkeep.Key.ToString().ToLower(), current_row_id);
                current_row_id++;
                upkeep_row.SetActive(true);
                Update_Resource_Row(upkeep_row, upkeep.Key, upkeep.Value, 2, true, true);
                upkeep_rows.Add(upkeep_row);
            }
            if (upkeep_rows.Count == 0) {
                GameObject free_upkeep_row = GameObject.Instantiate(
                    Upkeep_Row_Prototype,
                    new Vector3(
                        Upkeep_Row_Prototype.transform.position.x,
                        Upkeep_Row_Prototype.transform.position.y,
                        Upkeep_Row_Prototype.transform.position.z
                    ),
                    Quaternion.identity,
                    Upkeep_Content.transform
                );
                free_upkeep_row.name = "free_cost_row_#" + current_row_id;
                current_row_id++;
                free_upkeep_row.SetActive(true);
                GameObject.Find(string.Format("{0}/AmountText", free_upkeep_row.name)).GetComponent<Text>().text = "none";
                GameObject.Find(string.Format("{0}/ResourceText", free_upkeep_row.name)).GetComponent<Text>().text = string.Empty;
                GameObject.Find(string.Format("{0}/IconImage", free_upkeep_row.name)).GetComponent<Image>().gameObject.SetActive(false);
                upkeep_rows.Add(free_upkeep_row);
            }
            Upkeep_Content.GetComponentInChildren<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (15.0f * upkeep_rows.Count) + 5.0f);

        } else {
            Id_Text.text = string.Format("#{0}", building.Id);
            HP_Text.text = string.Format("HP: {0} / {1}", Helper.Float_To_String(building.HP, 0), building.Max_HP);
            if (!building.Is_Built) {
                Status_Text.text = string.Format("Construction: {0}%", Helper.Float_To_String(100.0f * (building.Construction_Progress / building.Construction_Time), 0));
            } else if (building.Is_Deconstructing) {
                Status_Text.text = string.Format("Deconstruction: {0}%", Helper.Float_To_String(100.0f * (building.Deconstruction_Progress / building.Construction_Time), 0));
            } else if (building.Is_Paused) {
                Status_Text.text = "Paused";
            } else if (building.Requires_Connection && !building.Is_Connected) {
                Status_Text.text = "Disconnected";
            } else if (building.Is_Operational) {
                Status_Text.text = "Operational";
            } else {
                Status_Text.text = "ERROR";
            }
            Efficency_Text.text = string.Format("Efficency: {0}%", Helper.Float_To_String(100.0f * building.Efficency, 0));

            if (!string.IsNullOrEmpty(building.Special_Status_Text_1)) {
                Special_Status_1_Text.gameObject.SetActive(true);
                Special_Status_1_Text.text = building.Special_Status_Text_1;
            } else {
                Special_Status_1_Text.gameObject.SetActive(false);
            }
            if (!string.IsNullOrEmpty(building.Special_Status_Text_2)) {
                Special_Status_2_Text.gameObject.SetActive(true);
                Special_Status_2_Text.text = building.Special_Status_Text_2;
            } else {
                Special_Status_2_Text.gameObject.SetActive(false);
            }

            //Workers
            if (building.Is_Built && !building.Is_Deconstructing && building.Max_Workers_Total != 0) {
                Workers_Container.SetActive(true);
                Residents_Container.SetActive(false);
                Taxed_By_Text.gameObject.SetActive(false);
                Worker_Peasant_Current.text = building.Current_Workers[Building.Resident.Peasant].ToString();
                Worker_Peasant_Max.text = building.Worker_Settings[Building.Resident.Peasant].ToString();
                Worker_Citizen_Current.text = building.Current_Workers[Building.Resident.Citizen].ToString();
                Worker_Citizen_Max.text = building.Worker_Settings[Building.Resident.Citizen].ToString();
                Worker_Noble_Current.text = building.Current_Workers[Building.Resident.Noble].ToString();
                Worker_Noble_Max.text = building.Worker_Settings[Building.Resident.Noble].ToString();
                Worker_Allocated_Current.text = building.Workers_Allocated.ToString();
                Worker_Allocated_Max.text = building.Max_Workers_Total.ToString();

                Worker_Peasant_Plus_Button.interactable = !building.Lock_Workers && building.Workers_Allocated < building.Max_Workers_Total && building.Worker_Settings[Building.Resident.Peasant] < building.Max_Workers[Building.Resident.Peasant];
                Worker_Citizen_Plus_Button.interactable = !building.Lock_Workers && building.Workers_Allocated < building.Max_Workers_Total && building.Worker_Settings[Building.Resident.Citizen] < building.Max_Workers[Building.Resident.Citizen];
                Worker_Noble_Plus_Button.interactable = !building.Lock_Workers && building.Workers_Allocated < building.Max_Workers_Total && building.Worker_Settings[Building.Resident.Noble] < building.Max_Workers[Building.Resident.Noble];
                Worker_Peasant_Minus_Button.interactable = !building.Lock_Workers && building.Workers_Allocated > 1 && building.Worker_Settings[Building.Resident.Peasant] > 0;
                Worker_Citizen_Minus_Button.interactable = !building.Lock_Workers && building.Workers_Allocated > 1 && building.Worker_Settings[Building.Resident.Citizen] > 0;
                Worker_Noble_Minus_Button.interactable = !building.Lock_Workers && building.Workers_Allocated > 1 && building.Worker_Settings[Building.Resident.Noble] > 0;
            } else if(building is Residence) {
                Workers_Container.SetActive(false);
                Residents_Container.SetActive(true);
                Taxed_By_Text.gameObject.SetActive(true);
                Residence residence = (Building as Residence);
                Residents_Peasant_Current.text = residence.Current_Residents[Building.Resident.Peasant].ToString();
                Residents_Peasant_Max.text = residence.Resident_Space[Building.Resident.Peasant].ToString();
                Residents_Peasant_Happiness.text = Helper.Float_To_String(100.0f * residence.Happiness[Building.Resident.Peasant], 0);
                if(residence.Happiness_Info[Building.Resident.Peasant].Count != 0) {
                    TooltipManager.Instance.Register_Tooltip(Residents_Peasant_Happiness.gameObject, Parse_Happiness_Tooltip(residence.Happiness_Info[Building.Resident.Peasant]), gameObject);
                } else {
                    TooltipManager.Instance.Unregister_Tooltip(Residents_Peasant_Happiness.gameObject);
                }
                Residents_Citizen_Current.text = residence.Current_Residents[Building.Resident.Citizen].ToString();
                Residents_Citizen_Max.text = residence.Resident_Space[Building.Resident.Citizen].ToString();
                Residents_Citizen_Happiness.text = Helper.Float_To_String(100.0f * residence.Happiness[Building.Resident.Citizen], 0);
                if (residence.Happiness_Info[Building.Resident.Citizen].Count != 0) {
                    TooltipManager.Instance.Register_Tooltip(Residents_Citizen_Happiness.gameObject, Parse_Happiness_Tooltip(residence.Happiness_Info[Building.Resident.Citizen]), gameObject);
                } else {
                    TooltipManager.Instance.Unregister_Tooltip(Residents_Citizen_Happiness.gameObject);
                }
                Residents_Noble_Current.text = residence.Current_Residents[Building.Resident.Noble].ToString();
                Residents_Noble_Max.text = residence.Resident_Space[Building.Resident.Noble].ToString();
                Residents_Noble_Happiness.text = Helper.Float_To_String(100.0f * residence.Happiness[Building.Resident.Noble], 0);
                if (residence.Happiness_Info[Building.Resident.Noble].Count != 0) {
                    TooltipManager.Instance.Register_Tooltip(Residents_Noble_Happiness.gameObject, Parse_Happiness_Tooltip(residence.Happiness_Info[Building.Resident.Noble]), gameObject);
                } else {
                    TooltipManager.Instance.Unregister_Tooltip(Residents_Noble_Happiness.gameObject);
                }
                if(residence.Taxed_By == -1) {
                    Taxed_By_Text.text = "Taxed by: none";
                } else {
                    Building tax_office = City.Instance.Buildings.FirstOrDefault(x => x.Id == residence.Taxed_By);
                    if(tax_office != null) {
                        Taxed_By_Text.text = string.Format("Taxed by:{0}{1} #{2}", Environment.NewLine, tax_office.Name, tax_office.Id);
                    } else {
                        CustomLogger.Instance.Error(string.Format("Building not found #{0}", residence.Taxed_By));
                        Taxed_By_Text.text = "Taxed by: ERROR";
                    }
                }
            } else {
                Workers_Container.SetActive(false);
                Residents_Container.SetActive(false);
                Taxed_By_Text.gameObject.SetActive(false);
            }

            //Delta
            foreach (GameObject row in delta_rows) {
                GameObject.Destroy(row);
            }
            delta_rows.Clear();

            if(building.Per_Day_Cash_Delta != 0.0f) {
                GameObject delta_row = GameObject.Instantiate(
                    Delta_Row_Prototype,
                    new Vector3(
                        Delta_Row_Prototype.transform.position.x,
                        Delta_Row_Prototype.transform.position.y - (15.0f * delta_rows.Count),
                        Delta_Row_Prototype.transform.position.z
                    ),
                    Quaternion.identity,
                    Delta_Content.transform
                );
                delta_row.name = "cash_delta_row_#" + current_row_id;
                current_row_id++;
                delta_row.SetActive(true);
                Update_Cash_Row(delta_row, building.Per_Day_Cash_Delta, 2, true, false);
                delta_rows.Add(delta_row);
            }

            foreach (KeyValuePair<Resource, float> resource in building.Per_Day_Resource_Delta) {
                if(resource.Value == 0.0f) {
                    continue;
                }
                GameObject delta_row = GameObject.Instantiate(
                    Delta_Row_Prototype,
                    new Vector3(
                        Delta_Row_Prototype.transform.position.x,
                        Delta_Row_Prototype.transform.position.y - (15.0f * delta_rows.Count),
                        Delta_Row_Prototype.transform.position.z
                    ),
                    Quaternion.identity,
                    Delta_Content.transform
                );
                delta_row.name = string.Format("{0}_delta_row_#{1}", resource.Key.ToString().ToLower(), current_row_id);
                current_row_id++;
                delta_row.SetActive(true);
                Update_Resource_Row(delta_row, resource.Key, resource.Value, 2, true, false);
                delta_rows.Add(delta_row);
            }
            Delta_Content.GetComponentInChildren<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (15.0f * delta_rows.Count) + 5.0f);

            Storage_Container.SetActive(!(Building is Residence));
            Services_Container.SetActive(Building is Residence);
            //Storage
            if (!(Building is Residence)) {
                foreach (GameObject row in storage_rows) {
                    GameObject.Destroy(row);
                }
                storage_rows.Clear();

                Storage_Text.text = building.Is_Storehouse ? string.Format("Storage ({0} / {1})", Helper.Float_To_String(building.Current_Storage_Amount, 0), building.Storage_Limit) : "Storage";

                foreach (KeyValuePair<Resource, float> resource in building.All_Resources) {
                    if (resource.Value == 0.0f) {
                        continue;
                    }
                    GameObject resource_row = GameObject.Instantiate(
                        Storage_Row_Prototype,
                        new Vector3(
                            Storage_Row_Prototype.transform.position.x,
                            Storage_Row_Prototype.transform.position.y - (15.0f * storage_rows.Count),
                            Storage_Row_Prototype.transform.position.z
                        ),
                        Quaternion.identity,
                        Storage_Content.transform
                    );
                    resource_row.name = string.Format("{0}_resource_row_#{1}", resource.Key.ToString().ToLower(), current_row_id);
                    current_row_id++;
                    resource_row.SetActive(true);
                    Update_Resource_Row(resource_row, resource.Key, resource.Value, 1, false, false);
                    storage_rows.Add(resource_row);
                }
                Storage_Content.GetComponentInChildren<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (15.0f * storage_rows.Count) + 5.0f);
            } else {
                Residence residence = building as Residence;
                //Services
                foreach (GameObject row in service_rows) {
                    GameObject.Destroy(row);
                }
                service_rows.Clear();

                foreach(Residence.ServiceType service in Enum.GetValues(typeof(Residence.ServiceType))) {
                    if (!residence.Consumes_Service(service)) {
                        continue;
                    }
                    GameObject service_row = GameObject.Instantiate(
                        Services_Row_Prototype,
                        new Vector3(
                            Services_Row_Prototype.transform.position.x,
                            Services_Row_Prototype.transform.position.y - (15.0f * service_rows.Count),
                            Services_Row_Prototype.transform.position.z
                        ),
                        Quaternion.identity,
                        Services_Content.transform
                    );
                    service_row.name = string.Format("{0}_service_row", service.ToString().ToLower());
                    service_row.SetActive(true);
                    service_row.GetComponentInChildren<Text>().text = string.Format("{0} {1} / 100 {2}%", Helper.Snake_Case_To_UI(service.ToString(), true), Helper.Float_To_String(100.0f * residence.Service_Level(service), 0), Helper.Float_To_String(100.0f * residence.Service_Quality(service), 0));
                    service_rows.Add(service_row);
                }
                Services_Content.GetComponentInChildren<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (15.0f * service_rows.Count) + 5.0f);
            }

            Pause_Button.interactable = building.Can_Be_Paused;
            Pause_Button.GetComponentInChildren<Text>().text = building.Is_Paused ? "Unpause" : "Pause";
            Delete_Button.interactable = building.Can_Be_Deleted;
            Settings_Button.interactable = building.Is_Complete && (building.Is_Storehouse || building.Special_Settings.Count > 0 || building.Tags.Contains(Building.Tag.Land_Trade) ||
                building.Tags.Contains(Building.Tag.Water_Trade) || building.Tags.Contains(Building.Tag.Is_Distribution_Depot) || (building.Tags.Contains(Building.Tag.Creates_Expeditions) &&
                building.Has_Functional_Dock() && building.Is_Operational));

            //Highlights
            float range = Mathf.Max(new float[3] { building.Range, building.Construction_Range, (building is Residence) ? ((building as Residence).Peasants_Only ? 0.0f : Residence.DIRT_ROAD_RANGE) : 0.0f });
            if (range > 0.0f) {
                List<Tile> tiles_in_range = building.Get_Tiles_In_Circle(range);
                List<Tile> special_highlight_tiles = building.On_Highlight != null ? building.On_Highlight(building) : new List<Tile>();
                foreach (Tile t in tiles_in_range) {
                    if (t.Building != building) {
                        t.Highlight = special_highlight_tiles.Contains(t) ? new Color(0.35f, 0.35f, 1.0f, 1.0f) : new Color(0.45f, 0.45f, 1.0f, 1.0f);
                        highlighted_tiles.Add(t);
                    }
                }
            }
            if (building.Road_Range > 0) {
                Dictionary<Building, int> connected_buildings = building.Get_Connected_Buildings(building.Road_Range);
                foreach (KeyValuePair<Building, int> pair in connected_buildings) {
                    if (pair.Key != building) {
                        foreach (Tile t in pair.Key.Tiles) {
                            t.Highlight = new Color(0.35f, 0.35f, 0.35f, 1.0f);
                            t.Show_Text(pair.Value.ToString());
                            highlighted_tiles.Add(t);
                        }
                    }
                }
            }
        }
    }

    public void Pause()
    {
        if(building == null || !building.Can_Be_Paused) {
            return;
        }
        building.Is_Paused = !building.Is_Paused;
        Update_GUI();
    }

    public void Delete()
    {
        if(building == null || !building.Can_Be_Deleted) {
            return;
        }
        building.Deconstruct();
        Update_GUI();
    }

    private void Add_Peasant_Worker()
    {
        if(building != null && building.Worker_Settings[Building.Resident.Peasant] < building.Max_Workers[Building.Resident.Peasant] && building.Workers_Allocated < building.Max_Workers_Total) {
            building.Worker_Settings[Building.Resident.Peasant]++;
        }
    }

    private void Remove_Peasant_Worker()
    {
        if (building != null && building.Worker_Settings[Building.Resident.Peasant] > 0 && building.Workers_Allocated > 1) {
            building.Worker_Settings[Building.Resident.Peasant]--;
        }
    }

    private void Add_Citizen_Worker()
    {
        if (building != null && building.Worker_Settings[Building.Resident.Citizen] < building.Max_Workers[Building.Resident.Citizen] && building.Workers_Allocated < building.Max_Workers_Total) {
            building.Worker_Settings[Building.Resident.Citizen]++;
        }
    }

    private void Remove_Citizen_Worker()
    {
        if (building != null && building.Worker_Settings[Building.Resident.Citizen] > 0 && building.Workers_Allocated > 1) {
            building.Worker_Settings[Building.Resident.Citizen]--;
        }
    }

    private void Add_Noble_Worker()
    {
        if (building != null && building.Worker_Settings[Building.Resident.Noble] < building.Max_Workers[Building.Resident.Noble] && building.Workers_Allocated < building.Max_Workers_Total) {
            building.Worker_Settings[Building.Resident.Noble]++;
        }
    }

    private void Remove_Noble_Worker()
    {
        if (building != null && building.Worker_Settings[Building.Resident.Noble] > 0 && building.Workers_Allocated > 1) {
            building.Worker_Settings[Building.Resident.Noble]--;
        }
    }

    private void Show_Settings()
    {
        if(building == null) {
            return;
        }
        if(building.Is_Storehouse) {
            StorageSettingsGUIManager.Instance.Show(building);
        } else if(building.Special_Settings.Count != 0) {
            SpecialSettingsGUIManager.Instance.Show(building);
        } else if (building.Tags.Contains(Building.Tag.Land_Trade) || building.Tags.Contains(Building.Tag.Water_Trade)) {
            TradeGUIManager.Instance.Show(building);
        } else if (building.Tags.Contains(Building.Tag.Creates_Expeditions)) {
            NewExpeditionGUIManager.Instance.Show(building);
        } else if (building.Tags.Contains(Building.Tag.Is_Distribution_Depot)) {
            DistributionDepotGUI.Instance.Show(building);
        }
    }

    private string Parse_Happiness_Tooltip(List<string> rows)
    {
        StringBuilder builder = new StringBuilder();
        for(int i = 0; i < rows.Count; i++) {
            builder.Append(rows[i]);
            if(i != rows.Count - 1) {
                builder.Append(Environment.NewLine);
            }
        }
        return builder.ToString();
    }

    private void Update_Cash_Row(GameObject row, float amount, int decimals, bool plus_sign, bool negative)
    {
        if (negative) {
            amount *= -1.0f;
        }
        GameObject.Find(string.Format("{0}/AmountText", row.name)).GetComponent<Text>().text = Helper.Float_To_String(amount, decimals, plus_sign);
        GameObject.Find(string.Format("{0}/ResourceText", row.name)).GetComponent<Text>().text = "Cash";
        GameObject.Find(string.Format("{0}/IconImage", row.name)).GetComponent<Image>().sprite = SpriteManager.Instance.Get("gold", SpriteManager.SpriteType.UI);
    }

    private void Update_Resource_Row(GameObject row, Resource resource, float amount, int decimals, bool plus_sign, bool negative)
    {
        if (negative) {
            amount *= -1.0f;
        }
        GameObject.Find(string.Format("{0}/AmountText", row.name)).GetComponent<Text>().text = Helper.Float_To_String(amount, decimals, plus_sign);
        GameObject.Find(string.Format("{0}/ResourceText", row.name)).GetComponent<Text>().text = resource.UI_Name;
        if (resource.Has_Sprite) {
            GameObject.Find(string.Format("{0}/IconImage", row.name)).GetComponent<Image>().sprite = SpriteManager.Instance.Get(resource.Sprite_Name, resource.Sprite_Type);
        } else {
            GameObject.Find(string.Format("{0}/IconImage", row.name)).GetComponent<Image>().gameObject.SetActive(false);
        }
    }
}
