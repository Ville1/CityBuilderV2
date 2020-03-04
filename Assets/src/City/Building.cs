using UnityEngine;

public class Building {
    private static long current_id = 0;

    public static readonly string TOWN_HALL_INTERNAL_NAME = "town_hall";
    public enum UI_Category { Admin, Infrastructure, Housing, Services, Forestry, Agriculture, Industry }
    public enum Resident { Peasant, Citizen, Noble }

    public long Id { get; private set; }
    public string Name { get; private set; }
    public string Internal_Name { get; private set; }
    public UI_Category Category { get; private set; }
    public string Sprite { get; private set; }
    public bool Is_Prototype { get { return Id < 0; } }
    public bool Is_Town_Hall { get { return Internal_Name == TOWN_HALL_INTERNAL_NAME; } }
    public int Width { get; private set; }
    public int Height { get; private set; }
    public Tile Tile { get; private set; }
    public bool Is_Preview { get; private set; }

    public GameObject GameObject { get; private set; }
    public SpriteRenderer Renderer { get { return GameObject != null ? GameObject.GetComponent<SpriteRenderer>() : null; } }

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
        Width = prototype.Width;
        Height = prototype.Height;
        Tile = tile;
        Tile.Building = this;
        Is_Preview = is_preview;

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

    public Building(string name, string internal_name, UI_Category category, string sprite, int width, int height)
    {
        Id = -1;
        Name = name;
        Internal_Name = internal_name;
        Category = category;
        Sprite = sprite;
        Width = width;
        Height = height;
        Is_Preview = false;
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

    public void Delete()
    {
        GameObject.Destroy(GameObject);
        if(Tile != null) {
            Tile.Building = null;
        }
    }

    private GameObject Get_Prefab()
    {
        if(Width == 2 && Height == 2) {
            return PrefabManager.Instance.Building_2x2;
        }
        CustomLogger.Instance.Error(string.Format("{0}x{1} prefab does not exist", Width, Height));
        return null;
    }

    private void Update_Sprite()
    {
        Renderer.sprite = SpriteManager.Instance.Get(Sprite, SpriteManager.SpriteType.Buildings, Is_Preview);
    }
}
