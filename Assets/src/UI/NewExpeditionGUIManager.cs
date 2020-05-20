using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class NewExpeditionGUIManager : MonoBehaviour {
    public static NewExpeditionGUIManager Instance { get; private set; }

    public GameObject Panel;

    public GameObject New_Container;
    public Dropdown Goal_Dropdown;
    public Dropdown Lenght_Dropdown;
    public Dropdown Resource_Dropdown;
    public Text Cost_Text;
    public Button Create_Button;

    public GameObject Existing_Container;
    public Text Goal_Text;
    public Text Lenght_Text;
    public Text Resource_Text;
    public Image Resource_Image;
    public Text Status_Text;
    public Text Time_Text;

    public GameObject Workers_Required_Container;

    private Building harbor;
    private bool initialized;
    private Expedition existing_expedition;

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
        initialized = false;
    }

    /// <summary>
    /// Per frame update
    /// </summary>
    private void Update()
    {
        if(Active && existing_expedition != null) {
            Status_Text.text = Helper.Snake_Case_To_UI(existing_expedition.State.ToString(), true);
            Time_Text.text = string.Format("({0} day{1})", Mathf.RoundToInt(existing_expedition.Time_Remaining), Helper.Plural(Mathf.RoundToInt(existing_expedition.Time_Remaining)));
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

    public Expedition Expedition
    {
        get {
            return existing_expedition;
        }
    }

    public void Show(Building harbor)
    {
        if (Active) {
            return;
        }
        if (!harbor.Tags.Contains(Building.Tag.Creates_Expeditions)) {
            CustomLogger.Instance.Error("Invalid harbor");
            return;
        }
        this.harbor = harbor;
        Active = true;
        existing_expedition = City.Instance.Expeditions.FirstOrDefault(x => x.Building_Id == harbor.Id);
        
        int total_workers = 0;
        foreach (KeyValuePair<Building.Resident, int> pair in harbor.Current_Workers) {
            total_workers += pair.Value;
        }
        if (total_workers < harbor.Max_Workers_Total && existing_expedition == null) {
            Existing_Container.SetActive(false);
            New_Container.SetActive(false);
            Workers_Required_Container.SetActive(true);
            return;
        }

        Existing_Container.SetActive(existing_expedition != null);
        New_Container.SetActive(existing_expedition == null);
        Workers_Required_Container.SetActive(false);
        if (existing_expedition != null) {
            Goal_Text.text = Helper.Snake_Case_To_UI(existing_expedition.Goal.ToString(), true);
            Lenght_Text.text = Helper.Snake_Case_To_UI(existing_expedition.Lenght.ToString(), true);
            Resource_Text.text = existing_expedition.Resource.UI_Name;
            Resource_Image.gameObject.SetActive(existing_expedition.Resource != null && existing_expedition.Resource.Has_Sprite);
            if(Resource_Image.gameObject.activeSelf) {
                Resource_Image.sprite = SpriteManager.Instance.Get(existing_expedition.Resource.Sprite_Name, existing_expedition.Resource.Sprite_Type);
            }
            Status_Text.text = Helper.Snake_Case_To_UI(existing_expedition.State.ToString(), true);
            Time_Text.text = string.Format("({0} day{1})", Mathf.RoundToInt(existing_expedition.Time_Remaining), Helper.Plural(Mathf.RoundToInt(existing_expedition.Time_Remaining)));
        } else {
            Update_New_GUI();
        }
    }

    private Expedition.ExpeditionGoal Selected_Goal
    {
        get {
            return (Expedition.ExpeditionGoal)Goal_Dropdown.value;
        }
        set {
            Goal_Dropdown.value = (int)value;
        }
    }

    private Expedition.ExpeditionLenght Selected_Lenght
    {
        get {
            return (Expedition.ExpeditionLenght)Lenght_Dropdown.value;
        }
        set {
            Lenght_Dropdown.value = (int)value;
        }
    }

    public void Update_New_GUI()
    {
        if (!initialized) {
            List<Dropdown.OptionData> goal_options = new List<Dropdown.OptionData>();
            foreach(Expedition.ExpeditionGoal goal in Enum.GetValues(typeof(Expedition.ExpeditionGoal))) {
                goal_options.Add(new Dropdown.OptionData(Helper.Snake_Case_To_UI(goal.ToString(), true)));
            }
            Goal_Dropdown.options = goal_options;
            Selected_Goal = Expedition.ExpeditionGoal.Collect_Resources;
            List<Dropdown.OptionData> lenght_options = new List<Dropdown.OptionData>();
            foreach (Expedition.ExpeditionLenght lenght in Enum.GetValues(typeof(Expedition.ExpeditionLenght))) {
                lenght_options.Add(new Dropdown.OptionData(Helper.Snake_Case_To_UI(lenght.ToString(), true)));
            }
            Lenght_Dropdown.options = lenght_options;
            Selected_Lenght = Expedition.ExpeditionLenght.Medium;
            List<Dropdown.OptionData> resource_options = new List<Dropdown.OptionData>();
            foreach (Resource resource in Resource.All.Where(x => x.Tags.Contains(Resource.ResourceTag.Basic)).OrderBy(x => x.UI_Name).ToArray()) {
                resource_options.Add(resource.Has_Sprite ? new Dropdown.OptionData(resource.UI_Name, SpriteManager.Instance.Get(resource.Sprite_Name, resource.Sprite_Type)) : new Dropdown.OptionData(resource.UI_Name));
            }
            Resource_Dropdown.options = resource_options;
            initialized = true;
        }

        Lenght_Dropdown.interactable = Selected_Goal == Expedition.ExpeditionGoal.Collect_Resources;
        Resource_Dropdown.interactable = Selected_Goal != Expedition.ExpeditionGoal.Establish_Colony;
        if (Selected_Goal == Expedition.ExpeditionGoal.Establish_Colony) {
            Selected_Lenght = Expedition.ExpeditionLenght.Long;
            Resource_Dropdown.value = 0;
        }
        float cost = Expedition.COSTS[Selected_Goal][Selected_Lenght];
        Cost_Text.text = Helper.Float_To_String(cost, 0);
        Create_Button.interactable = City.Instance.Cash >= cost;
    }

    public void Create()
    {
        Resource resource = null;
        if (Resource_Dropdown.interactable) {
            List<Resource> resources = Resource.All.Where(x => x.Tags.Contains(Resource.ResourceTag.Basic)).OrderBy(x => x.UI_Name).ToList();
            resource = resources[Resource_Dropdown.value];
        }
        Expedition expedition = new Expedition(Selected_Goal, Selected_Lenght, harbor.Id, resource);
        City.Instance.Take_Cash(Expedition.COSTS[Selected_Goal][Selected_Lenght]);
        City.Instance.Add_Expedition(expedition);
        harbor.Lock_Workers = true;
        Active = false;
    }

    public void Cancel()
    {
        Active = false;
    }
}
