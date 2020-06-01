using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Tile
{
    public static readonly string GAME_OBJECT_NAME_PREFIX = "Tile_";
    public enum Fog_Of_War_Status { Visible, Not_Visible, Not_Explored }
    public enum Work_Type { Forage, Hunt, Fish, Farm, Cut_Wood, Mine, Trap }

    private static long current_id = 0;
    
    public long Id { get; private set; }
    public string Internal_Name { get; private set; }
    public string Terrain { get; private set; }
    public string Sprite { get; private set; }
    public List<string> Animation_Sprites { get; private set; }
    public bool Sync_Animation { get; private set; }
    public float Animation_Framerate { get; private set; }
    public bool Is_Animated { get { return Animation_Sprites != null && Animation_Sprites.Count > 1 && Animation_Framerate > 0.0f; } }

    public int X { get; private set; }
    public int Y { get; private set; }
    public GameObject GameObject { get; private set; }
    public SpriteRenderer SpriteRenderer { get { return GameObject.GetComponent<SpriteRenderer>(); } }
    public bool Destroyed { get; private set; }
    public bool Buildable { get; private set; }
    public bool Is_Prototype { get { return Id < 0; } }
    public Building Building { get; set; }
    public float Base_Appeal { get; private set; }
    public float Base_Appeal_Range { get; private set; }
    public bool Can_Have_Minerals { get; private set; }
    public float Appeal { get; set; }
    public List<WorkData> Worked_By { get; private set; }
    public Dictionary<Mineral, float> Minerals { get; private set; }
    public List<Mineral> Mineral_Spawns { get; private set; }
    public List<Entity> Entities { get; private set; }
    public bool Is_Water { get { return Internal_Name.StartsWith("water"); } }
    public Coordinates.Direction? Water_Flow { get; set; }
    public bool Has_Ship_Access { get; set; }

    protected Color highlight_color;
    protected bool show_coordinates;
    protected GameObject text_game_object;
    protected TextMesh TextMesh { get { return text_game_object.GetComponent<TextMesh>(); } }
    protected Color? border_color;
    protected GameObject border_game_object;

    private float animation_cooldown;
    private int animation_index;

    public Tile(int x, int y, Tile prototype)
    {
        Id = current_id;
        current_id++;
        X = x;
        Y = y;
        Worked_By = new List<WorkData>();

        GameObject = GameObject.Instantiate(
            PrefabManager.Instance.Tile,
            new Vector3(X, Y, Map.Z_LEVEL),
            Quaternion.identity,
            Map.Instance.Tile_Container.transform
        );
        GameObject.name = string.Format("{0}{1}_#{2}", GAME_OBJECT_NAME_PREFIX, Coordinates.Parse_Text(true, false), Id);

        highlight_color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        Destroyed = false;
        show_coordinates = false;
        text_game_object = GameObject.GetComponentInChildren<TextMesh>().gameObject;
        text_game_object.SetActive(false);
        text_game_object.GetComponentInChildren<MeshRenderer>().sortingLayerName = "Text";
        Minerals = new Dictionary<Mineral, float>();
        Mineral_Spawns = new List<Mineral>();
        Entities = new List<Entity>();
        Has_Ship_Access = false;

        Change_To(prototype);
    }
    
    public Tile(string internal_name, string terrain, string sprite, bool buildable, float appeal, float appeal_range, bool can_have_minerals)
    {
        Id = -1;
        Internal_Name = internal_name;
        Terrain = terrain;
        Sprite = sprite;
        Buildable = buildable;
        Base_Appeal = appeal;
        Base_Appeal_Range = appeal_range;
        Can_Have_Minerals = can_have_minerals;
        Minerals = new Dictionary<Mineral, float>();
        Mineral_Spawns = new List<Mineral>();
        Animation_Sprites = new List<string>();
    }

    public Tile(string internal_name, string terrain, bool buildable, float appeal, float appeal_range, bool can_have_minerals, List<string> animation_sprites, bool sync_animation, float animation_framerate)
    {
        Id = -1;
        Internal_Name = internal_name;
        Terrain = terrain;
        Sprite = animation_sprites[0];
        Buildable = buildable;
        Base_Appeal = appeal;
        Base_Appeal_Range = appeal_range;
        Can_Have_Minerals = can_have_minerals;
        Minerals = new Dictionary<Mineral, float>();
        Mineral_Spawns = new List<Mineral>();
        Animation_Sprites = Helper.Clone_List(animation_sprites);
        Sync_Animation = sync_animation;
        Animation_Framerate = animation_framerate;
    }

    public void Change_To(Tile prototype)
    {
        Internal_Name = prototype.Internal_Name;
        Terrain = prototype.Terrain;
        Sprite = prototype.Sprite;
        Buildable = prototype.Buildable;
        float old_base_appeal = Base_Appeal;
        float old_base_appeal_range = Base_Appeal_Range;
        Base_Appeal = prototype.Base_Appeal;
        Base_Appeal_Range = prototype.Base_Appeal_Range;
        Can_Have_Minerals = prototype.Can_Have_Minerals;
        if (!Can_Have_Minerals) {
            Minerals.Clear();
        }
        Animation_Sprites = Helper.Clone_List(prototype.Animation_Sprites);
        Sync_Animation = prototype.Sync_Animation;
        Animation_Framerate = prototype.Animation_Framerate;
        SpriteRenderer.sprite = SpriteManager.Instance.Get(Sprite, SpriteManager.SpriteType.Terrain);
        animation_index = 0;
        if (Is_Animated && !Sync_Animation) {
            animation_cooldown = (1.0f / Animation_Framerate) * RNG.Instance.Next_F();
            animation_index = RNG.Instance.Next(0, Animation_Sprites.Count - 1);
        }
        if(Map.Instance.State == Map.MapState.Normal && Building == null) {
            if (old_base_appeal != 0.0f) {
                Appeal -= old_base_appeal;
                if (old_base_appeal_range != 0.0f) {
                    foreach (Tile affected in Map.Instance.Get_Tiles_In_Circle(Coordinates, old_base_appeal_range)) {
                        if (affected == this) {
                            continue;
                        }
                        affected.Appeal -= Tile.Calculate_Appeal_Effect(Coordinates, old_base_appeal, old_base_appeal_range, affected.Coordinates);
                    }
                }
            }
            if (Base_Appeal != 0.0f) {
                Appeal += Base_Appeal;
                if (Base_Appeal_Range != 0.0f) {
                    foreach (Tile affected in Map.Instance.Get_Tiles_In_Circle(Coordinates, Base_Appeal_Range)) {
                        if (affected == this) {
                            continue;
                        }
                        affected.Appeal += Tile.Calculate_Appeal_Effect(Coordinates, Base_Appeal, Base_Appeal_Range, affected.Coordinates);
                    }
                }
            }
        }
    }

    public void Update(float delta_time)
    {
        if(!Is_Animated) {
            return;
        }
        if (Sync_Animation) {
            SpriteRenderer.sprite = SpriteManager.Instance.Get(Animation_Sprites[TilePrototypes.Instance.Animation_Index(Internal_Name)], SpriteManager.SpriteType.Terrain);
            return;
        }
        animation_cooldown -= delta_time;
        if(animation_cooldown > 0.0f) {
            return;
        }
        animation_cooldown += (1.0f / Animation_Framerate);
        animation_index++;
        if(animation_index == Animation_Sprites.Count) {
            animation_index = 0;
        }
        SpriteRenderer.sprite = SpriteManager.Instance.Get(Animation_Sprites[animation_index], SpriteManager.SpriteType.Terrain);
    }

    public Coordinates Coordinates
    {
        get {
            return new global::Coordinates(X, Y);
        }
    }

    public PathfindingNode PathfindingNode
    {
        get {
            return new PathfindingNode(Coordinates, 1.0f);
        }
    }

    public PathfindingNode Road_PathfindingNode
    {
        get {
            return new PathfindingNode(Coordinates, Building == null || !Building.Is_Road || !Building.Is_Complete || Building.Is_Deconstructing ? float.MaxValue : 1.0f);
        }
    }

    public PathfindingNode Ship_PathfindingNode
    {
        get {
            return new PathfindingNode(Coordinates, Has_Ship_Access ? 1.0f : float.MaxValue);
        }
    }

    public bool Active
    {
        get {
            return GameObject.activeSelf;
        }
        set {
            GameObject.SetActive(value);
        }
    }

    public void Clear_Highlight()
    {
        highlight_color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        GameObject.GetComponent<SpriteRenderer>().color = highlight_color;
    }

    /// <summary>
    /// Color used to highlight this tile
    /// </summary>
    public Color Highlight
    {
        get {
            return highlight_color;
        }
        set {
            if (value != highlight_color) {
                highlight_color = value;
            } else {
                highlight_color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            }
            GameObject.GetComponent<SpriteRenderer>().color = highlight_color;
        }
    }

    public bool Show_Coordinates
    {
        get {
            if (!text_game_object.activeSelf) {
                return false;
            }
            return show_coordinates;
        }
        set {
            show_coordinates = value;
            if (!show_coordinates) {
                text_game_object.SetActive(false);
                return;
            }
            text_game_object.SetActive(true);
            TextMesh.text = Coordinates.Parse_Text(true, true);
        }
    }

    public void Show_Text(string text)
    {
        text_game_object.SetActive(true);
        TextMesh.text = text;
    }

    public void Hide_Text()
    {
        text_game_object.SetActive(false);
    }

    public void Add_Workers(Building building, Work_Type type)
    {
        if(!Worked_By.Exists(x => x.Building == building && x.Type == type)) {
            Worked_By.Add(new WorkData() { Building = building, Type = type });
        }
    }

    public bool Can_Work(Building building, Work_Type type)
    {
        if (!Worked_By.Exists(x => x.Building == building && x.Type == type)) {
            return false;
        }
        Building first = Worked_By.First(x => x.Type == type).Building;
        return first.Id == building.Id;
    }

    public void Remove_Workers(Building building)
    {
        if (Worked_By.Exists(x => x.Building == building)) {
            Worked_By = Worked_By.Where(x => x.Building != building).ToList();
        }
    }

    public TileSaveData Save_Data()
    {
        return new TileSaveData() {
            X = X,
            Y = Y,
            Internal_Name = Internal_Name,
            Worked_By = Worked_By.Select(x => new WorkSaveData() { Id = x.Building.Id, Type = (int)x.Type }).ToList(),
            Minerals = Minerals.Select(x => new MineralSaveData() { Mineral = (int)x.Key, Amount = x.Value }).ToList(),
            Mineral_Spawns = Mineral_Spawns.Select(x => (int)x).ToList(),
            Water_Flow = Water_Flow.HasValue ? (int)Water_Flow : -1
        };
    }

    public string Mineral_String()
    {
        if(Minerals.Count == 0) {
            return string.Empty;
        }
        StringBuilder builder = new StringBuilder();
        foreach(KeyValuePair<Mineral, float> mineral_data in Minerals) {
            builder.Append(mineral_data.Key.ToString().ToUpper()[0]).Append(mineral_data.Key.ToString().ToLower()[1]).Append(": ").Append(Helper.Float_To_String(mineral_data.Value, 1)).Append(", ");
        }
        return builder.Remove(builder.Length - 2, 2).ToString();
    }

    public override string ToString()
    {
        return Is_Prototype ? string.Format("{0}-Prototype", Internal_Name) : string.Format("{0}_{1}_#{2}", Internal_Name, Coordinates.Parse_Text(true, false), Id);
    }

    /// <summary>
    /// Deletes tile's GameObject
    /// </summary>
    public void Delete()
    {
        GameObject.Destroy(GameObject);
        Destroyed = true;
    }

    public static float Calculate_Appeal_Effect(Coordinates source, float appeal, float range, Coordinates target)
    {
        float distance = source.Distance(target);
        if(distance > range) {
            return 0.0f;
        }
        float distance_multiplier = Mathf.Max((1.0f - distance / range), 0.5f);
        return appeal * distance_multiplier;
    }

    public static Coordinates Parse_Coordinates_From_GameObject_Name(string name)
    {
        if(!name.StartsWith(GAME_OBJECT_NAME_PREFIX)) {
            return null;
        }
        string coordinate_part = name.Substring(GAME_OBJECT_NAME_PREFIX.Length + 3, name.IndexOf(")") - GAME_OBJECT_NAME_PREFIX.Length - 3);
        int x;
        if (!int.TryParse(coordinate_part.Substring(0, coordinate_part.IndexOf(",")), out x)) {
            return null;
        }
        int y;
        if(!int.TryParse(coordinate_part.Substring(coordinate_part.IndexOf(",") + 3), out y)) {
            return null;
        }
        return new Coordinates(x, y);
    }

    public class WorkData
    {
        public Work_Type Type { get; set; }
        public Building Building { get; set; }
    }
}
