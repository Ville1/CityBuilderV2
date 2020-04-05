using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ContactsGUIManager : MonoBehaviour {
    public static ContactsGUIManager Instance { get; private set; }

    public GameObject Panel;

    public GameObject Contact_Row_Prototype;
    public GameObject Content;
    public GameObject Side_Panel;
    public Text Name_Text;
    public Text Type_Text;
    public Text Route_Text;
    public Text Relations_Text;
    public Text Discount_Text;
    public GameObject Export_Row_Prototype;
    public GameObject Export_Content;
    public GameObject Import_Row_Prototype;
    public GameObject Import_Content;

    private long current_row_id;
    private Dictionary<ForeignCity, GameObject> rows;
    private List<GameObject> export_rows;
    private List<GameObject> import_rows;

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
        Contact_Row_Prototype.SetActive(false);
        Export_Row_Prototype.SetActive(false);
        Import_Row_Prototype.SetActive(false);
        Side_Panel.SetActive(false);
        current_row_id = 0;
        rows = new Dictionary<ForeignCity, GameObject>();
        export_rows = new List<GameObject>();
        import_rows = new List<GameObject>();
    }

    /// <summary>
    /// Per frame update
    /// </summary>
    private void Update()
    {
        if (Active) {
            foreach(KeyValuePair<ForeignCity, GameObject> pair in rows) {
                GameObject.Find(string.Format("{0}/RelationsText", pair.Value.name)).GetComponent<Text>().text = Helper.Float_To_String(pair.Key.Relations * 100.0f, 0, true) + "%";
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
                Update_GUI();
            } else {
                Side_Panel.SetActive(false);
            }
        }
    }

    private void Update_GUI()
    {
        foreach (KeyValuePair<ForeignCity, GameObject> row in rows) {
            GameObject.Destroy(row.Value.gameObject);
        }
        rows.Clear();
        
        foreach (ForeignCity city in Contacts.Instance.Cities) {
            GameObject row = GameObject.Instantiate(
                Contact_Row_Prototype,
                new Vector3(
                    Contact_Row_Prototype.transform.position.x,
                    Contact_Row_Prototype.transform.position.y - (25.0f * rows.Count),
                    Contact_Row_Prototype.transform.position.z
                ),
                Quaternion.identity,
                Content.transform
            );
            row.gameObject.SetActive(true);
            row.name = string.Format("row_{0}", current_row_id);
            current_row_id++;
            if(current_row_id > 99999) {
                current_row_id = 0;
            }

            GameObject.Find(string.Format("{0}/NameText", row.name)).GetComponent<Text>().text = city.Name;
            GameObject.Find(string.Format("{0}/RelationsText", row.name)).GetComponent<Text>().text = Helper.Float_To_String(city.Relations * 100.0f, 0, true) + "%";

            Button.ButtonClickedEvent click = new Button.ButtonClickedEvent();
            click.AddListener(new UnityAction(delegate () { Select_City(city); }));
            row.GetComponentInChildren<Button>().onClick = click;

            rows.Add(city, row);
        }
        Content.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (25.0f * rows.Count) + 5.0f);
    }

    private void Select_City(ForeignCity city)
    {
        Side_Panel.SetActive(true);
        Name_Text.text = city.Name;
        Type_Text.text = Helper.Snake_Case_To_UI(city.City_Type.ToString(), true);
        Relations_Text.text = Helper.Float_To_String(city.Relations * 100.0f, 0, true) + "%";
        Discount_Text.text = city.Discount.HasValue ? Helper.Float_To_String(city.Discount.Value * 100.0f, 0) + "%" : "-";

        switch (city.Trade_Route_Type) {
            case ForeignCity.TradeRouteType.Both:
                Route_Text.text = "Land and water";
                break;
            case ForeignCity.TradeRouteType.Land:
                Route_Text.text = "Land";
                break;
            case ForeignCity.TradeRouteType.Water:
                Route_Text.text = "Water";
                break;
        }

        foreach (GameObject row in export_rows) {
            GameObject.Destroy(row.gameObject);
        }
        export_rows.Clear();
        foreach (GameObject row in import_rows) {
            GameObject.Destroy(row.gameObject);
        }
        import_rows.Clear();

        foreach (Resource resource in city.Cheap_Exports) {
            Add_Export_Row(city, resource, "Cheap");
        }
        foreach (Resource resource in city.Exports) {
            Add_Export_Row(city, resource, "Normal");
        }
        foreach (Resource resource in city.Expensive_Exports) {
            Add_Export_Row(city, resource, "Expensive");
        }
        Export_Content.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (25.0f * export_rows.Count) + 5.0f);

        foreach (Resource resource in city.Preferred_Imports) {
            Add_Import_Row(city, resource, "High");
        }
        foreach (Resource resource in city.Disliked_Imports) {
            Add_Import_Row(city, resource, "Low");
        }
        foreach (Resource resource in city.Unaccepted_Imports) {
            Add_Import_Row(city, resource, "Unaccepted");
        }
        Import_Content.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (25.0f * import_rows.Count) + 5.0f);
    }

    private void Add_Export_Row(ForeignCity city, Resource resource, string price_label)
    {
        GameObject row = GameObject.Instantiate(
                Export_Row_Prototype,
                new Vector3(
                    Export_Row_Prototype.transform.position.x,
                    Export_Row_Prototype.transform.position.y - (20.0f * export_rows.Count),
                    Export_Row_Prototype.transform.position.z
                ),
                Quaternion.identity,
                Export_Content.transform
            );
        row.gameObject.SetActive(true);
        row.name = string.Format("export_row_{0}", current_row_id);
        current_row_id++;
        if (current_row_id > 99999) {
            current_row_id = 0;
        }
        
        if (resource.Has_Sprite) {
            GameObject.Find(string.Format("{0}/IconImage", row.name)).GetComponent<Image>().sprite = SpriteManager.Instance.Get(resource.Sprite_Name, resource.Sprite_Type);
        } else {
            GameObject.Find(string.Format("{0}/IconImage", row.name)).SetActive(false);
        }
        GameObject.Find(string.Format("{0}/NameText", row.name)).GetComponent<Text>().text = resource.UI_Name;
        GameObject.Find(string.Format("{0}/PriceText", row.name)).GetComponent<Text>().text = Helper.Float_To_String(city.Get_Export_Price(resource), 2);
        GameObject.Find(string.Format("{0}/PriceDetailText", row.name)).GetComponent<Text>().text = price_label;

        export_rows.Add(row);
    }

    private void Add_Import_Row(ForeignCity city, Resource resource, string price_label)
    {
        GameObject row = GameObject.Instantiate(
                Import_Row_Prototype,
                new Vector3(
                    Import_Row_Prototype.transform.position.x,
                    Import_Row_Prototype.transform.position.y - (20.0f * import_rows.Count),
                    Import_Row_Prototype.transform.position.z
                ),
                Quaternion.identity,
                Import_Content.transform
            );
        row.gameObject.SetActive(true);
        row.name = string.Format("import_row_{0}", current_row_id);
        current_row_id++;
        if (current_row_id > 99999) {
            current_row_id = 0;
        }

        if (resource.Has_Sprite) {
            GameObject.Find(string.Format("{0}/IconImage", row.name)).GetComponent<Image>().sprite = SpriteManager.Instance.Get(resource.Sprite_Name, resource.Sprite_Type);
        } else {
            GameObject.Find(string.Format("{0}/IconImage", row.name)).SetActive(false);
        }
        GameObject.Find(string.Format("{0}/NameText", row.name)).GetComponent<Text>().text = resource.UI_Name;
        GameObject.Find(string.Format("{0}/PriceText", row.name)).GetComponent<Text>().text = Helper.Float_To_String(city.Get_Import_Price(resource), 2);
        GameObject.Find(string.Format("{0}/PriceDetailText", row.name)).GetComponent<Text>().text = price_label;

        import_rows.Add(row);
    }
}
