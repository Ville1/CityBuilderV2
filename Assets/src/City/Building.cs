﻿using System.Collections.Generic;
using UnityEngine;

public class Building {
    private static long current_id = 0;

    public static readonly float UPDATE_INTERVAL = 1.0f;
    public static readonly string TOWN_HALL_INTERNAL_NAME = "town_hall";
    public static int INPUT_OUTPUT_STORAGE_LIMIT = 100;
    public static float DECONSTRUCTION_SPEED = 10.0f;
    public static float REFOUND = 0.50f;
    public static float TOOL_REFOUND = 0.10f;
    public enum UI_Category { Admin, Infrastructure, Housing, Services, Forestry, Agriculture, Industry }
    public enum Resident { Peasant, Citizen, Noble }
    public enum BuildingSize { s1x1, s2x2, s3x3 }

    public long Id { get; private set; }
    public string Name { get; private set; }
    public string Internal_Name { get; private set; }
    public UI_Category Category { get; private set; }
    public string Sprite { get; private set; }
    public bool Is_Prototype { get { return Id < 0; } }
    public bool Is_Town_Hall { get { return Internal_Name == TOWN_HALL_INTERNAL_NAME; } }
    public BuildingSize Size { get; private set; }
    public Tile Tile { get; private set; }
    public List<Tile> Tiles { get; private set; }
    public bool Is_Preview { get; private set; }
    public Dictionary<Resource, int> Cost { get; private set; }
    public int Cash_Cost { get; private set; }
    public Dictionary<Resource, float> Storage { get; private set; }
    public Dictionary<Resource, float> Input_Storage { get; private set; }
    public Dictionary<Resource, float> Output_Storage { get; private set; }
    public int Storage_Limit { get { return Is_Deconstructing ? int.MaxValue : storage_limit; } set { storage_limit = value; } }
    public List<Resource> Allowed_Resources { get; private set; }
    public bool Is_Storehouse { get { return storage_limit > 0 && Allowed_Resources != null && Allowed_Resources.Count != 0; } }
    public int Construction_Time { get; private set; }
    public Dictionary<Resource, float> Upkeep { get; private set; }
    public float Cash_Upkeep { get; private set; }
    public float Construction_Progress { get; private set; }
    public float Deconstruction_Progress { get; private set; }
    public bool Is_Built { get { return Is_Town_Hall || Construction_Progress == Construction_Time; } }
    public bool Is_Operational { get { return Is_Built && !Is_Paused && !Is_Deconstructing; } }
    public float Construction_Speed { get; private set; }
    public float Construction_Range { get; private set; }
    public Dictionary<Resident, int> Max_Workers { get; private set; }
    public int Max_Workers_Total { get; private set; }
    public Dictionary<Resident, int> Worker_Settings { get; private set; }
    public Dictionary<Resident, int> Current_Workers { get; private set; }
    public bool Can_Be_Paused { get { return can_be_paused && !Is_Deconstructing && Is_Built; } set { can_be_paused = value; } }
    public bool Can_Be_Deleted { get { return !Is_Town_Hall && !Is_Deconstructing && Is_Built; } }
    public int Max_HP { get; private set; }
    public float HP { get; private set; }
    public bool Is_Paused { get; set; }
    public bool Is_Deconstructing { get; private set; }
    public bool Is_Deleted { get; private set; }

    public GameObject GameObject { get; private set; }
    public SpriteRenderer Renderer { get { return GameObject != null ? GameObject.GetComponent<SpriteRenderer>() : null; } }

    protected float update_cooldown;
    protected bool update_on_last_call;
    private bool can_be_paused;
    private int storage_limit;

    public Building(Building prototype, Tile tile, List<Tile> tiles, bool is_preview)
    {
        if (!is_preview) {
            Id = current_id;
            current_id++;
        }
        Name = prototype.Name;
        Internal_Name = prototype.Internal_Name;
        Category = prototype.Category;
        Sprite = prototype.Sprite;
        Size = prototype.Size;
        HP = prototype.HP;
        Max_HP = prototype.Max_HP;
        if (!is_preview) {
            Tile = tile;
            Tiles = tiles;
            foreach (Tile t in tiles) {
                t.Building = this;
            }
        }
        Is_Preview = is_preview;
        Cost = Helper.Clone_Dictionary(prototype.Cost);
        Cash_Cost = prototype.Cash_Cost;
        Allowed_Resources = Helper.Clone_List(prototype.Allowed_Resources);
        Storage = new Dictionary<Resource, float>();
        Storage_Limit = prototype.Storage_Limit;
        Input_Storage = new Dictionary<Resource, float>();
        Output_Storage = new Dictionary<Resource, float>();
        Construction_Time = prototype.Construction_Time;
        Upkeep = Helper.Clone_Dictionary(prototype.Upkeep);
        Cash_Upkeep = prototype.Cash_Upkeep;
        Construction_Progress = 0.0f;
        Construction_Speed = prototype.Construction_Speed;
        Construction_Range = prototype.Construction_Range;
        Max_Workers = Helper.Clone_Dictionary(prototype.Max_Workers);
        Max_Workers_Total = prototype.Max_Workers_Total;
        Worker_Settings = Helper.Clone_Dictionary(prototype.Max_Workers);
        Current_Workers = new Dictionary<Resident, int>();
        Can_Be_Paused = prototype.can_be_paused;
        Is_Paused = false;

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

        Update_Sprite();
    }

