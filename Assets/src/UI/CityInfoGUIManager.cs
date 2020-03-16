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

    public GameObject Resources_Tab_Panel;
    public GameObject Resources_Content;
    public GameObject Resource_Row_Prototype;

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
            }
        }
    }

    public void Open_Overview_Tab()
    {
        Overview_Tab_Button.interactable = false;
        Resources_Tab_Button.interactable = true;
        Overview_Tab_Panel.SetActive(true);
        Resources_Tab_Panel.SetActive(false);
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
    }

    private void Update_Resources()
    {
        if(resource_rows.Count == 0) {
            foreach(Resource resouce in Enum.GetValues(typeof(Resource))) {
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
                GameObject.Find(string.Format("{0}/NameText", row.name)).GetComponent<Text>().text = resouce.ToString();
                if (City.Resource_Icons.ContainsKey(resouce)) {
                    GameObject.Find(string.Format("{0}/IconImage", row.name)).GetComponent<Image>().sprite = SpriteManager.Instance.Get(City.Resource_Icons[resouce].Name, City.Resource_Icons[resouce].Type);
                } else {
                    GameObject.Find(string.Format("{0}/IconImage", row.name)).SetActive(false);
                }
                resource_rows.Add(resouce, row);
            }
        }

        foreach(Resource resource in Enum.GetValues(typeof(Resource))) {
            GameObject row = resource_rows[resource];
            GameObject.Find(string.Format("{0}/CurrentText", row.name)).GetComponent<Text>().text = Helper.Float_To_String(City.Instance.Resource_Totals[resource], 0);
            GameObject.Find(string.Format("{0}/MaxText", row.name)).GetComponent<Text>().text = Helper.Float_To_String(City.Instance.Resource_Max_Storage[resource], 0);
            GameObject.Find(string.Format("{0}/DeltaText", row.name)).GetComponent<Text>().text = Helper.Float_To_String(City.Instance.Resource_Delta[resource], 1, true);
        }
    }
}
