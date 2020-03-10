using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class InspectorManager : MonoBehaviour {
    public static InspectorManager Instance { get; private set; }

    public GameObject Panel;

    public Text Name_Text;
    public Image Image;

    public GameObject Instance_Container;
    public Text HP_Text;
    public Text Status_Text;
    public Text Efficency_Text;
    public Text Storage_Text;
    public GameObject Storage_Content;
    public GameObject Storage_Row_Prototype;
    public Button Pause_Button;
    public Button Delete_Button;

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
    private List<GameObject> storage_rows;

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
        Storage_Row_Prototype.SetActive(false);
        cost_rows = new List<GameObject>();
        upkeep_rows = new List<GameObject>();
        storage_rows = new List<GameObject>();

        Button.ButtonClickedEvent click = new Button.ButtonClickedEvent();
        click.AddListener(new UnityAction(Pause));
        Pause_Button.onClick = click;

        Button.ButtonClickedEvent click2 = new Button.ButtonClickedEvent();
        click2.AddListener(new UnityAction(Delete));
        Delete_Button.onClick = click2;
    }

    /// <summary>
    /// Per frame update
    /// </summary>
    private void Update()
    {
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
        Image.sprite = SpriteManager.Instance.Get(building.Sprite, SpriteManager.SpriteType.Building);
        if (building.Is_Prototype) {
            foreach(GameObject row in cost_rows) {
                GameObject.Destroy(row);
            }
            cost_rows.Clear();
            foreach (GameObject row in upkeep_rows) {
                GameObject.Destroy(row);
            }
            upkeep_rows.Clear();

            Size_Text.text = string.Format("Size: {0}x{1}", building.Width, building.Height);
            
            //Cost
            if(building.Cash_Cost != 0) {
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
                cash_cost_row.name = "cash_cost_row";
                cash_cost_row.SetActive(true);
                cash_cost_row.GetComponentInChildren<Text>().text = string.Format("{0} cash", building.Cash_Cost);
                cost_rows.Add(cash_cost_row);
            }
            foreach(KeyValuePair<Resource, int> cost in building.Cost) {
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
                cost_row.name = string.Format("{0}_cost_row", cost.Key.ToString().ToLower());
                cost_row.SetActive(true);
                cost_row.GetComponentInChildren<Text>().text = string.Format("{0}x {1}", cost.Value, cost.Key.ToString().ToLower());
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
                free_cost_row.name = "free_cost_row";
                free_cost_row.SetActive(true);
                free_cost_row.GetComponentInChildren<Text>().text = "none";
                cost_rows.Add(free_cost_row);
            }
            Cost_Content.GetComponentInChildren<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 15.0f * cost_rows.Count);

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
                cash_upkeep_row.name = "cash_upkeep_row";
                cash_upkeep_row.SetActive(true);
                cash_upkeep_row.GetComponentInChildren<Text>().text = string.Format("{0} cash", Helper.Float_To_String(building.Cash_Upkeep, 1));
                upkeep_rows.Add(cash_upkeep_row);
            }
            foreach (KeyValuePair<Resource, float> upkeep in building.Upkeep) {
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
                upkeep_row.name = string.Format("{0}_upkeep_row", upkeep.Key.ToString().ToLower());
                upkeep_row.SetActive(true);
                upkeep_row.GetComponentInChildren<Text>().text = string.Format("{0} {1}", Helper.Float_To_String(upkeep.Value, 1), upkeep.Key.ToString().ToLower());
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
                free_upkeep_row.name = "free_cost_row";
                free_upkeep_row.SetActive(true);
                free_upkeep_row.GetComponentInChildren<Text>().text = "none";
                upkeep_rows.Add(free_upkeep_row);
            }
            Upkeep_Content.GetComponentInChildren<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 15.0f * upkeep_rows.Count);

        } else {
            HP_Text.text = string.Format("HP: {0} / {1}", Helper.Float_To_String(building.HP, 0), building.Max_HP);
            if (!building.Is_Built) {
                Status_Text.text = string.Format("Construction: {0}%", Helper.Float_To_String(100.0f * (building.Construction_Progress / building.Construction_Time), 0));
            } else if (building.Is_Deconstructing) {
                Status_Text.text = string.Format("Deconstruction: {0}%", Helper.Float_To_String(100.0f * (building.Deconstruction_Progress / building.Construction_Time), 0));
            } else if (building.Is_Paused) {
                Status_Text.text = "Paused";
            } else if (building.Is_Operational) {
                Status_Text.text = "Operational";
            } else {
                Status_Text.text = "ERROR";
            }
            Efficency_Text.text = string.Format("Efficency: {0}%", Helper.Float_To_String(100.0f * building.Efficency, 0));
            
            foreach (GameObject row in storage_rows) {
                GameObject.Destroy(row);
            }
            storage_rows.Clear();

            Storage_Text.text = building.Is_Storehouse ? string.Format("Storage ({0} / {1})", Helper.Float_To_String(building.Current_Storage_Amount, 0), building.Storage_Limit) : "Storage";

            foreach (KeyValuePair<Resource, float> resource in building.All_Resources) {
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
                resource_row.name = string.Format("{0}_resource_row", resource.Key.ToString().ToLower());
                resource_row.SetActive(true);
                resource_row.GetComponentInChildren<Text>().text = string.Format("{0} {1}", Helper.Float_To_String(resource.Value, 1), resource.Key.ToString().ToLower());
                storage_rows.Add(resource_row);
            }
            Storage_Content.GetComponentInChildren<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 15.0f * storage_rows.Count);

            Pause_Button.interactable = building.Can_Be_Paused;
            Pause_Button.GetComponentInChildren<Text>().text = building.Is_Paused ? "Unpause" : "Pause";
            Delete_Button.interactable = building.Can_Be_Deleted;
        }
    }

    private void Pause()
    {
        if(building == null || !building.Can_Be_Paused) {
            return;
        }
        building.Is_Paused = !building.Is_Paused;
        Update_GUI();
    }

    private void Delete()
    {
        if(building == null || !building.Can_Be_Deleted) {
            return;
        }
        building.Deconstruct();
        Update_GUI();
    }
}