    public Building(string name, string internal_name, UI_Category category, string sprite, BuildingSize size, int hp, Dictionary<Resource, int> cost, int cash_cost, List<Resource> allowed_resources, int storage_limit, int construction_time,
        Dictionary<Resource, float> upkeep, float cash_upkeep, float construction_speed, float construction_range, Dictionary<Resident, int> workers, int max_workers, bool can_be_paused)
    {
        Id = -1;
        Name = name;
        Internal_Name = internal_name;
        Category = category;
        Sprite = sprite;
        Size = size;
        Max_HP = hp;
        HP = Max_HP;
        Is_Preview = false;
        Cost = Helper.Clone_Dictionary(cost);
        Cash_Cost = cash_cost;
        Allowed_Resources = Helper.Clone_List(allowed_resources);
        Storage = new Dictionary<Resource, float>();
        Storage_Limit = storage_limit;
        Input_Storage = new Dictionary<Resource, float>();
        Output_Storage = new Dictionary<Resource, float>();
        Construction_Time = construction_time;
        Upkeep = Helper.Clone_Dictionary(upkeep);
        Cash_Upkeep = cash_upkeep;
        Construction_Progress = 0.0f;
        Construction_Speed = construction_speed;
        Construction_Range = construction_range;
        Max_Workers = Helper.Clone_Dictionary(workers);
        Max_Workers_Total = max_workers;
        Worker_Settings = new Dictionary<Resident, int>();
        Current_Workers = new Dictionary<Resident, int>();
        Can_Be_Paused = can_be_paused;
        Is_Paused = false;
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

    public bool Store_Resources(Resource resource, float amount)
    {
        if(Current_Storage_Amount + amount > Storage_Limit) {
            return false;
        }
        if (Storage.ContainsKey(resource)) {
            Storage[resource] = Storage[resource] + amount;
        } else {
            Storage.Add(resource, amount);
        }
        return true;
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
        delta_time = update_cooldown * TimeManager.Instance.Multiplier;
        update_on_last_call = true;

        if(!Can_Be_Paused && Is_Paused) {
            CustomLogger.Instance.Warning(string.Format("{0} can't be paused", Internal_Name));
            Is_Paused = false;
        }

        if (Is_Deconstructing) {
            Deconstruction_Progress += delta_time * DECONSTRUCTION_SPEED;
            Update_Sprite();
            if(Deconstruction_Progress >= Construction_Time) {
                Delete();
                City.Instance.Remove_Building(this);
            }
            return;
        }

        if(Construction_Speed > 0.0f && Construction_Range > 0.0f) {
            float construction_progress = Efficency * delta_time;
            foreach(Building building in City.Instance.Buildings) {
                if (building.Is_Built) {
                    continue;
                }
                float range_multiplier = 1.0f;
                if(Tile.Coordinates.Distance(building.Tile.Coordinates) > Construction_Range) {
                    range_multiplier = 10.0f / (10.0f + (Tile.Coordinates.Distance(building.Tile.Coordinates) - Construction_Range));
                }
                building.Construction_Progress = Mathf.Clamp(building.Construction_Progress + (range_multiplier * construction_progress), 0.0f, building.Construction_Time);
                building.Update_Sprite();
            }
        }

        //TODO if non-cash upkeep is not provided, deteriorate HP (can't be destroyed this way)
    }

    public float Efficency
    {
        get {
            if (Is_Town_Hall) {
                return 1.0f;
            }
            //TODO: worker types, worker happiness, HP
            return 1.0f;
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

    public void Deconstruct()
    {
        Deconstruction_Progress = 0.0f;
        Is_Deconstructing = true;
        City.Instance.Add_Cash(Cash_Cost * REFOUND);
        foreach(KeyValuePair<Resource, int> resource in Cost) {
            if(resource.Key == Resource.Tools) {
                Store_Resources(Resource.Tools, resource.Value * TOOL_REFOUND);
            } else {
                Store_Resources(resource.Key, resource.Value * REFOUND);
            }
        }
        foreach(KeyValuePair<Resource, float> resource in Input_Storage) {
            Store_Resources(resource.Key, resource.Value);
        }
        foreach (KeyValuePair<Resource, float> resource in Output_Storage) {
            Store_Resources(resource.Key, resource.Value);
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
        }
    }

    public override string ToString()
    {
        return Is_Prototype ? string.Format("{0} prototype", Internal_Name) : string.Format("{0} (#{1})", Internal_Name, Id);
    }

    private GameObject Get_Prefab()
    {
        if(Size == BuildingSize.s2x2) {
            return PrefabManager.Instance.Building_2x2;
        }
        CustomLogger.Instance.Error(string.Format("{0}x{1} prefab does not exist", Width, Height));
        return null;
    }

    private void Update_Sprite()
    {
        if ((Is_Built || Is_Town_Hall || Is_Preview) && !Is_Deconstructing) {
            Renderer.sprite = SpriteManager.Instance.Get(Sprite, SpriteManager.SpriteType.Building, Is_Preview);
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
}
