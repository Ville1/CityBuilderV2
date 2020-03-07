using System.Collections.Generic;
using UnityEngine;

public class Building {
    private static long current_id = 0;

    public static readonly float UPDATE_INTERVAL = 1.0f;
    public static readonly string TOWN_HALL_INTERNAL_NAME = "town_hall";
    public static int INPUT_OUTPUT_STORAGE_LIMIT = 100;
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
    public bool Is_Preview { get; private set; }
    public Dictionary<Resource, int> Cost { get; private set; }
    public int Cash_Cost { get; private set; }
    public Dictionary<Resource, int> Storage { get; private set; }
    public Dictionary<Resource, int> Input_Storage { get; private set; }
    public Dictionary<Resource, int> Output_Storage { get; private set; }
    public int Storage_Limit { get; private set; }
    public List<Resource> Allowed_Resources { get; private set; }
    public bool Is_Storehouse { get { return Storage_Limit > 0 && Allowed_Resources != null && Allowed_Resources.Count != 0; } }
    public int Construction_Time { get; private set; }
    public Dictionary<Resource, float> Upkeep { get; private set; }
    public float Construction_Progress { get; private set; }
    public bool Is_Built { get { return Is_Town_Hall || Construction_Progress == Construction_Time; } }
    public float Construction_Speed { get; private set; }
    public float Construction_Range { get; private set; }

    public GameObject GameObject { get; private set; }
    public SpriteRenderer Renderer { get { return GameObject != null ? GameObject.GetComponent<SpriteRenderer>() : null; } }

    protected float update_cooldown;
    protected bool update_on_last_call;

    public Building(Building prototype, Tile tile, bool is_preview)
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
        Tile = tile;
        Tile.Building = this;
        Is_Preview = is_preview;
        Cost = Helper.Clone_Dictionary(prototype.Cost);
        Cash_Cost = prototype.Cash_Cost;
        Allowed_Resources = Helper.Clone_List(prototype.Allowed_Resources);
        Storage = new Dictionary<Resource, int>();
        Storage_Limit = prototype.Storage_Limit;
        Input_Storage = new Dictionary<Resource, int>();
        Output_Storage = new Dictionary<Resource, int>();
        Construction_Time = prototype.Construction_Time;
        Upkeep = Helper.Clone_Dictionary(prototype.Upkeep);
        Construction_Progress = 0.0f;
        Construction_Speed = prototype.Construction_Speed;
        Construction_Range = prototype.Construction_Range;
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

    public Building(string name, string internal_name, UI_Category category, string sprite, BuildingSize size, Dictionary<Resource, int> cost, int cash_cost, List<Resource> allowed_resources, int storage_limit, int construction_time,
        Dictionary<Resource, float> upkeep, float construction_speed, float construction_range)
    {
        Id = -1;
        Name = name;
        Internal_Name = internal_name;
        Category = category;
        Sprite = sprite;
        Size = size;
        Is_Preview = false;
        Cost = Helper.Clone_Dictionary(cost);
        Cash_Cost = cash_cost;
        Allowed_Resources = Helper.Clone_List(allowed_resources);
        Storage = new Dictionary<Resource, int>();
        Storage_Limit = storage_limit;
        Input_Storage = new Dictionary<Resource, int>();
        Output_Storage = new Dictionary<Resource, int>();
        Construction_Time = construction_time;
        Upkeep = Helper.Clone_Dictionary(upkeep);
        Construction_Progress = 0.0f;
        Construction_Speed = construction_speed;
        Construction_Range = construction_range;
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

    public int Current_Storage_Amount
    {
        get {
            int amount = 0;
            foreach(KeyValuePair<Resource, int> pair in Storage) {
                amount += pair.Value;
            }
            return amount;
        }
    }

    public bool Store_Resources(Resource resource, int amount)
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

    public int Take_Resources(Resource resource, int amount)
    {
        if (!Storage.ContainsKey(resource)) {
            return 0;
        }
        if(Storage[resource] < amount) {
            int i = Storage[resource];
            Storage[resource] = 0;
            return i;
        }
        Storage[resource] = Storage[resource] - amount;
        return amount;
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
    }

    public float Efficency
    {
        get {
            if (Is_Storehouse) {
                return 1.0f;
            }
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

    public void Delete()
    {
        GameObject.Destroy(GameObject);
        if(Tile != null) {
            Tile.Building = null;
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
        if (Is_Built || Is_Town_Hall || Is_Preview) {
            Renderer.sprite = SpriteManager.Instance.Get(Sprite, SpriteManager.SpriteType.Buildings, Is_Preview);
            return;
        }
        if(Construction_Progress <= Construction_Time * 0.33f) {
            switch (Size) {
                case BuildingSize.s1x1:
                    Renderer.sprite = SpriteManager.Instance.Get("construction_1x1_1", SpriteManager.SpriteType.Buildings, false);
                    break;
                case BuildingSize.s2x2:
                    Renderer.sprite = SpriteManager.Instance.Get("construction_2x2_1", SpriteManager.SpriteType.Buildings, false);
                    break;
                case BuildingSize.s3x3:
                    Renderer.sprite = SpriteManager.Instance.Get("construction_3x3_1", SpriteManager.SpriteType.Buildings, false);
                    break;
                default:
                    CustomLogger.Instance.Error(string.Format("Size = {0}", Size.ToString()));
                    break;
            }
        } else if(Construction_Progress > Construction_Time * 0.33f && Construction_Progress <= Construction_Time * 0.66f) {
            switch (Size) {
                case BuildingSize.s1x1:
                    Renderer.sprite = SpriteManager.Instance.Get("construction_1x1_2", SpriteManager.SpriteType.Buildings, false);
                    break;
                case BuildingSize.s2x2:
                    Renderer.sprite = SpriteManager.Instance.Get("construction_2x2_2", SpriteManager.SpriteType.Buildings, false);
                    break;
                case BuildingSize.s3x3:
                    Renderer.sprite = SpriteManager.Instance.Get("construction_3x3_2", SpriteManager.SpriteType.Buildings, false);
                    break;
                default:
                    CustomLogger.Instance.Error(string.Format("Size = {0}", Size.ToString()));
                    break;
            }
        } else {
            switch (Size) {
                case BuildingSize.s1x1:
                    Renderer.sprite = SpriteManager.Instance.Get("construction_1x1_3", SpriteManager.SpriteType.Buildings, false);
                    break;
                case BuildingSize.s2x2:
                    Renderer.sprite = SpriteManager.Instance.Get("construction_2x2_3", SpriteManager.SpriteType.Buildings, false);
                    break;
                case BuildingSize.s3x3:
                    Renderer.sprite = SpriteManager.Instance.Get("construction_3x3_3", SpriteManager.SpriteType.Buildings, false);
                    break;
                default:
                    CustomLogger.Instance.Error(string.Format("Size = {0}", Size.ToString()));
                    break;
            }
        }
    }
}
