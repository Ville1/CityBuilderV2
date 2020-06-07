using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CityInfoGUIManager : MonoBehaviour {
    public static CityInfoGUIManager Instance { get; private set; }

    public GameObject Panel;

    public Button Overview_Tab_Button;
    public Button Resources_Tab_Button;

    public GameObject Overview_Tab_Panel;
    public Text City_Name_Text;
    public Text Cash_Text;
    public Text Income_Text;
    public Text Food_Current_Text;
    public Text Food_Max_Text;
    public Text Food_Produced_Text;
    public Text Food_Consumed_Text;
    public Text Food_Delta_Text;

    public Text Peasant_Happiness_Text;
    public Text Peasant_Education_Text;
    public Text Peasant_Health_Text;
    public Text Peasant_Efficency_Text;

    public Text Citizen_Happiness_Text;
    public Text Citizen_Education_Text;
    public Text Citizen_Health_Text;
    public Text Citizen_Efficency_Text;

    public Text Noble_Happiness_Text;
    public Text Noble_Education_Text;
    public Text Noble_Health_Text;
    public Text Noble_Efficency_Text;

    public GameObject Resources_Tab_Panel;
    public GameObject Resources_Content;
    public GameObject Resource_Row_Prototype;

    public GameObject Resource_Info_Panel;
    public Text Resource_Name_Text;
    public Image Resource_Icon_Image_Text;
    public Text Resource_Value_Text;
    public GameObject Resource_Food_Info;
    public Text Resource_Food_Quality;
    public Text Resource_Food_Type;
    public GameObject Resource_Fuel_Info;
    public Text Resource_Fuel_Value;

    private Dictionary<Resource, GameObject> resource_rows;

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

        Overview_Tab_Button.interactable = false;
        Resources_Tab_Button.interactable = false;
        Overview_Tab_Panel.SetActive(false);
        Resources_Tab_Panel.SetActive(false);
        Resource_Row_Prototype.SetActive(false);
        Resource_Info_Panel.SetActive(false);
        resource_rows = new Dictionary<Resource, GameObject>();
    }

    /// <summary>
    /// Per frame update
    /// </summary>
    private void Update()
    {
        if (Active) {
            if (Overview_Tab_Panel.activeSelf) {
                Update_Overview();
            } else if (Resources_Tab_Panel.activeSelf) {
                Update_Resources();
            }
        }
    }

    public bool Active
    {
        get {
            return Panel.activeSelf;
        }
        set {
            if(Panel.activeSelf == value) {
                return;
            }
            Panel.SetActive(value);
            if (value) {
                Open_Overview_Tab();
                MasterUIManager.Instance.Close_Others(GetType().Name);
            } else {
                Resource_Info_Panel.SetActive(false);
            }
        }
    }

    public void Open_Overview_Tab()
    {
        Overview_Tab_Button.interactable = false;
        Resources_Tab_Button.interactable = true;
        Overview_Tab_Panel.SetActive(true);
        Resources_Tab_Panel.SetActive(false);
        Resource_Info_Panel.SetActive(false);
    }

    public void Open_Resources_Tab()
    {
        Overview_Tab_Button.interactable = true;
        Resources_Tab_Button.interactable = false;
        Overview_Tab_Panel.SetActive(false);
        Resources_Tab_Panel.SetActive(true);
    }

    private void Update_Overview()
    {
        City_Name_Text.text = City.Instance.Name;
        Cash_Text.text = Helper.Float_To_String(City.Instance.Cash, 1);
        Income_Text.text = Helper.Float_To_String(City.Instance.Cash_Delta, 2, true);
        Food_Current_Text.text = Helper.Float_To_String(City.Instance.Food_Current, 0);
        Food_Max_Text.text = Helper.Float_To_String(City.Instance.Food_Max, 0);
        Food_Produced_Text.text = Helper.Float_To_String(City.Instance.Food_Produced, 1, true);
        Food_Consumed_Text.text = Helper.Float_To_String(City.Instance.Food_Consumed, 1);
        Food_Delta_Text.text = Helper.Float_To_String(City.Instance.Food_Delta, 1, true);
        //Peasants
        Peasant_Happiness_Text.text = Helper.Float_To_String(City.Instance.Happiness[Building.Resident.Peasant] * 100.0f, 0);
        Peasant_Education_Text.text = "-";
        Peasant_Health_Text.text = "WIP";
        Peasant_Efficency_Text.text = string.Format("{0}%", Helper.Float_To_String(Residence.Get_Efficency(Building.Resident.Peasant) * 100.0f, 1));
        //Citizens
        Citizen_Happiness_Text.text = Helper.Float_To_String(City.Instance.Happiness[Building.Resident.Citizen] * 100.0f, 0);
        Citizen_Education_Text.text = string.Format("{0}%", Helper.Float_To_String(City.Instance.Education[Building.Resident.Citizen] * 100.0f, 1));
        Citizen_Health_Text.text = "WIP";
        Citizen_Efficency_Text.text = string.Format("{0}%", Helper.Float_To_String(Residence.Get_Efficency(Building.Resident.Citizen) * 100.0f, 1));
        //Nobles
        Noble_Happiness_Text.text = Helper.Float_To_String(City.Instance.Happiness[Building.Resident.Noble] * 100.0f, 0);
        Noble_Education_Text.text = string.Format("{0}%", Helper.Float_To_String(City.Instance.Education[Building.Resident.Noble] * 100.0f, 1));
        Noble_Health_Text.text = "WIP";
        Noble_Efficency_Text.text = string.Format("{0}%", Helper.Float_To_String(Residence.Get_Efficency(Building.Resident.Noble) * 100.0f, 1));
    }

    private void Update_Resources()
    {
        if(resource_rows.Count == 0) {
            foreach(Resource resouce in Resource.All) {
                GameObject row = GameObject.Instantiate(
                    Resource_Row_Prototype,
                    new Vector3(
                        Resource_Row_Prototype.transform.position.x,
                        Resource_Row_Prototype.transform.position.y - (30.0f * resource_rows.Count),
                        Resource_Row_Prototype.transform.position.z
                    ),
                    Quaternion.identity,
                    Resources_Content.transform
                );
                row.SetActive(true);
                row.name = string.Format("{0}_row", resouce.ToString().ToLower());
                GameObject.Find(string.Format("{0}/NameText", row.name)).GetComponent<Text>().text = resouce.UI_Name;
                if (resouce.Has_Sprite) {
                    GameObject.Find(string.Format("{0}/IconImage", row.name)).GetComponent<Image>().sprite = SpriteManager.Instance.Get(resouce.Sprite_Name, resouce.Sprite_Type);
                } else {
                    GameObject.Find(string.Format("{0}/IconImage", row.name)).SetActive(false);
                }
                resource_rows.Add(resouce, row);
            }
        }
        Resources_Content.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (resource_rows.Count * 30.0f) + 5.0f);

        foreach (Resource resource in Resource.All) {
            GameObject row = resource_rows[resource];
            GameObject.Find(string.Format("{0}/CurrentText", row.name)).GetComponent<Text>().text = Helper.Float_To_String(City.Instance.Resource_Totals[resource], 0);
            GameObject.Find(string.Format("{0}/MaxText", row.name)).GetComponent<Text>().text = Helper.Float_To_String(City.Instance.Resource_Max_Storage[resource], 0);
            GameObject.Find(string.Format("{0}/DeltaText", row.name)).GetComponent<Text>().text = Helper.Float_To_String(City.Instance.Resource_Delta[resource], 1, true);

            Button.ButtonClickedEvent on_click_event = new Button.ButtonClickedEvent();
            on_click_event.AddListener(new UnityEngine.Events.UnityAction(delegate () {
                Show_Resource_Info(resource);
            }));
            GameObject.Find(string.Format("{0}/InvisibleButton", row.name)).GetComponent<Button>().onClick = on_click_event;

        }
    }

    private void Show_Resource_Info(Resource resource)
    {
        Resource_Info_Panel.SetActive(true);
        Resource_Name_Text.text = resource.UI_Name;
        if (resource.Has_Sprite) {
            Resource_Icon_Image_Text.sprite = SpriteManager.Instance.Get(resource.Sprite_Name, resource.Sprite_Type);
        } else {
            Resource_Icon_Image_Text.sprite = SpriteManager.Instance.Get("placeholder", SpriteManager.SpriteType.UI);
        }
        Resource_Value_Text.text = Helper.Float_To_String(resource.Value, 2);
        Resource_Food_Info.SetActive(resource.Is_Food);
        if (resource.Is_Food) {
            Resource_Food_Quality.text = Helper.Float_To_String(resource.Food_Quality, 2);
            Resource_Food_Type.text = Helper.Snake_Case_To_UI(resource.Food_Type.ToString(), false);
        }
        Resource_Fuel_Info.SetActive(resource.Is_Fuel);
        if (resource.Is_Fuel) {
            Resource_Fuel_Value.text = Helper.Float_To_String(resource.Fuel_Value, 2);
        }
    }
}
