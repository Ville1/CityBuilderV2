using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NewColonyGUIManager : MonoBehaviour {
    public static NewColonyGUIManager Instance { get; private set; }

    public GameObject Panel;

    public Dropdown Location_Dropdown;
    public Text Count_Text;
    public InputField Name_Input;
    public Text Route_Type_Text;
    public GameObject Exports_Content;
    public GameObject Exports_Row_Prototype;
    public GameObject Imports_Content;
    public GameObject Imports_Row_Prototype;
    public Text Cost_Text;
    public Button Ok_Button;

    private Building harbor;
    private ColonyLocation location;
    private RowScrollView<Resource.ResourceType> exports_scroll_view;
    private RowScrollView<Resource.ResourceType> imports_scroll_view;
    private Color default_text_color;

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
        exports_scroll_view = new RowScrollView<Resource.ResourceType>("exports_scroll_view", Exports_Content, Exports_Row_Prototype, 20.0f);
        imports_scroll_view = new RowScrollView<Resource.ResourceType>("imports_scroll_view", Imports_Content, Imports_Row_Prototype, 20.0f);
        default_text_color = Count_Text.color;
        Active = false;
    }

    /// <summary>
    /// Per frame update
    /// </summary>
    private void Update()
    {

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
                Initialize_GUI();
            }
        }
    }

    public void Show(Building harbor)
    {
        if(City.Instance.Colony_Locations.Count == 0) {
            MessageManager.Instance.Show_Message("You haven't found any colony locations yet");
            return;
        }
        Active = true;
        this.harbor = harbor;
        location = City.Instance.Colony_Locations[0];
        Update_GUI();
    }

    public void Location_Changed()
    {
        location = City.Instance.Colony_Locations[Location_Dropdown.value];
        Update_GUI();
    }

    public void Update_GUI()
    {
        switch (location.Trade_Route_Type) {
            case ForeignCity.TradeRouteType.Both:
                Route_Type_Text.text = "Land/Water";
                break;
            case ForeignCity.TradeRouteType.Land:
                Route_Type_Text.text = "Land";
                break;
            case ForeignCity.TradeRouteType.Water:
                Route_Type_Text.text = "Water";
                break;
        }
        Count_Text.text = string.Format("{0} / {1}", City.Instance.Colonies.Count, City.Instance.Max_Colonies);
        Count_Text.color = City.Instance.Colonies.Count >= City.Instance.Max_Colonies ? Color.red : default_text_color;

        exports_scroll_view.Clear();
        foreach(Resource export in location.Cheap_Exports) {
            exports_scroll_view.Add(export.Type, new List<UIElementData>() {
                new UIElementData("IconImage", export.Has_Sprite ? export.Sprite_Name : "empty", export.Has_Sprite ? export.Sprite_Type : SpriteManager.SpriteType.UI),
                new UIElementData("ResourceNameText", export.UI_Name),
                new UIElementData("PriceText", "Cheap")
            });
        }
        foreach (Resource export in location.Exports) {
            exports_scroll_view.Add(export.Type, new List<UIElementData>() {
                new UIElementData("IconImage", export.Has_Sprite ? export.Sprite_Name : "empty", export.Has_Sprite ? export.Sprite_Type : SpriteManager.SpriteType.UI),
                new UIElementData("ResourceNameText", export.UI_Name),
                new UIElementData("PriceText", "Normal")
            });
        }
        foreach (Resource export in location.Expensive_Exports) {
            exports_scroll_view.Add(export.Type, new List<UIElementData>() {
                new UIElementData("IconImage", export.Has_Sprite ? export.Sprite_Name : "empty", export.Has_Sprite ? export.Sprite_Type : SpriteManager.SpriteType.UI),
                new UIElementData("ResourceNameText", export.UI_Name),
                new UIElementData("PriceText", "Expensive")
            });
        }

        imports_scroll_view.Clear();
        foreach (Resource import in location.Preferred_Imports) {
            imports_scroll_view.Add(import.Type, new List<UIElementData>() {
                new UIElementData("IconImage", import.Has_Sprite ? import.Sprite_Name : "empty", import.Has_Sprite ? import.Sprite_Type : SpriteManager.SpriteType.UI),
                new UIElementData("ResourceNameText", import.UI_Name),
                new UIElementData("PriceText", "High")
            });
        }
        foreach (Resource import in location.Disliked_Imports) {
            imports_scroll_view.Add(import.Type, new List<UIElementData>() {
                new UIElementData("IconImage", import.Has_Sprite ? import.Sprite_Name : "empty", import.Has_Sprite ? import.Sprite_Type : SpriteManager.SpriteType.UI),
                new UIElementData("ResourceNameText", import.UI_Name),
                new UIElementData("PriceText", "Low")
            });
        }
        foreach (Resource import in location.Unaccepted_Imports) {
            imports_scroll_view.Add(import.Type, new List<UIElementData>() {
                new UIElementData("IconImage", import.Has_Sprite ? import.Sprite_Name : "empty", import.Has_Sprite ? import.Sprite_Type : SpriteManager.SpriteType.UI),
                new UIElementData("ResourceNameText", import.UI_Name),
                new UIElementData("PriceText", "Unaccepted")
            });
        }

        float cost = Expedition.COSTS[Expedition.ExpeditionGoal.Establish_Colony][Expedition.ExpeditionLenght.Long];
        Cost_Text.text = Helper.Float_To_String(cost, 0);
        Cost_Text.color = City.Instance.Cash >= cost ? default_text_color : Color.red;
        Ok_Button.interactable = City.Instance.Cash >= cost && City.Instance.Max_Colonies > City.Instance.Colonies.Count;
    }

    public void Ok_Button_Click()
    {
        if (string.IsNullOrEmpty(Name_Input.text)) {
            Random_Name();
        }
        location.Name = Name_Input.text;
        Expedition expedition = new Expedition(harbor.Id, location);
        expedition.Launch(harbor);
        Active = false;
    }

    public void Cancel_Button_Click()
    {
        Active = false;
    }

    public void Random_Name_Click()
    {
        Random_Name();
    }

    private void Random_Name()
    {
        Name_Input.text = NameManager.Instance.Get_Name(NameManager.NameType.City, true);
    }

    private void Initialize_GUI()
    {
        Location_Dropdown.options.Clear();
        List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();
        for(int i = 0; i < City.Instance.Colony_Locations.Count; i++) {
            options.Add(new Dropdown.OptionData(string.Format("{0}. {1}{2}", (i + 1).ToString(), City.Instance.Colony_Locations[i].Cheap_Exports[0], City.Instance.Colony_Locations[i].Cheap_Exports.Count > 1 ? "..." : string.Empty)));
        }
        Location_Dropdown.options = options;
        Random_Name();
    }
}
