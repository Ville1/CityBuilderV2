using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class DistributionDepotGUI : MonoBehaviour {
    public static readonly string TARGET_ID_KEY = "distribution_target_id";

    public static DistributionDepotGUI Instance { get; private set; }

    public GameObject Panel;
    public Text Target_Text;
    public Button Apply_Button;
    public GameObject Scroll_Content;
    public GameObject Scroll_Row_Prototype;

    public GameObject Selected_Resource_Container;
    public Text Resource_Name_Text;
    public Toggle Transport_Toggle;
    public InputField Min_Amount_Input_Field;
    
    private Building depot;
    private Building target;
    private RowScrollView<Resource.ResourceType> scroll_view;
    private Dictionary<Resource, int?> settings = new Dictionary<Resource, int?>();
    private Resource selected_resource;
    private bool waiting_for_target;
    private bool ignore_input_update;
    private List<long> connected_ids;

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
        scroll_view = new RowScrollView<Resource.ResourceType>("resource_scroll_view", Scroll_Content, Scroll_Row_Prototype, 20.0f);
        waiting_for_target = false;
        ignore_input_update = false;
        connected_ids = new List<long>();
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
                Update_GUI();
            } else {
                waiting_for_target = false;
            }
        }
    }
    
    public bool Waiting_For_Target
    {
        get {
            return waiting_for_target;
        }
        set {
            waiting_for_target = value;
            if (!value) {
                Active = true;
            }
        }
    }

    public void Show(Building depot)
    {
        if (!depot.Tags.Contains(Building.Tag.Is_Distribution_Depot)) {
            CustomLogger.Instance.Error(string.Format("{0} is not a distribution depot", depot.Internal_Name));
            return;
        }
        this.depot = depot;
        bool has_storehouse = false;
        foreach(Building b in Map.Instance.Get_Buildings_Around(depot).Where(x => x.Is_Storehouse).ToList()) {
            has_storehouse = true;
            break;
        }
        if (!has_storehouse) {
            MessageManager.Instance.Show_Message("Depot needs to be adjacent to at least one storehouse to operate.");
            return;
        }
        target = null;
        if (depot.Data.ContainsKey(TARGET_ID_KEY)) {
            int id = int.Parse(depot.Data[TARGET_ID_KEY]);
            target = City.Instance.Buildings.FirstOrDefault(x => x.Id == id);
        }
        settings = new Dictionary<Resource, int?>();
        foreach (Resource resource in Resource.All) {
            if (depot.Data.ContainsKey(resource.Type.ToString().ToLower())) {
                settings.Add(resource, int.Parse(depot.Data[resource.Type.ToString().ToLower()]));
            } else {
                settings.Add(resource, null);
            }
        }
        connected_ids = depot.Get_Connected_Buildings().Select(x => x.Key.Id).ToList();
        Active = true;
        Selected_Resource_Container.SetActive(false);
    }

    private void Update_GUI()
    {
        Target_Text.text = target == null ? "None" : string.Format("{0} (#{1})", target.Name, target.Id);
        scroll_view.Clear();
        foreach(Resource resource in Resource.All.Where(x => !x.Tags.Contains(Resource.ResourceTag.Non_Tradeable)).OrderBy(x => x.UI_Name).ToList()) {
            int? setting = settings[resource];
            scroll_view.Add(resource.Type, new List<UIElementData>() {
                new UIElementData("IconImage", resource.Has_Sprite ? resource.Sprite_Name : "empty", resource.Has_Sprite ? resource.Sprite_Type : SpriteManager.SpriteType.UI),
                new UIElementData("NameText", resource.UI_Name),
                new UIElementData("SettingText", setting.HasValue ? string.Format(">{0}", setting.Value.ToString()) : "-"),
                new UIElementData("SelectRowButton", null, delegate() {
                    Select_Resource(resource);
                })
            });
        }
    }

    public void On_Select_Click()
    {
        Active = false;
        MessageManager.Instance.Show_Message("Click on a storehouse building you wisth to select. Press esc to cancel.");
        Waiting_For_Target = true;
    }

    public void Select_Target(Building target)
    {
        if (!target.Is_Storehouse) {
            MessageManager.Instance.Show_Message(string.Format("{0} is not a storehouse. Press esc to cancel.", target.Name));
            return;
        }
        if (!connected_ids.Contains(target.Id)) {
            MessageManager.Instance.Show_Message(string.Format("{0} is not connected to depot, or is too far away. Press esc to cancel.", target.Name));
            return;
        }
        this.target = target;
        Waiting_For_Target = false;
    }

    public void On_Cancel_Click()
    {
        Active = false;
    }

    public void On_Apply_Click()
    {
        if(target != null) {
            if (depot.Data.ContainsKey(TARGET_ID_KEY)) {
                depot.Data[TARGET_ID_KEY] = target.Id.ToString();
            } else {
                depot.Data.Add(TARGET_ID_KEY, target.Id.ToString());
            }
        } else if (depot.Data.ContainsKey(TARGET_ID_KEY)) {
            depot.Data.Remove(TARGET_ID_KEY);
        }
        foreach(KeyValuePair<Resource, int?> pair in settings) {
            string key = pair.Key.Type.ToString().ToLower();
            if (!pair.Value.HasValue && depot.Data.ContainsKey(key)) {
                depot.Data.Remove(key);
            } else if (pair.Value.HasValue) {
                if (depot.Data.ContainsKey(key)) {
                    depot.Data[key] = pair.Value.Value.ToString();
                } else {
                    depot.Data.Add(key, pair.Value.Value.ToString());
                }
            }
        }
        Active = false;
    }

    public void On_Min_Input_Changed()
    {
        if (ignore_input_update) {
            return;
        }
        int value = 0;
        if(!int.TryParse(Min_Amount_Input_Field.text, out value)) {
            value = 0;
        }
        Min_Amount_Input_Field.text = value.ToString();
        settings[selected_resource] = value;
        Update_GUI();
    }

    public void On_Transport_Toggle_Changed()
    {
        if (ignore_input_update) {
            return;
        }
        Min_Amount_Input_Field.text = "0";
        Min_Amount_Input_Field.interactable = Transport_Toggle.isOn;
        settings[selected_resource] = Transport_Toggle.isOn ? (int?)0 : null;
        Update_GUI();
    }

    private void Select_Resource(Resource resource)
    {
        ignore_input_update = true;
        Selected_Resource_Container.SetActive(true);
        selected_resource = resource;
        Resource_Name_Text.text = resource.UI_Name;
        Transport_Toggle.isOn = settings[resource].HasValue;
        if (settings[resource].HasValue) {
            Min_Amount_Input_Field.interactable = true;
            Min_Amount_Input_Field.text = settings[resource].Value.ToString();
        } else {
            Min_Amount_Input_Field.interactable = false;
            Min_Amount_Input_Field.text = "0";
        }
        ignore_input_update = false;
    }
}
