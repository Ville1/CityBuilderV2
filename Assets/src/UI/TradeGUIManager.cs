using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class TradeGUIManager : MonoBehaviour {
    public static TradeGUIManager Instance { get; private set; }

    public GameObject Panel;

    public Dropdown Partner_Dropdown;
    public Text Opinion_Text;
    public Text Errors_Text;
    public Dropdown Action_Dropdown;
    public Dropdown Resource_Dropdown;
    public InputField Amount_Input_Field;
    public Text Amount_Label_Text;
    public Text Next_Trade_Text;
    public Text Cash_Delta_Text;
    public Text Resource_Name_Text;
    public Image Resource_Icon_Image;
    public Text Resource_Delta_Text;

    private Building building;
    private TradeRouteSettings settings;
    private List<Resource> option_resource_list;

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
    }

    /// <summary>
    /// Per frame update
    /// </summary>
    private void Update()
    {
        if (Active) {
            Next_Trade_Text.text = building.Trade_Route_Settings.Caravan_Cooldown == 1.0f ? "1.0 day" : string.Format("{0} days", Helper.Float_To_String(building.Trade_Route_Settings.Caravan_Cooldown, 1));
            if(settings.Partner != null) {
                Opinion_Text.text = Helper.Float_To_String(settings.Partner.Opinion * 100.0f, 0, true) + "%";
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
                Update_GUI(true, true);
            }
        }
    }

    private List<ForeignCity> Partners
    {
        get {
            List<ForeignCity> partners = new List<ForeignCity>();
            if (building.Tags.Contains(Building.Tag.Land_Trade)) {
                foreach (ForeignCity city in Contacts.Instance.Cities) {
                    if (city.Trade_Route_Type == ForeignCity.TradeRouteType.Land || city.Trade_Route_Type == ForeignCity.TradeRouteType.Both) {
                        partners.Add(city);
                    }
                }
            }
            if (building.Tags.Contains(Building.Tag.Water_Trade)) {
                foreach (ForeignCity city in Contacts.Instance.Cities) {
                    if (!partners.Contains(city) && (city.Trade_Route_Type == ForeignCity.TradeRouteType.Water || city.Trade_Route_Type == ForeignCity.TradeRouteType.Both)) {
                        partners.Add(city);
                    }
                }
            }
            partners = partners.OrderByDescending(x => x.Opinion).ToList();
            return partners;
        }
    }

    private void Update_GUI(bool generate_city_list, bool generate_resource_list)
    {
        Amount_Label_Text.text = string.Format("per {0} day{1}", TradeRouteSettings.CARAVAN_INTERVAL, Helper.Plural(TradeRouteSettings.CARAVAN_INTERVAL));
        List<ForeignCity> partners = Partners;
        if (generate_city_list) {
            List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();
            options.Add(new Dropdown.OptionData("None"));
            foreach (ForeignCity city in partners) {
                options.Add(new Dropdown.OptionData(city.Name));
            }
            Partner_Dropdown.options = options;
        }

        int index = 0;
        if (settings.Partner != null) {
            index = partners.IndexOf(settings.Partner) + 1;
        }
        Partner_Dropdown.value = index;

        if (settings.Partner != null) {
            Opinion_Text.text = Helper.Float_To_String(settings.Partner.Opinion * 100.0f, 0, true) + "%";
        } else {
            Opinion_Text.text = "-";
        }
        Action_Dropdown.interactable = settings.Partner != null && settings.Partner.Opinion >= 0.0f;
        Resource_Dropdown.interactable = settings.Partner != null && settings.Partner.Opinion >= 0.0f;
        Amount_Input_Field.interactable = settings.Partner != null && settings.Partner.Opinion >= 0.0f;

        List<string> errors = new List<string>();
        if (!City.Instance.Has_Outside_Road_Connection()) {
            errors.Add("no outside connection");
        }
        if(settings.Partner != null && settings.Partner.Opinion < 0) {
            errors.Add("opinion too low");
        }
        StringBuilder error_builder = new StringBuilder();
        foreach(string error in errors) {
            error_builder.Append(error_builder.Length == 0 ? error[0].ToString().ToUpper() + error.Substring(1) : error).Append(", ");
        }
        if(error_builder.Length == 0) {
            Errors_Text.text = string.Empty;
        } else {
            Errors_Text.text = error_builder.Remove(error_builder.Length - 2, 2).ToString();
        }
        Action_Dropdown.value = settings.Action == TradeRouteSettings.TradeAction.Buy ? 0 : 1;

        if (generate_resource_list) {
            if (settings.Action == TradeRouteSettings.TradeAction.Buy) {
                if (settings.Partner == null) {
                    Resource_Dropdown.options = new List<Dropdown.OptionData>() { new Dropdown.OptionData("none") };
                    Resource_Dropdown.value = 0;
                } else {
                    List<Dropdown.OptionData> resource_options = new List<Dropdown.OptionData>();
                    option_resource_list = new List<Resource>();
                    foreach (Resource export in settings.Partner.Cheap_Exports) {
                        string option_text = string.Format("{0} {1} (cheap)", export.UI_Name, Helper.Float_To_String(settings.Partner.Get_Export_Price(export), 2));
                        resource_options.Add(export.Has_Sprite ? new Dropdown.OptionData(option_text, SpriteManager.Instance.Get(export.Sprite_Name, export.Sprite_Type)) : new Dropdown.OptionData(option_text));
                        option_resource_list.Add(export);
                    }
                    foreach (Resource export in settings.Partner.Exports) {
                        string option_text = string.Format("{0} {1} (normal)", export.UI_Name, Helper.Float_To_String(settings.Partner.Get_Export_Price(export), 2));
                        resource_options.Add(export.Has_Sprite ? new Dropdown.OptionData(option_text, SpriteManager.Instance.Get(export.Sprite_Name, export.Sprite_Type)) : new Dropdown.OptionData(option_text));
                        option_resource_list.Add(export);
                    }
                    foreach (Resource export in settings.Partner.Expensive_Exports) {
                        string option_text = string.Format("{0} {1} (expensive)", export.UI_Name, Helper.Float_To_String(settings.Partner.Get_Export_Price(export), 2));
                        resource_options.Add(export.Has_Sprite ? new Dropdown.OptionData(option_text, SpriteManager.Instance.Get(export.Sprite_Name, export.Sprite_Type)) : new Dropdown.OptionData(option_text));
                        option_resource_list.Add(export);
                    }
                    Resource_Dropdown.options = resource_options;
                    if (settings.Resource != null) {
                        if (option_resource_list.Contains(settings.Resource)) {
                            Resource_Dropdown.value = option_resource_list.IndexOf(settings.Resource);
                        } else {
                            settings.Resource = option_resource_list.Count != 0 ? option_resource_list[0] : null;
                        }
                    } else if(option_resource_list.Count != 0) {
                        settings.Resource = option_resource_list[0];
                    }
                }
            } else {
                if (settings.Partner == null) {
                    Resource_Dropdown.options = new List<Dropdown.OptionData>() { new Dropdown.OptionData("none") };
                    Resource_Dropdown.value = 0;
                } else {
                    List<Dropdown.OptionData> resource_options = new List<Dropdown.OptionData>();
                    option_resource_list = new List<Resource>();
                    foreach (Resource import in settings.Partner.Preferred_Imports) {
                        string option_text = string.Format("{0} {1} (high)", import.UI_Name, Helper.Float_To_String(settings.Partner.Get_Import_Price(import), 2));
                        resource_options.Add(import.Has_Sprite ? new Dropdown.OptionData(option_text, SpriteManager.Instance.Get(import.Sprite_Name, import.Sprite_Type)) : new Dropdown.OptionData(option_text));
                        option_resource_list.Add(import);
                    }
                    foreach (Resource resource in Resource.All) {
                        if (!(settings.Partner.Preferred_Imports.Contains(resource) || settings.Partner.Disliked_Imports.Contains(resource) || settings.Partner.Unaccepted_Imports.Contains(resource))) {
                            string option_text = string.Format("{0} {1} (normal)", resource.UI_Name, Helper.Float_To_String(settings.Partner.Get_Import_Price(resource), 2));
                            resource_options.Add(resource.Has_Sprite ? new Dropdown.OptionData(option_text, SpriteManager.Instance.Get(resource.Sprite_Name, resource.Sprite_Type)) : new Dropdown.OptionData(option_text));
                            option_resource_list.Add(resource);
                        }
                    }
                    foreach (Resource import in settings.Partner.Disliked_Imports) {
                        string option_text = string.Format("{0} {1} (low)", import.UI_Name, Helper.Float_To_String(settings.Partner.Get_Import_Price(import), 2));
                        resource_options.Add(import.Has_Sprite ? new Dropdown.OptionData(option_text, SpriteManager.Instance.Get(import.Sprite_Name, import.Sprite_Type)) : new Dropdown.OptionData(option_text));
                        option_resource_list.Add(import);
                    }
                    Resource_Dropdown.options = resource_options;
                    if (settings.Resource != null) {
                        if (option_resource_list.Contains(settings.Resource)) {
                            Resource_Dropdown.value = option_resource_list.IndexOf(settings.Resource);
                        } else {
                            settings.Resource = option_resource_list.Count != 0 ? option_resource_list[0] : null;
                        }
                    } else if (option_resource_list.Count != 0) {
                        settings.Resource = option_resource_list[0];
                    }
                }
            }
        }
        Amount_Input_Field.text = settings.Partner != null && settings.Resource != null ? settings.Amount.ToString() : "0";

        if(settings.Partner != null && settings.Resource != null && settings.Amount != 0.0f) {
            Cash_Delta_Text.text = Helper.Float_To_String(settings.Cash_Delta, 2, true);
            Resource_Name_Text.text = settings.Resource.UI_Name;
            Resource_Icon_Image.gameObject.SetActive(settings.Resource.Has_Sprite);
            if (settings.Resource.Has_Sprite) {
                Resource_Icon_Image.sprite = SpriteManager.Instance.Get(settings.Resource.Sprite_Name, settings.Resource.Sprite_Type);
            }
            Resource_Delta_Text.text = Helper.Float_To_String(settings.Resource_Delta, 2, true);
        } else {
            Cash_Delta_Text.text = "0.00";
            Resource_Name_Text.text = "Resource";
            Resource_Icon_Image.gameObject.SetActive(false);
            Resource_Delta_Text.text = "0.00";
        }
    }

    public void Show(Building building)
    {
        if(!building.Tags.Contains(Building.Tag.Land_Trade) && !building.Tags.Contains(Building.Tag.Water_Trade)) {
            return;
        }
        this.building = building;
        settings = building.Trade_Route_Settings.Clone();
        Active = true;
    }

    public void On_Partner_Change()
    {
        if(Partner_Dropdown.value == 0) {
            settings.Partner = null;
        } else {
            settings.Partner = Partners[Partner_Dropdown.value - 1];
        }
        Update_GUI(false, true);
    }

    public void On_Action_Change()
    {
        settings.Action = Action_Dropdown.value == 0 ? TradeRouteSettings.TradeAction.Buy : TradeRouteSettings.TradeAction.Sell;
        Update_GUI(false, true);
    }

    public void On_Resource_Change()
    {
        settings.Resource = option_resource_list[Resource_Dropdown.value];
        Update_GUI(false, false);
    }

    public void On_Amount_Changed()
    {
        int amount = 0;
        if(!int.TryParse(Amount_Input_Field.text, out amount)) {
            Amount_Input_Field.text = Building.INPUT_OUTPUT_STORAGE_LIMIT.ToString();
            amount = Building.INPUT_OUTPUT_STORAGE_LIMIT;
        }
        if(amount < 1 || amount > Building.INPUT_OUTPUT_STORAGE_LIMIT) {
            Amount_Input_Field.text = Building.INPUT_OUTPUT_STORAGE_LIMIT.ToString();
            amount = Building.INPUT_OUTPUT_STORAGE_LIMIT;
        }
        settings.Amount = amount;
        Update_GUI(false, false);
    }
    
    public void On_Ok_Click()
    {
        settings.Action = Action_Dropdown.value == 0 ? TradeRouteSettings.TradeAction.Buy : TradeRouteSettings.TradeAction.Sell;
        building.Trade_Route_Settings.Apply(settings);
        building.Trade_Route_Settings.Validate();
        Active = false;
    }

    public void On_Cancel_Click()
    {
        Active = false;
    }
}
