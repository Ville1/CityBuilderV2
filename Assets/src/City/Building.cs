﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Building {
    protected static long current_id = 0;

    public delegate void OnUpdateDelegate(Building building, float delta_time);
    public delegate void OnBuiltDelegate(Building building);
    public delegate void OnDeconstructDelegate(Building building);
    public delegate List<Tile> OnHighlightDelegate(Building building);
    public delegate bool OnBuildCheckDelegate(Building building, Tile tile, out string message);

    public static readonly float UPDATE_INTERVAL = 1.0f;
    public static readonly float ALERT_CHANGE_INTERVAL = 2.0f;
    public static readonly string TOWN_HALL_INTERNAL_NAME = "town_hall";
    public static int INPUT_OUTPUT_STORAGE_LIMIT = 100;
    public static float DECONSTRUCTION_SPEED = 10.0f;
    public static float REFUND = 0.50f;
    public static float TOOL_REFUND = 0.10f;
    public static float DISREPAIR_SPEED = 1.0f;//HP / day
    public static float PAUSE_UPKEEP_MULTIPLIER = 0.5f;

    public enum UI_Category { Admin, Infrastructure, Housing, Services, Forestry, Agriculture, Textile, Industry, Unbuildable }
    public enum Resident { Peasant, Citizen, Noble }
    public enum BuildingSize { s1x1, s2x2, s3x3 }
    public enum Tag { Undeletable, Does_Not_Block_Wind, Bridge, Land_Trade, Water_Trade, Unique }

    public long Id { get; protected set; }
    public string Name { get; private set; }
    public string Internal_Name { get; private set; }
    public UI_Category Category { get; private set; }
    public SpriteData Sprite { get { return Sprites[Selected_Sprite]; } }
    public List<SpriteData> Sprites { get; private set; }
    public bool Is_Prototype { get { return Id < 0; } }
    public bool Is_Town_Hall { get { return Internal_Name == TOWN_HALL_INTERNAL_NAME; } }
    public BuildingSize Size { get; private set; }
    public Tile Tile { get; private set; }
    public List<Tile> Tiles { get; private set; }
    public bool Is_Preview { get; private set; }
    public Dictionary<Resource, int> Cost { get; private set; }
    public int Cash_Cost { get; private set; }
    public Dictionary<Resource, float> Storage { get; private set; }
    public float Transfer_Speed { get; private set; }//Resources / day
    public StorageSettings Storage_Settings { get; private set; }
    public Dictionary<Resource, float> Input_Storage { get; private set; }
    public List<Resource> Consumes { get; private set; }
    public Dictionary<Resource, float> Output_Storage { get; private set; }
    public List<Resource> Produces { get; private set; }
    public int Storage_Limit { get { return Is_Deconstructing ? int.MaxValue : storage_limit; } set { storage_limit = value; } }
    public List<Resource> Allowed_Resources { get; private set; }
    public bool Is_Storehouse { get { return storage_limit > 0 && Allowed_Resources != null && Allowed_Resources.Count != 0 && Transfer_Speed > 0.0f; } }
    public int Construction_Time { get; private set; }
    public Dictionary<Resource, float> Upkeep { get; private set; }
    public float Cash_Upkeep { get; private set; }
    public float Construction_Progress { get; private set; }
    public float Deconstruction_Progress { get; private set; }
    public bool Is_Built { get { return Is_Town_Hall || Construction_Progress == Construction_Time; } }
    public bool Is_Operational { get { return Is_Built && !Is_Paused && !Is_Deconstructing && (!Requires_Connection || Is_Connected); } }
    public float Construction_Speed { get; private set; }
    public float Construction_Range { get; private set; }
    public Dictionary<Resident, int> Max_Workers { get; private set; }
    public int Max_Workers_Total { get; private set; }
    public Dictionary<Resident, int> Worker_Settings { get; private set; }
    public Dictionary<Resident, int> Current_Workers { get; private set; }
    public bool Can_Be_Paused { get { return can_be_paused && !Is_Deconstructing && Is_Built; } set { can_be_paused = value; } }
    public bool Can_Be_Deleted { get { return !Is_Town_Hall && !Is_Deconstructing && Is_Built && !Tags.Contains(Tag.Undeletable); } }
    public int Max_HP { get; private set; }
    public float HP { get; private set; }
    public bool Is_Paused { get; set; }
    public bool Is_Deconstructing { get; private set; }
    public bool Is_Deleted { get; private set; }
    public bool Is_Road { get; private set; }
    public bool Is_Connected { get; set; }
    public bool Is_Complete { get { return Is_Built && !Is_Deconstructing; } }
    public bool Requires_Connection { get { return requires_connection && Is_Built && !Is_Deconstructing; } set { requires_connection = value; } }
    public bool Requires_Workers { get { return Max_Workers_Total != 0; } }
    public float Range { get; private set; }
    public int Road_Range { get; private set; }
    public Dictionary<string, object> Data { get; private set; }
    public OnBuiltDelegate On_Built { get; private set; }
    public OnUpdateDelegate On_Update { get; private set; }
    public OnDeconstructDelegate On_Deconstruct { get; private set; }
    public OnHighlightDelegate On_Highlight { get; private set; }
    public OnBuildCheckDelegate On_Build_Check { get; set; }
    public List<string> Permitted_Terrain { get; private set; }
    public List<Tag> Tags { get; private set; }
    public Dictionary<Resource, float> Per_Day_Resource_Delta { get; private set; }
    public float Per_Day_Cash_Delta { get; set; }
    public List<SpecialSetting> Special_Settings { get; private set; }
    public float Food_Production_Per_Day { get; private set; }
    public bool Losing_HP_From_No_Upkeep { get; private set; }
    public float Appeal { get; private set; }
    public float Appeal_Range { get; private set; }
    public List<Entity> Entities_Spawned { get; private set; }
    public TradeRouteSettings Trade_Route_Settings { get; set; }

    public GameObject GameObject { get; private set; }
    public SpriteRenderer Renderer { get { return GameObject != null ? GameObject.GetComponent<SpriteRenderer>() : null; } }

    protected float update_cooldown;
    protected bool update_on_last_call;
    private bool can_be_paused;
    private int storage_limit;
    private List<string> active_alerts;
    private List<GameObject> alerts;
    private GameObject active_alert;
    private bool requires_connection;
    private float alert_change_cooldown;
    private int animation_index;
    private float animation_cooldown;
    private bool animation_initialized;
    private int selected_sprite;

    public Building(Building prototype, Tile tile, List<Tile> tiles, bool is_preview)
    {
        if (!is_preview) {
            Id = current_id;
            current_id++;
        }

        Name = prototype.Name;
        Internal_Name = prototype.Internal_Name;
        Category = prototype.Category;
        Sprites = new List<SpriteData>();
        foreach(SpriteData data in prototype.Sprites) {
            Sprites.Add(data.Clone());
        }
        Selected_Sprite = prototype.Selected_Sprite;
        Size = prototype.Size;
        HP = prototype.HP;
        Max_HP = prototype.Max_HP;
        if (!is_preview) {
            Tile = tile;
            Tiles = tiles;
            foreach (Tile t in tiles) {
                t.Building = this;
                foreach(Entity entity in t.Entities) {
                    Map.Instance.Delete_Entity(entity);
                }
            }
        }
        Is_Preview = is_preview;
        Cost = Helper.Clone_Dictionary(prototype.Cost);
        Cash_Cost = prototype.Cash_Cost;
        Allowed_Resources = Helper.Clone_List(prototype.Allowed_Resources);
        Storage = new Dictionary<Resource, float>();
        foreach(Resource resource in Allowed_Resources) {
            Storage.Add(resource, 0.0f);
        }
        Storage_Limit = prototype.Storage_Limit;
        Transfer_Speed = prototype.Transfer_Speed;
        Input_Storage = new Dictionary<Resource, float>();
        Output_Storage = new Dictionary<Resource, float>();
        Construction_Time = prototype.Construction_Time;
        Upkeep = Helper.Clone_Dictionary(prototype.Upkeep);
        Cash_Upkeep = prototype.Cash_Upkeep;
        Construction_Progress = 0.0f;
        Construction_Speed = prototype.Construction_Speed;
        Construction_Range = prototype.Construction_Range;
        Max_Workers = Make_Resident_Dictionary(prototype.Max_Workers);
        Max_Workers_Total = prototype.Max_Workers_Total;
        Worker_Settings = Make_Resident_Dictionary();
        int settings_set = 0;
        if (Max_Workers.Count != 0 && Max_Workers_Total != 0) {
            foreach (KeyValuePair<Resident, int> pair in Max_Workers) {
                for (int i = 0; i < pair.Value; i++) {
                    Worker_Settings[pair.Key]++;
                    settings_set++;
                    if(settings_set == Max_Workers_Total) {
                        break;
                    }
                }
                if(settings_set == Max_Workers_Total) {
                    break;
                }
            }
        }
        Current_Workers = Make_Resident_Dictionary();
        Can_Be_Paused = prototype.can_be_paused;
        Is_Paused = false;
        Is_Road = prototype.Is_Road;
        Requires_Connection = prototype.requires_connection;
        Is_Connected = !Is_Preview && Is_Town_Hall;
        Range = prototype.Range;
        Road_Range = prototype.Road_Range;
        Data = new Dictionary<string, object>();
        On_Built = prototype.On_Built;
        On_Update = prototype.On_Update;
        On_Deconstruct = prototype.On_Deconstruct;
        On_Highlight = prototype.On_Highlight;
        On_Build_Check = prototype.On_Build_Check;
        Permitted_Terrain = Helper.Clone_List(prototype.Permitted_Terrain);
        Tags = Helper.Clone_List(prototype.Tags);
        Per_Day_Resource_Delta = new Dictionary<Resource, float>();
        if (Is_Storehouse) {
            Storage_Settings = new StorageSettings(this);
        }
        Consumes = Helper.Clone_List(prototype.Consumes);
        foreach(Resource resource in Consumes) {
            Input_Storage.Add(resource, 0.0f);
        }
        Produces = Helper.Clone_List(prototype.Produces);
        foreach (Resource resource in Produces) {
            Output_Storage.Add(resource, 0.0f);
        }
        Special_Settings = new List<SpecialSetting>();
        foreach(SpecialSetting setting in prototype.Special_Settings) {
            Special_Settings.Add(new SpecialSetting(setting));
        }
        Appeal = prototype.Appeal;
        Appeal_Range = prototype.Appeal_Range;
        Entities_Spawned = new List<Entity>();
        if(Tags.Contains(Tag.Land_Trade) || Tags.Contains(Tag.Water_Trade)) {
            Trade_Route_Settings = new TradeRouteSettings(this);
        }

        animation_index = 0;
        animation_cooldown = Sprite.Animation_Frame_Time;
        animation_initialized = false;
        update_cooldown = RNG.Instance.Next_F() * UPDATE_INTERVAL;

        GameObject = GameObject.Instantiate(
            Get_Prefab(),
            new Vector3(
                tile.GameObject.transform.position.x,
                tile.GameObject.transform.position.y,
                tile.GameObject.transform.position.z
            ),
            Quaternion.identity,
            Map.Instance.Building_Container.transform
        );
        GameObject.name = !is_preview ? string.Format("{0}_#{1}", Internal_Name, Id) : string.Format("{0}_preview", Internal_Name);
        GameObject.GetComponentInChildren<TextMesh>().gameObject.SetActive(false);
        alerts = new List<GameObject>();
        active_alerts = new List<string>();
        alert_change_cooldown = ALERT_CHANGE_INTERVAL;

        if (Is_Built && !Is_Preview) {
            Update_Appeal();
            if(On_Built != null) {
                On_Built(this);
            }
        }
        Update_Sprite();
    }

    public Building(string name, string internal_name, UI_Category category, string sprite, BuildingSize size, int hp, Dictionary<Resource, int> cost, int cash_cost, List<Resource> allowed_resources, int storage_limit, float transfer_speed,
        int construction_time, Dictionary<Resource, float> upkeep, float cash_upkeep, float construction_speed, float construction_range, Dictionary<Resident, int> workers, int max_workers, bool can_be_paused, bool is_road,
        bool p_requires_connection, float range, int road_range, OnBuiltDelegate on_built, OnUpdateDelegate on_update, OnDeconstructDelegate on_deconstruct, OnHighlightDelegate on_highlight, List<Resource> consumes, List<Resource> produces,
        float appeal, float appeal_range)
    {
        Id = -1;
        Name = name;
        Internal_Name = internal_name;
        Category = category;
        Sprites = new List<SpriteData>();
        Sprites.Add(new SpriteData(sprite));
        Selected_Sprite = 0;
        Size = size;
        Max_HP = hp;
        HP = Max_HP;
        Is_Preview = false;
        Cost = Helper.Clone_Dictionary(cost);
        Cash_Cost = cash_cost;
        Allowed_Resources = Helper.Clone_List(allowed_resources);
        Storage = new Dictionary<Resource, float>();
        Storage_Limit = storage_limit;
        Transfer_Speed = transfer_speed;
        Input_Storage = new Dictionary<Resource, float>();
        Output_Storage = new Dictionary<Resource, float>();
        Construction_Time = construction_time;
        Upkeep = Helper.Clone_Dictionary(upkeep);
        Cash_Upkeep = cash_upkeep;
        Construction_Progress = 0.0f;
        Construction_Speed = construction_speed;
        Construction_Range = construction_range;
        Max_Workers = Make_Resident_Dictionary(workers);
        Max_Workers_Total = max_workers;
        Worker_Settings = new Dictionary<Resident, int>();
        Current_Workers = new Dictionary<Resident, int>();
        Can_Be_Paused = can_be_paused;
        Is_Paused = false;
        Is_Road = is_road;
        Requires_Connection = p_requires_connection;
        Range = range;
        Road_Range = road_range;
        On_Built = on_built;
        On_Update = on_update;
        On_Deconstruct = on_deconstruct;
        On_Highlight = on_highlight;
        Consumes = Helper.Clone_List(consumes);
        Produces = Helper.Clone_List(produces);
        Permitted_Terrain = new List<string>();
        Tags = new List<Tag>();
        Special_Settings = new List<SpecialSetting>();
        Appeal = appeal;
        Appeal_Range = appeal_range;
    }

    public Building(BuildingSaveData data) : this(BuildingPrototypes.Instance.Get(data.Internal_Name), Map.Instance.Get_Tile_At(data.X, data.Y),
        Map.Instance.Get_Tiles(data.X, data.Y, BuildingPrototypes.Instance.Get(data.Internal_Name).Width, BuildingPrototypes.Instance.Get(data.Internal_Name).Height), false)
    {
        Id = data.Id;
        if(Id >= current_id) {
            current_id = Id + 1;
        }
        foreach(ResourceSaveData resource_data in data.Storage) {
            Resource r = Resource.Get((Resource.ResourceType)resource_data.Resource);
            if (!Storage.ContainsKey(r)) {
                Storage.Add(r, resource_data.Amount);
            } else {
                Storage[r] = resource_data.Amount;
            }
        }
        foreach (ResourceSaveData resource_data in data.Input_Storage) {
            Resource r = Resource.Get((Resource.ResourceType)resource_data.Resource);
            if (!Input_Storage.ContainsKey(r)) {
                Input_Storage.Add(r, resource_data.Amount);
            } else {
                Input_Storage[r] = resource_data.Amount;
            }
        }
        foreach (ResourceSaveData resource_data in data.Output_Storage) {
            Resource r = Resource.Get((Resource.ResourceType)resource_data.Resource);
            if (!Output_Storage.ContainsKey(r)) {
                Output_Storage.Add(r, resource_data.Amount);
            } else {
                Output_Storage[r] = resource_data.Amount;
            }
        }
        foreach (ResourceSaveData resource_data in data.Storage) {
            Resource r = Resource.Get((Resource.ResourceType)resource_data.Resource);
            if (!Storage.ContainsKey(r)) {
                Storage.Add(r, resource_data.Amount);
            } else {
                Storage[r] = resource_data.Amount;
            }
        }
        foreach (ResidentSaveData worker_data in data.Worker_Allocation) {
            Worker_Settings[(Resident)worker_data.Resident] = worker_data.Count;
        }
        Is_Deconstructing = data.Is_Deconstructing;
        Is_Connected = data.Is_Connected;
        Is_Paused = data.Is_Paused;
        Construction_Progress = data.Construction_Progress;
        Deconstruction_Progress = data.Deconstruction_Progress;
        HP = data.HP;
        foreach(SpecialSettingSaveData saved_setting in data.Settings) {
            SpecialSetting setting = Special_Settings.FirstOrDefault(x => x.Name == saved_setting.Name);
            if(setting == null) {
                CustomLogger.Instance.Warning(string.Format("Save data contains setting {0} that building type does not have", saved_setting.Name));
            } else {
                setting.Slider_Value = saved_setting.Slider_Value;
                setting.Toggle_Value = saved_setting.Toggle_Value;
                setting.Dropdown_Selection = saved_setting.Dropdown_Selection;
            }
        }
        if(Is_Storehouse && data.Storage_Settings != null) {
            Storage_Settings = new StorageSettings(this);
            foreach (StorageSettingSaveData saved_setting in data.Storage_Settings) {
                Storage_Settings.Get(Resource.Get((Resource.ResourceType)saved_setting.Resource)).Limit = saved_setting.Limit;
                Storage_Settings.Get(Resource.Get((Resource.ResourceType)saved_setting.Resource)).Priority = (StorageSetting.StoragePriority)saved_setting.Priority;
            }
        }
        Selected_Sprite = data.Selected_Sprite;
        if(Tags.Contains(Tag.Land_Trade) || Tags.Contains(Tag.Water_Trade)) {
            if (data.Trade_Route_Settings == null) {
                CustomLogger.Instance.Error("No TradeRouteSettings save data found");
                Trade_Route_Settings = new TradeRouteSettings(this);
            } else {
                Trade_Route_Settings = new TradeRouteSettings(this, data.Trade_Route_Settings);
            }
        }
        Update_Sprite();
    }

    public void Move(Tile tile)
    {
        if (!Is_Preview) {
            CustomLogger.Instance.Warning("Only preview buildings can be moved");
            return;
        }
        GameObject.transform.position = new Vector3(
            tile.GameObject.transform.position.x,
            tile.GameObject.transform.position.y,
            tile.GameObject.transform.position.z
        );
        Update_Sprite();
    }

    public void Instant_Build()
    {
        if (Is_Built) {
            return;
        }
        Construction_Progress = Construction_Time;
        //TODO: Duplicate code
        Update_Connectivity();
        Update_Appeal();
        if (On_Built != null) {
            On_Built(this);
        }
        if (Is_Road) {
            foreach (Building b in Map.Instance.Get_Buildings_Around(this)) {
                if (b.Is_Connected && b.Is_Built && !b.Is_Deconstructing) {
                    b.Update_Connectivity();
                }
            }
        }
        Update_Sprite();
    }


    public int Selected_Sprite {
        get {
            return selected_sprite;
        }
        set {
            selected_sprite = value;
            if (selected_sprite < 0) {
                selected_sprite = 0;
            } else if(selected_sprite >= Sprites.Count) {
                selected_sprite = Sprites.Count - 1;
            }
            if (GameObject != null) {
                Update_Sprite();
            }
        }
    }

    public void Switch_Selected_Sprite()
    {
        if(Sprites.Count == 1) {
            return;
        }
        if(selected_sprite == Sprites.Count - 1) {
            Selected_Sprite = 0;
        } else {
            Selected_Sprite++;
        }
    }

    public void Switch_Selected_Sprite(string sprite)
    {
        SpriteData data = Sprites.FirstOrDefault(x => x.Name == sprite);
        if(data != null) {
            Selected_Sprite = Sprites.IndexOf(data);
        }
    }

    public void Switch_Selected_Sprite(int index)
    {
        if(index >= 0 && index < Sprites.Count) {
            Selected_Sprite = index;
        }
    }

    public float Current_Storage_Amount
    {
        get {
            float amount = 0;
            foreach(KeyValuePair<Resource, float> pair in Storage) {
                amount += pair.Value;
            }
            return amount;
        }
    }

    public float Store_Resources(Resource resource, float amount)
    {
        float max = Is_Deconstructing ? float.MaxValue : (Mathf.Min(Storage_Limit, Storage_Settings != null && Storage_Settings.Has(resource) ? Storage_Settings.Get(resource).Limit : float.MaxValue));
        float current = Storage.ContainsKey(resource) ? Storage[resource] : 0.0f;
        float space = max - current;
        float stored = Mathf.Min(amount, space);
        if (Storage.ContainsKey(resource)) {
            Storage[resource] += stored;
        } else {
            Storage.Add(resource, stored);
        }
        return stored;
    }

    public float Take_Resources(Resource resource, float amount)
    {
        if (!Storage.ContainsKey(resource)) {
            return 0;
        }
        if(Storage[resource] < amount) {
            float f = Storage[resource];
            Storage[resource] = 0;
            return f;
        }
        Storage[resource] = Storage[resource] - amount;
        return amount;
    }

    public Dictionary<Resource, float> Total_Max_Storage
    {
        get {
            Dictionary<Resource, float> max = new Dictionary<Resource, float>();
            foreach(Resource resource in Allowed_Resources) {
                float current = Storage.ContainsKey(resource) ? Storage[resource] : 0.0f;
                float specific_limit = (Storage_Settings.Has(resource) ? Storage_Settings.Get(resource).Limit : Storage_Limit) - current;
                float general_limit = Storage_Limit - Current_Storage_Amount;
                max.Add(resource, Math.Min(specific_limit, general_limit) + current);
            }
            foreach(Resource resource in Consumes) {
                float current = Input_Storage.ContainsKey(resource) ? Input_Storage[resource] : 0.0f;
                if (max.ContainsKey(resource)) {
                    max[resource] += (INPUT_OUTPUT_STORAGE_LIMIT - current);
                } else {
                    max.Add(resource, (INPUT_OUTPUT_STORAGE_LIMIT - current));
                }
            }
            foreach (Resource resource in Produces) {
                float current = Output_Storage.ContainsKey(resource) ? Output_Storage[resource] : 0.0f;
                if (max.ContainsKey(resource)) {
                    max[resource] += (INPUT_OUTPUT_STORAGE_LIMIT - current);
                } else {
                    max.Add(resource, (INPUT_OUTPUT_STORAGE_LIMIT - current));
                }
            }
            return max;
        }
    }

    public Dictionary<Resource, float> All_Resources
    {
        get {
            Dictionary<Resource, float> all = new Dictionary<Resource, float>();
            foreach(KeyValuePair<Resource, float> pair in Storage) {
                if (!all.ContainsKey(pair.Key)) {
                    all.Add(pair.Key, pair.Value);
                } else {
                    all[pair.Key] += pair.Value;
                }
            }
            foreach (KeyValuePair<Resource, float> pair in Output_Storage) {
                if (!all.ContainsKey(pair.Key)) {
                    all.Add(pair.Key, pair.Value);
                } else {
                    all[pair.Key] += pair.Value;
                }
            }
            foreach (KeyValuePair<Resource, float> pair in Input_Storage) {
                if (!all.ContainsKey(pair.Key)) {
                    all.Add(pair.Key, pair.Value);
                } else {
                    all[pair.Key] += pair.Value;
                }
            }
            return all;
        }
    }

    public void Update(float delta_time)
    {
        update_cooldown -= delta_time;
        if(update_cooldown > 0.0f) {
            update_on_last_call = false;
            return;
        }
        update_cooldown += UPDATE_INTERVAL;
        if (TimeManager.Instance.Paused || !Is_Built) {
            update_on_last_call = false;
            return;
        }
        float realtime_delta_time = update_cooldown;
        delta_time = update_cooldown * TimeManager.Instance.Multiplier;
        update_on_last_call = true;
        float delta_days = TimeManager.Instance.Seconds_To_Days(delta_time, 1.0f);

        if(Sprite.Animation_Sprites.Count != 0) {
            animation_cooldown -= realtime_delta_time;
            Update_Sprite(true);
        }

        if(!Can_Be_Paused && Is_Paused) {
            CustomLogger.Instance.Warning(string.Format("{0} can't be paused", Internal_Name));
            Is_Paused = false;
        }

        if (Is_Paused) {
            Show_Alert("alert_pause");
        }
        if(Max_Workers_Total != 0 && !Is_Paused && Is_Built) {
            int workers_missing = 0;
            foreach(KeyValuePair<Resident, int> pair in Worker_Settings) {
                workers_missing += (pair.Value - Current_Workers[pair.Key]);
            }
            if(workers_missing > Workers_Allocated / 2) {
                Show_Alert("alert_workers");
            }
        }

        Per_Day_Resource_Delta.Clear();
        Per_Day_Cash_Delta = 0.0f;
        Food_Production_Per_Day = 0.0f;
        Losing_HP_From_No_Upkeep = false;

        if (Is_Deconstructing) {
            //Move resources
            if (Current_Storage_Amount > 0.0f) {
                Dictionary<Resource, float> added = new Dictionary<Resource, float>();
                foreach (KeyValuePair<Resource, float> pair in Storage) {
                    added.Add(pair.Key, City.Instance.Add_To_Storage(pair.Key, pair.Value));
                }
                foreach (KeyValuePair<Resource, float> pair in added) {
                    Storage[pair.Key] -= pair.Value;
                    if (Storage[pair.Key] < 0.0f) {
                        CustomLogger.Instance.Error("Negative resource: " + pair.Key.ToString());
                        Storage[pair.Key] = 0.0f;
                    }
                }
            }
            //Update progress
            Deconstruction_Progress += delta_time * DECONSTRUCTION_SPEED;
            Update_Sprite();
            if(Deconstruction_Progress >= Construction_Time) {
                Delete();
                Update_Appeal();
                City.Instance.Remove_Building(this);
                foreach (Building building in Map.Instance.Get_Buildings_Around(this)) {
                    building.Update_Sprite();
                }
            }
            return;
        }
        
        if(Requires_Connection && !Is_Connected) {
            Show_Alert("alert_road");
        }

        if(Cash_Upkeep > 0.0f) {
            float amount = Cash_Upkeep;
            if (Is_Paused) {
                amount *= PAUSE_UPKEEP_MULTIPLIER;
            } else {
                int total_workers = Current_Workers[Resident.Peasant] + Current_Workers[Resident.Citizen] + Current_Workers[Resident.Noble];
                if(total_workers > Max_Workers_Total) {
                    CustomLogger.Instance.Error(string.Format("Too many workers: {0} / {1}", total_workers, Max_Workers_Total));
                } else if(total_workers < Max_Workers_Total) {
                    float multiplier = (0.5f + (0.5f * (1.0f - ((Max_Workers_Total - total_workers) / (float)Max_Workers_Total))));
                    amount *= multiplier;
                }
            }
            Per_Day_Cash_Delta -= amount;
            City.Instance.Take_Cash(Calculate_Actual_Amount(amount, delta_time));
        }

        //TODO: Have buildings (storehouses?) transfer upkeep to buildings?
        foreach (KeyValuePair<Resource, float> pair in Upkeep) {
            float amount = pair.Value * (Is_Paused ? PAUSE_UPKEEP_MULTIPLIER : 1.0f);
            float actual_amount = Calculate_Actual_Amount(amount, delta_time);
            float amount_taken = City.Instance.Take_From_Storage(pair.Key, actual_amount);
            Update_Delta(pair.Key, -amount);
            if(amount_taken < actual_amount) {
                //TODO: New icon?
                Show_Alert("alert_no_resources");
                HP -= DISREPAIR_SPEED * delta_days;
                Losing_HP_From_No_Upkeep = true;
                if(HP < 1.0f) {
                    HP = 1.0f;
                }
            }
        }

        if(Is_Operational && Construction_Speed > 0.0f && Construction_Range > 0.0f) {
            float construction_progress = Efficency * Construction_Speed * delta_time;
            foreach(Building building in City.Instance.Buildings) {
                if (!building.Is_Built) {
                    float range_multiplier = 1.0f;
                    if (Tile.Coordinates.Distance(building.Tile.Coordinates) > Construction_Range) {
                        range_multiplier = 10.0f / (10.0f + (Tile.Coordinates.Distance(building.Tile.Coordinates) - Construction_Range));
                    }
                    building.Construction_Progress = Mathf.Clamp(building.Construction_Progress + (range_multiplier * construction_progress), 0.0f, building.Construction_Time);
                    building.Update_Sprite();
                    if (building.Is_Built) {
                        building.Update_Appeal();
                        building.Update_Connectivity();
                        if (building.On_Built != null) {
                            building.On_Built(building);
                        }
                        if (building.Is_Road) {
                            foreach (Building b in Map.Instance.Get_Buildings_Around(building)) {
                                if (b.Is_Connected && b.Is_Built && !b.Is_Deconstructing) {
                                    b.Update_Connectivity();
                                }
                            }
                        } else {
                            NotificationManager.Instance.Add_Notification(new Notification(string.Format("Building completed: {0}", building.Name), building.Sprite.Name, building.Sprite.Type, delegate() {
                                CameraManager.Instance.Set_Camera_Location(building.Tile.Coordinates.Vector);
                            }));
                        }
                    }
                } else if(building.Is_Built && building.HP < building.Max_HP && !building.Losing_HP_From_No_Upkeep) {
                    building.HP = Mathf.Min(building.Max_HP, building.HP + construction_progress);
                }
            }
        }

        //Collecting resources
        if(Is_Operational && (Is_Storehouse || (Transfer_Speed > 0.0f && Road_Range > 0 && Consumes.Count != 0))) {
            List<Building> connected_buildings = Get_Connected_Buildings(Road_Range).Select(x => x.Key).ToList();
            float resources_transfered = 0.0f;
            float max_transfer = Transfer_Speed * delta_days * Efficency;
            if (Is_Storehouse) {
                foreach(Building building in connected_buildings) {
                    if(building.Id == Id) {
                        continue;
                    }
                    foreach(Resource resource in Allowed_Resources) {
                        float take = Math.Min(max_transfer - resources_transfered, Storage_Settings.Get(resource).Limit - Storage[resource]);
                        if (building.Produces.Contains(resource) && building.Output_Storage.ContainsKey(resource)) {
                            float resources_taken = Mathf.Min(building.Output_Storage[resource], take);
                            building.Output_Storage[resource] -= resources_taken;
                            resources_transfered += resources_taken;
                            Storage[resource] += resources_taken;
                        } else if(!building.Consumes.Contains(resource) && building.Input_Storage.ContainsKey(resource) && building.Input_Storage[resource] > 0.0f) {
                            float resources_taken = Mathf.Min(building.Input_Storage[resource], take);
                            building.Input_Storage[resource] -= resources_taken;
                            resources_transfered += resources_taken;
                            Storage[resource] += resources_taken;
                        } else if (building.Is_Storehouse && building.Allowed_Resources.Contains(resource) && (int)Storage_Settings.Get(resource).Priority > (int)building.Storage_Settings.Get(resource).Priority) {
                            float resources_taken = building.Take_Resources(resource, take);
                            resources_transfered += resources_taken;
                            Storage[resource] += resources_taken;
                        }
                    }
                }
            } else {
                foreach(Building building in connected_buildings) {
                    if (building.Id == Id) {
                        continue;
                    }
                    foreach (Resource resource in Consumes) {
                        float take = Math.Min(max_transfer - resources_transfered, INPUT_OUTPUT_STORAGE_LIMIT - Input_Storage[resource]);
                        if (building.Produces.Contains(resource)) {
                            float resources_taken = Mathf.Min(building.Output_Storage[resource], take);
                            building.Output_Storage[resource] -= resources_taken;
                            resources_transfered += resources_taken;
                            Input_Storage[resource] += resources_taken;
                        } else {
                            float resources_taken = building.Take_Resources(resource, take);
                            resources_transfered += resources_taken;
                            Input_Storage[resource] += resources_taken;
                        }
                    }
                }
            }
        }

        //Distributing resources
        if (Is_Operational && Is_Storehouse) {
            List<Building> connected_buildings = Get_Connected_Buildings(Road_Range).Select(x => x.Key).ToList();
            float resources_transfered = 0.0f;
            float max_transfer = Transfer_Speed * delta_days * Efficency;
            foreach (Building building in connected_buildings) {
                if (building.Id == Id || !building.Is_Complete) {
                    continue;
                }
                foreach (Resource resource in building.Consumes) {
                    if (!Storage.ContainsKey(resource)) {
                        continue;
                    }
                    if (!building.Input_Storage.ContainsKey(resource)) {
                        building.Input_Storage.Add(resource, 0.0f);
                    }
                    float give = Math.Min(max_transfer - resources_transfered, INPUT_OUTPUT_STORAGE_LIMIT - building.Input_Storage[resource]);
                    float resources_given = Mathf.Min(Storage[resource], give);
                    building.Input_Storage[resource] += resources_given;
                    resources_transfered += resources_given;
                    Storage[resource] -= resources_given;
                }
            }
        }

        if((Tags.Contains(Tag.Land_Trade) || Tags.Contains(Tag.Water_Trade)) && Trade_Route_Settings.Set) {
            Per_Day_Cash_Delta += Trade_Route_Settings.Cash_Delta;
            Update_Delta(Trade_Route_Settings.Resource, Trade_Route_Settings.Resource_Delta);
        }

        if(On_Update != null) {
            On_Update(this, delta_time);
        }
        
        foreach(KeyValuePair<Resource, float> pair in Output_Storage) {
            if(pair.Value >= INPUT_OUTPUT_STORAGE_LIMIT) {
                Show_Alert("alert_no_room");
                break;
            }
        }
        
        //TODO if non-cash upkeep is not provided, deteriorate HP (can't be destroyed this way)

        Refresh_Alerts(realtime_delta_time);
    }

    public float Efficency
    {
        get {
            if (Is_Town_Hall || Max_Workers_Total == 0) {
                return 1.0f;
            }
            float workers = 0.0f;
            float workers_needed = Max_Workers_Total;
            float worker_happiness_total = 0.0f;
            foreach(KeyValuePair<Resident, int> pair in Current_Workers) {
                workers += pair.Value;
                worker_happiness_total += pair.Value * City.Instance.Happiness[pair.Key];
            }
            if(workers == 0.0f) {
                return 0.0f;
            }
            float base_efficency = workers / workers_needed;
            float multiplier = 1.0f;
            float average_happiness = Mathf.Clamp01(worker_happiness_total / workers);

            float happiness_penalty_threshold = 0.40f;
            float happiness_penalty_max = 0.65f;
            float happiness_bonus_threshold = 0.60f;
            float happiness_bonus_max = 0.35f;
            if (average_happiness < happiness_penalty_threshold) {
                float penalty = happiness_penalty_max * ((happiness_penalty_threshold - average_happiness) / happiness_penalty_threshold);
                multiplier -= penalty;
            } else if(average_happiness > happiness_bonus_threshold) {
                float bonus = happiness_bonus_max * ((average_happiness - happiness_bonus_threshold) / (1.0f - happiness_bonus_threshold));
                multiplier += bonus;
            }

            float hp_penalty_threshold = 0.5f;
            float hp_penalty_max = 0.5f;
            float hp_relative = HP / Max_HP;
            if (hp_relative < hp_penalty_threshold) {
                float penalty = hp_penalty_max * ((hp_penalty_threshold - hp_relative) / hp_penalty_threshold);
                multiplier -= penalty;
            }

            float min_multiplier = 0.05f;
            multiplier = Math.Max(multiplier, min_multiplier);

            return base_efficency * multiplier;
        }
    }

    public int Width
    {
        get {
            switch (Size) {
                case BuildingSize.s1x1:
                    return 1;
                case BuildingSize.s2x2:
                    return 2;
                case BuildingSize.s3x3:
                    return 3;
            }
            CustomLogger.Instance.Error(string.Format("Size = {0}", Size.ToString()));
            return 1;
        }
    }

    public int Height
    {
        get {
            switch (Size) {
                case BuildingSize.s1x1:
                    return 1;
                case BuildingSize.s2x2:
                    return 2;
                case BuildingSize.s3x3:
                    return 3;
            }
            CustomLogger.Instance.Error(string.Format("Size = {0}", Size.ToString()));
            return 1;
        }
    }

    public int Workers_Allocated
    {
        get {
            int allocated = 0;
            foreach (Resident resident in Enum.GetValues(typeof(Resident))) {
                allocated += Worker_Settings[resident];
            }
            return allocated;
        }
    }

    public void Deconstruct(bool instant = false, bool refund = true)
    {
        Deconstruction_Progress = instant ? float.MaxValue : 0.0f;
        Is_Deconstructing = true;
        City.Instance.Add_Cash(Cash_Cost * REFUND);
        if (refund) {
            foreach (KeyValuePair<Resource, int> resource in Cost) {
                if (resource.Key == Resource.Tools) {
                    Store_Resources(Resource.Tools, resource.Value * TOOL_REFUND);
                } else {
                    Store_Resources(resource.Key, resource.Value * REFUND);
                }
            }
        }
        foreach(KeyValuePair<Resource, float> resource in Input_Storage) {
            Store_Resources(resource.Key, resource.Value);
        }
        foreach (KeyValuePair<Resource, float> resource in Output_Storage) {
            Store_Resources(resource.Key, resource.Value);
        }
        foreach (Building b in Map.Instance.Get_Buildings_Around(this)) {
            if (Is_Road && b.Is_Connected && b.Is_Complete) {
                b.Update_Connectivity();
            }
            b.Update_Sprite();
        }
        if(On_Deconstruct != null) {
            On_Deconstruct(this);
        }
        Dictionary<Resource, float> added = new Dictionary<Resource, float>();
        foreach(KeyValuePair<Resource, float> pair in Storage) {
            added.Add(pair.Key, City.Instance.Add_To_Storage(pair.Key, pair.Value));
        }
        foreach(KeyValuePair<Resource, float> pair in added) {
            Storage[pair.Key] -= pair.Value;
            if(Storage[pair.Key] < 0.0f) {
                CustomLogger.Instance.Error("Negative resource: " + pair.Key.ToString());
                Storage[pair.Key] = 0.0f;
            }
        }
    }

    public List<Tile> Get_Tiles_In_Circle(float range = -1.0f)
    {
        if(range == -1.0f) {
            range = Range;
        }
        if(range <= 0.0f) {
            return new List<Tile>();
        }
        Coordinates coordinates = Is_Preview ? new Coordinates((int)GameObject.transform.position.x, (int)GameObject.transform.position.y) : Tile.Coordinates;
        return Map.Instance.Get_Tiles_In_Circle(
            Size == BuildingSize.s3x3 ? coordinates.Shift(new Coordinates(1, 1)) : coordinates,
            range,
            Size == BuildingSize.s2x2,
            Size == BuildingSize.s2x2
        );
    }

    public BuildingSaveData Save_Data()
    {
        BuildingSaveData data = new BuildingSaveData() {
            Id = Id,
            Internal_Name = Internal_Name,
            X = Tile.Coordinates.X,
            Y = Tile.Coordinates.Y,
            Storage = new List<ResourceSaveData>(),
            Input_Storage = new List<ResourceSaveData>(),
            Output_Storage = new List<ResourceSaveData>(),
            Residents = new List<ResidentSaveData>(),
            Recently_Moved_Residents = new List<ResidentSaveData>(),
            Is_Residence = this is Residence,
            Worker_Allocation = new List<ResidentSaveData>(),
            Is_Deconstructing = Is_Deconstructing,
            Is_Connected = Is_Connected,
            Is_Paused = Is_Paused,
            Construction_Progress = Construction_Progress,
            Deconstruction_Progress = Deconstruction_Progress,
            HP = HP,
            Settings = new List<SpecialSettingSaveData>(),
            Services = new List<ServiceSaveData>(),
            Storage_Settings = new List<StorageSettingSaveData>(),
            Selected_Sprite = Selected_Sprite
        };
        foreach(KeyValuePair<Resource, float> pair in Storage) {
            data.Storage.Add(new ResourceSaveData() { Resource = pair.Key.Id, Amount = pair.Value });
        }
        foreach (KeyValuePair<Resource, float> pair in Input_Storage) {
            data.Input_Storage.Add(new ResourceSaveData() { Resource = pair.Key.Id, Amount = pair.Value });
        }
        foreach (KeyValuePair<Resource, float> pair in Output_Storage) {
            data.Output_Storage.Add(new ResourceSaveData() { Resource = pair.Key.Id, Amount = pair.Value });
        }
        foreach (KeyValuePair<Resident, int> pair in Worker_Settings) {
            data.Worker_Allocation.Add(new ResidentSaveData() { Resident = (int)pair.Key, Count = pair.Value });
        }
        if(this is Residence) {
            foreach (KeyValuePair<Resident, int> pair in (this as Residence).Current_Residents) {
                data.Residents.Add(new ResidentSaveData() { Resident = (int)pair.Key, Count = pair.Value });
            }
            foreach (KeyValuePair<Resident, int> pair in (this as Residence).Recently_Moved) {
                data.Recently_Moved_Residents.Add(new ResidentSaveData() { Resident = (int)pair.Key, Count = pair.Value });
            }
            foreach (Residence.ServiceType service in Enum.GetValues(typeof(Residence.ServiceType))) {
                if ((this as Residence).Service_Level(service) > 0.0f) {
                    data.Services.Add(new ServiceSaveData() {
                        Service = (int)service,
                        Amount = (this as Residence).Service_Level(service),
                        Quality = (this as Residence).Service_Quality(service)
                    });
                }
            }
        }
        foreach(SpecialSetting setting in Special_Settings) {
            data.Settings.Add(new SpecialSettingSaveData() {
                Name = setting.Name,
                Slider_Value = setting.Slider_Value,
                Toggle_Value = setting.Toggle_Value,
                Dropdown_Selection = setting.Dropdown_Selection
            });
        }
        if (Is_Storehouse) {
            foreach(StorageSetting setting in Storage_Settings.Settings) {
                data.Storage_Settings.Add(new StorageSettingSaveData() {
                    Resource = (int)setting.Resource.Type,
                    Limit = setting.Limit,
                    Priority = (int)setting.Priority
                });
            }
        }
        if(Tags.Contains(Tag.Land_Trade) || Tags.Contains(Tag.Water_Trade)) {
            data.Trade_Route_Settings = Trade_Route_Settings.Save_Data;
        }
        return data;
    }

    /// <summary>
    /// TODO: High game speeds create small error in amount of produced resources
    /// </summary>
    /// <param name="resource"></param>
    /// <param name="amount_per_day"></param>
    /// <param name="delta_time"></param>
    public void Produce(Resource resource, float amount_per_day, float delta_time)
    {
        float actual_amount = Calculate_Actual_Amount(amount_per_day * Efficency, delta_time);
        float space = Mathf.Max(0.0f, Output_Storage.ContainsKey(resource) ? INPUT_OUTPUT_STORAGE_LIMIT - Output_Storage[resource] : INPUT_OUTPUT_STORAGE_LIMIT);
        float stored = Mathf.Min(space, actual_amount);
        if (Output_Storage.ContainsKey(resource)) {
            Output_Storage[resource] += stored;
        } else {
            Output_Storage.Add(resource, stored);
        }
        if(Output_Storage[resource] == INPUT_OUTPUT_STORAGE_LIMIT) {
            Show_Alert("alert_no_room");
        }
        Update_Delta(resource, amount_per_day * Efficency);
    }

    public void Process(Resource input, float input_amount, Resource output, float output_amount, float delta_time)
    {
        Process(new Dictionary<Resource, float>() { { input, input_amount } }, new Dictionary<Resource, float>() { { output, output_amount } }, delta_time);
    }

    public void Process(Dictionary<Resource, float> inputs, Dictionary<Resource, float> outputs, float delta_time)
    {
        float min_input_relative = 1.0f;
        float efficency = Efficency;
        Dictionary<Resource, float> consumed_resouces = new Dictionary<Resource, float>();
        foreach(KeyValuePair<Resource, float> pair in inputs) {
            float amount = pair.Value * efficency;
            float actual_amount = Calculate_Actual_Amount(amount, delta_time);
            float actual_amount_desired = actual_amount;
            Update_Delta(pair.Key, -amount);
            if (!Input_Storage.ContainsKey(pair.Key)) {
                actual_amount = 0.0f;
            } else if(Input_Storage[pair.Key] < actual_amount) {
                actual_amount = Input_Storage[pair.Key];
            }
            float relative = actual_amount / actual_amount_desired;
            if(relative < min_input_relative) {
                min_input_relative = relative;
            }
            consumed_resouces.Add(pair.Key, actual_amount);
        }
        if(min_input_relative == 0.0f) {
            Show_Alert("alert_no_resources");
            return;
        }

        float min_output_relative = 1.0f;
        Dictionary<Resource, float> produced_resouces = new Dictionary<Resource, float>();
        foreach (KeyValuePair<Resource, float> pair in outputs) {
            float amount = pair.Value * efficency;
            float actual_amount = Calculate_Actual_Amount(amount, delta_time);
            float actual_amount_desired = actual_amount;

            float cap = Output_Storage.ContainsKey(pair.Key) ? INPUT_OUTPUT_STORAGE_LIMIT - Output_Storage[pair.Key] : INPUT_OUTPUT_STORAGE_LIMIT;
            if(cap < actual_amount) {
                actual_amount = cap;
            }

            float relative = actual_amount / actual_amount_desired;
            if (relative < min_output_relative) {
                min_output_relative = relative;
            }
            produced_resouces.Add(pair.Key, actual_amount);
        }
        if(min_output_relative == 0.0f) {
            Show_Alert("alert_no_room");
            return;
        }

        foreach (KeyValuePair<Resource, float> pair in consumed_resouces) {
            float consumed = pair.Value;
            if (min_output_relative < min_input_relative) {
                consumed = Calculate_Actual_Amount(inputs[pair.Key] * efficency, delta_time) * min_output_relative;
            }
            if (!Input_Storage.ContainsKey(pair.Key)) {
                CustomLogger.Instance.Error(string.Format("Out of {0}", pair.Key));
                return;
            }
            Input_Storage[pair.Key] -= consumed;
            if (Input_Storage[pair.Key] < 0.0f) {
                CustomLogger.Instance.Error(string.Format("Negative {0}", pair.Key));
                Input_Storage[pair.Key] = 0.0f;
            }
        }

        foreach(KeyValuePair<Resource, float> pair in produced_resouces) {
            float produced = pair.Value;
            float per_day = outputs[pair.Key] * efficency * min_output_relative;
            if (min_input_relative < min_output_relative) {
                produced = Calculate_Actual_Amount(outputs[pair.Key] * efficency, delta_time) * min_input_relative;
                per_day = outputs[pair.Key] * efficency * min_input_relative;
            }
            if (!Output_Storage.ContainsKey(pair.Key)) {
                Output_Storage.Add(pair.Key, 0.0f);
            }
            Output_Storage[pair.Key] += produced;
            if(Output_Storage[pair.Key] > INPUT_OUTPUT_STORAGE_LIMIT) {
                CustomLogger.Instance.Error(string.Format("Too much {0}", pair.Key));
                Output_Storage[pair.Key] = INPUT_OUTPUT_STORAGE_LIMIT;
            }
            Update_Delta(pair.Key, per_day);
        }
    }

    public static float Calculate_Actual_Amount(float amount_per_day, float delta_time)
    {
        return (amount_per_day / TimeManager.Instance.Days_To_Seconds(1.0f, 1.0f)) * delta_time;
    }

    public void Update_Delta(Resource resource, float amount_per_day, bool update_food = true)
    {
        if (Per_Day_Resource_Delta.ContainsKey(resource)) {
            Per_Day_Resource_Delta[resource] += amount_per_day;
        } else {
            Per_Day_Resource_Delta.Add(resource, amount_per_day);
        }
        if (resource.Is_Food && update_food) {
            Food_Production_Per_Day += amount_per_day;
        }
    }

    public void Delete()
    {
        GameObject.Destroy(GameObject);
        Is_Deleted = true;
        if (!Is_Preview) {
            if (Tile != null) {
                Tile.Building = null;
            }
            foreach (Tile t in Tiles) {
                t.Building = null;
            }
            foreach(Entity entity in Entities_Spawned) {
                entity.Spawner = null;
                Map.Instance.Delete_Entity(entity);
            }
        }
    }

    public override string ToString()
    {
        return Is_Prototype ? string.Format("{0} prototype", Internal_Name) : string.Format("{0} (#{1})", Internal_Name, Id);
    }

    protected void Update_Appeal()
    {
        if (Is_Deleted) {
            foreach (Tile tile in Tiles) {
                if (Appeal != 0.0f) {
                    tile.Appeal -= Appeal;
                    if (Appeal_Range != 0.0f) {
                        foreach (Tile affected in Map.Instance.Get_Tiles_In_Circle(tile.Coordinates, Appeal_Range)) {
                            if (affected == tile) {
                                continue;
                            }
                            affected.Appeal -= Tile.Calculate_Appeal_Effect(tile.Coordinates, Appeal, Appeal_Range, affected.Coordinates);
                        }
                    }
                }
            }
            foreach (Tile tile in Tiles) {
                if (tile.Base_Appeal != 0.0f) {
                    tile.Appeal += tile.Base_Appeal;
                    if (tile.Base_Appeal_Range != 0.0f) {
                        foreach (Tile affected in Map.Instance.Get_Tiles_In_Circle(tile.Coordinates, tile.Base_Appeal_Range)) {
                            if (affected == tile) {
                                continue;
                            }
                            affected.Appeal += Tile.Calculate_Appeal_Effect(tile.Coordinates, tile.Base_Appeal, tile.Base_Appeal_Range, affected.Coordinates);
                        }
                    }
                }
            }
        } else {
            foreach (Tile tile in Tiles) {
                if (tile.Base_Appeal != 0.0f) {
                    tile.Appeal -= tile.Base_Appeal;
                    if (tile.Base_Appeal_Range != 0.0f) {
                        foreach (Tile affected in Map.Instance.Get_Tiles_In_Circle(tile.Coordinates, tile.Base_Appeal_Range)) {
                            if (affected == tile) {
                                continue;
                            }
                            affected.Appeal -= Tile.Calculate_Appeal_Effect(tile.Coordinates, tile.Base_Appeal, tile.Base_Appeal_Range, affected.Coordinates);
                        }
                    }
                }
            }
            foreach (Tile tile in Tiles) {
                if (Appeal != 0.0f) {
                    tile.Appeal += Appeal;
                    if (Appeal_Range != 0.0f) {
                        foreach (Tile affected in Map.Instance.Get_Tiles_In_Circle(tile.Coordinates, Appeal_Range)) {
                            if (affected == tile) {
                                continue;
                            }
                            affected.Appeal += Tile.Calculate_Appeal_Effect(tile.Coordinates, Appeal, Appeal_Range, affected.Coordinates);
                        }
                    }
                }
            }
        }
    }

    private GameObject Get_Prefab()
    {
        if(Is_Road && !Sprite.Simple) {
            return PrefabManager.Instance.Road;
        }
        return PrefabManager.Instance.Building_Generic;
    }

    public void Show_Alert(string sprite)
    {
        if (!active_alerts.Contains(sprite) && !Map.Instance.Hide_Alerts) {
            active_alerts.Add(sprite);
        }
    }

    private void Refresh_Alerts(float delta_time)
    {
        List<GameObject> destroyed_alerts = new List<GameObject>();
        foreach(GameObject gameobject in alerts) {
            if (!active_alerts.Contains(gameobject.name)) {
                GameObject.Destroy(gameobject);
                destroyed_alerts.Add(gameobject);
            }
        }
        foreach(GameObject alert in destroyed_alerts) {
            alerts.Remove(alert);
        }
        foreach(string icon in active_alerts) {
            if(!alerts.Exists(x => x.name == icon)) {
                GameObject new_alert = GameObject.Instantiate(
                    PrefabManager.Instance.Alert,
                    new Vector3(
                        GameObject.transform.position.x,
                        GameObject.transform.position.y,
                        GameObject.transform.position.z
                    ),
                    Quaternion.identity,
                    GameObject.transform
                );
                new_alert.name = icon;
                new_alert.SetActive(false);
                new_alert.GetComponentInChildren<SpriteRenderer>().sprite = SpriteManager.Instance.Get(icon, SpriteManager.SpriteType.UI);
                alerts.Add(new_alert);
            }
        }

        if(alerts.Count == 1) {
            active_alert = alerts[0];
            if (!active_alert.activeSelf) {
                active_alert.SetActive(true);
            }
        } else if(alerts.Count > 1) {
            if (active_alert == null) {
                active_alert = alerts[0];
                active_alert.SetActive(true);
            } else {
                alert_change_cooldown -= delta_time;
                if (alert_change_cooldown <= 0.0f) {
                    alert_change_cooldown += ALERT_CHANGE_INTERVAL;
                    active_alert.SetActive(false);
                    if(alerts.IndexOf(active_alert) == alerts.Count - 1) {
                        active_alert = alerts[0];
                    } else {
                        active_alert = alerts[alerts.IndexOf(active_alert) + 1];
                    }
                    active_alert.SetActive(true);
                }
            }
        } else {
            active_alert = null;
        }

        active_alerts.Clear();
    }

    private void Update_Connectivity()
    {
        Dictionary<Building, int> connected_buildings = Get_Connected_Buildings();
        bool connected = connected_buildings.Select(x => x.Key).ToList().Exists(x => x.Is_Town_Hall);
        if (Is_Road && Is_Complete) {
            foreach (KeyValuePair<Building, int> pair in connected_buildings) {
                pair.Key.Is_Connected = connected;
            }
        }
        Is_Connected = connected;
    }

    public Dictionary<Building, int> Get_Connected_Buildings(int range = -1)
    {
        Dictionary<Building, int> connected = new Dictionary<Building, int>();
        foreach(Building b in Map.Instance.Get_Buildings_Around(this)) {
            Get_Connected_Buildings_Recursive(b, this, ref connected, range, 0, true);
        }
        return connected;
    }

    private void Get_Connected_Buildings_Recursive(Building building, Building previous, ref Dictionary<Building, int> connected, int range, int distance, bool first)
    {
        if ((connected.ContainsKey(building) && connected[building] <= distance) || (!building.Is_Road && !previous.Is_Road)) {
            return;
        }
        if(range > 0 && distance > range) {
            return;
        }
        if (!connected.ContainsKey(building)) {
            connected.Add(building, distance);
        } else {
            connected[building] = distance;
        }
        if(!(building.Is_Road && building.Is_Complete)) {
            return;
        }
        foreach(Building b in Map.Instance.Get_Buildings_Around(building)) {
            Get_Connected_Buildings_Recursive(b, building, ref connected, range, distance + 1, false);
        }
    }

    private Dictionary<Resident, int> Make_Resident_Dictionary(Dictionary<Resident, int> param = null)
    {
        Dictionary<Resident, int> dictionary = new Dictionary<Resident, int>();
        foreach(Resident resident in Enum.GetValues(typeof(Resident))) {
            if(param != null && param.ContainsKey(resident)) {
                dictionary.Add(resident, param[resident]);
            } else {
                dictionary.Add(resident, 0);
            }
        }
        return dictionary;
    }

    private void Update_Sprite(bool ignore_adjacent = false)
    {
        int order = Map.Instance.Height - (int)GameObject.transform.position.y;
        if(Renderer.sortingOrder != order) {
            Renderer.sortingOrder = order;
        }
        if (!ignore_adjacent) {
            foreach (Building building in Map.Instance.Get_Buildings_Around(this)) {
                if (!building.Sprite.Simple) {
                    building.Update_Sprite(true);
                }
            }
        }
        if ((Is_Built || Is_Town_Hall || Is_Preview) && !Is_Deconstructing) {
            if (Sprite.Simple || (Sprite.Animation_Sprites.Count != 0 && Is_Preview)) {
                Renderer.sprite = SpriteManager.Instance.Get(Sprite.Name, Sprite.Type, Is_Preview);
            } else if(Sprite.Logic != null) {
                Renderer.sprite = SpriteManager.Instance.Get(Sprite.Logic(this), Sprite.Type, false);
            } else {
                if (!animation_initialized) {
                    Renderer.sprite = SpriteManager.Instance.Get(Sprite.Animation_Sprites[animation_index], Sprite.Type, false);
                    animation_initialized = true;
                }
                if(animation_cooldown <= 0.0f) {
                    animation_cooldown += Sprite.Animation_Frame_Time;
                    animation_index++;
                    if(animation_index >= Sprite.Animation_Sprites.Count) {
                        animation_index = 0;
                    }
                    Renderer.sprite = SpriteManager.Instance.Get(Sprite.Animation_Sprites[animation_index], Sprite.Type, false);
                }
            }
            return;
        }
        if((Is_Deconstructing && Deconstruction_Progress >= Construction_Time * 0.66f) || Construction_Progress <= Construction_Time * 0.33f) {
            switch (Size) {
                case BuildingSize.s1x1:
                    Renderer.sprite = SpriteManager.Instance.Get("construction_1x1_1", SpriteManager.SpriteType.Building, false);
                    break;
                case BuildingSize.s2x2:
                    Renderer.sprite = SpriteManager.Instance.Get("construction_2x2_1", SpriteManager.SpriteType.Building, false);
                    break;
                case BuildingSize.s3x3:
                    Renderer.sprite = SpriteManager.Instance.Get("construction_3x3_1", SpriteManager.SpriteType.Building, false);
                    break;
                default:
                    CustomLogger.Instance.Error(string.Format("Size = {0}", Size.ToString()));
                    break;
            }
        } else if((Is_Deconstructing && Deconstruction_Progress < Construction_Time * 0.66f && Deconstruction_Progress >= Construction_Time * 0.33f) ||
            (Construction_Progress > Construction_Time * 0.33f && Construction_Progress <= Construction_Time * 0.66f)) {
            switch (Size) {
                case BuildingSize.s1x1:
                    Renderer.sprite = SpriteManager.Instance.Get("construction_1x1_2", SpriteManager.SpriteType.Building, false);
                    break;
                case BuildingSize.s2x2:
                    Renderer.sprite = SpriteManager.Instance.Get("construction_2x2_2", SpriteManager.SpriteType.Building, false);
                    break;
                case BuildingSize.s3x3:
                    Renderer.sprite = SpriteManager.Instance.Get("construction_3x3_2", SpriteManager.SpriteType.Building, false);
                    break;
                default:
                    CustomLogger.Instance.Error(string.Format("Size = {0}", Size.ToString()));
                    break;
            }
        } else {
            switch (Size) {
                case BuildingSize.s1x1:
                    Renderer.sprite = SpriteManager.Instance.Get("construction_1x1_3", SpriteManager.SpriteType.Building, false);
                    break;
                case BuildingSize.s2x2:
                    Renderer.sprite = SpriteManager.Instance.Get("construction_2x2_3", SpriteManager.SpriteType.Building, false);
                    break;
                case BuildingSize.s3x3:
                    Renderer.sprite = SpriteManager.Instance.Get("construction_3x3_3", SpriteManager.SpriteType.Building, false);
                    break;
                default:
                    CustomLogger.Instance.Error(string.Format("Size = {0}", Size.ToString()));
                    break;
            }
        }
    }

    public static void Reset_Current_Id()
    {
        current_id = 0;
    }
}
