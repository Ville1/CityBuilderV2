using UnityEngine;

public class Tile
{
    public static readonly string GAME_OBJECT_NAME_PREFIX = "Tile_";
    public enum Fog_Of_War_Status { Visible, Not_Visible, Not_Explored }

    private static long current_id = 0;
    
    public long Id { get; private set; }
    public string Internal_Name { get; private set; }
    public string Terrain { get; private set; }
    public string Sprite { get; private set; }

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
    public float Appeal { get { return Base_Appeal; } }

    protected Color highlight_color;
    protected bool show_coordinates;
    protected GameObject text_game_object;
    protected TextMesh TextMesh { get { return text_game_object.GetComponent<TextMesh>(); } }
    protected Color? border_color;
    protected GameObject border_game_object;

    public Tile(int x, int y, Tile prototype)
    {
        Id = current_id;
        current_id++;
        X = x;
        Y = y;
        
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

        Change_To(prototype);
    }
    
    public Tile(string internal_name, string terrain, string sprite, bool buildable, float appeal, float appeal_range)
    {
        Id = -1;
        Internal_Name = internal_name;
        Terrain = terrain;
        Sprite = sprite;
        Buildable = buildable;
        Base_Appeal = appeal;
        Base_Appeal_Range = appeal_range;
    }
    
    public void Change_To(Tile prototype)
    {
        Internal_Name = prototype.Internal_Name;
        Terrain = prototype.Terrain;
        Sprite = prototype.Sprite;
        Buildable = prototype.Buildable;
        Base_Appeal = prototype.Base_Appeal;
        Base_Appeal_Range = prototype.Base_Appeal_Range;
        SpriteRenderer.sprite = SpriteManager.Instance.Get(Sprite, SpriteManager.SpriteType.Terrain);
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
}
