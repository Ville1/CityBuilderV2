using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Entity {
    private static long current_id = 0;

    private static readonly float BRIDGE_HEIGHT = 0.1f;

    public enum EntityType { Static, Road_Path, Ship }

    public long Id { get; private set; }
    public string Internal_Name { get; private set; }
    public EntityType Type { get; private set; }
    public List<SpriteData> Sprites { get; private set; }
    public SpriteData Sprite { get; private set; }
    public bool Is_Prototype { get { return Id < 0; } }
    public Tile Tile { get; private set; }
    public GameObject GameObject { get; private set; }
    public SpriteRenderer SpriteRenderer { get { return GameObject == null ? null : GameObject.GetComponent<SpriteRenderer>(); } }
    public Building Spawner { get; set; }
    public int Max_Duration { get; private set; }
    public int Min_Duration { get; private set; }
    public float Current_Duration { get; private set; }
    public bool Is_Animated { get { return Sprite.Is_Animated; } }
    public List<PathfindingNode> Path { get; private set; }
    public PathfindingNode Current_Node { get; private set; }
    public PathfindingNode Next_Node { get; private set; }
    public float Current_Movement { get; private set; }
    public int Path_Index { get; private set; }
    public float Speed { get; private set; }
    public bool Limited_Duration { get { return Min_Duration > 0 && Max_Duration > 0; } }

    private int animation_index;
    private float animations_cooldown;
    private List<PathOrder> orders;
    private float wait_time_left;
    private PathOrder current_order;

    public Entity(Entity prototype, Tile tile, Building spawner)
    {
        Id = current_id;
        current_id++;
        Tile = tile;
        Spawner = spawner;
        if(Spawner != null) {
            Spawner.Entities_Spawned.Add(this);
        }
        Tile.Entities.Add(this);
        Map.Instance.Entities.Add(this);

        Internal_Name = prototype.Internal_Name;
        Type = prototype.Type;
        Max_Duration = prototype.Max_Duration;
        Min_Duration = prototype.Min_Duration;
        Current_Duration = Limited_Duration ? Min_Duration + (RNG.Instance.Next_F() * (Max_Duration - Min_Duration)) : 0.0f;
        Sprites = new List<SpriteData>();
        foreach (SpriteData data in prototype.Sprites) {
            Sprites.Add(data.Clone());
        }
        orders = new List<PathOrder>();
        wait_time_left = 0.0f;
        current_order = null;

        GameObject = GameObject.Instantiate(
            PrefabManager.Instance.Entity,
            new Vector3(
                tile.GameObject.transform.position.x,
                tile.GameObject.transform.position.y,
                tile.GameObject.transform.position.z
            ),
            Quaternion.identity,
            Map.Instance.Entity_Container.transform
        );
        GameObject.name = ToString();
        SpriteRenderer.sortingLayerName = "Entities";

        Sprite = RNG.Instance.Item(Sprites);
        SpriteRenderer.sprite = SpriteManager.Instance.Get(Sprite.Name, Sprite.Type);

        if (Is_Animated) {
            animations_cooldown = Sprite.Animation_Frame_Time;
            animation_index = 0;
        }
        if(Type == EntityType.Ship && Sprites.Count != 4) {
            CustomLogger.Instance.Error("Ship has invalid sprite count");
        }
    }

    public Entity(string name, EntityType type, List<SpriteData> sprites, int min_duration, int max_duration)
    {
        Id = -1;
        Internal_Name = name;
        Type = type;
        Min_Duration = min_duration;
        Max_Duration = max_duration;
        Sprites = new List<SpriteData>();
        foreach(SpriteData data in sprites) {
            Sprites.Add(data.Clone());
        }
        orders = new List<PathOrder>();
        wait_time_left = 0.0f;
        current_order = null;
    }

    public void Update(float delta_time)
    {
        if (Limited_Duration) {
            Current_Duration -= TimeManager.Instance.Multiplier * delta_time;
            if (Current_Duration <= 0.0f) {
                Map.Instance.Delete_Entity(this);
            }
        }
        if (Is_Animated) {
            animations_cooldown -= delta_time;
            if(animations_cooldown <= 0.0f) {
                animation_index++;
                if(animation_index == Sprite.Animation_Sprites.Count) {
                    animation_index = 0;
                }
                SpriteRenderer.sprite = SpriteManager.Instance.Get(Sprite.Animation_Sprites[animation_index], Sprite.Type);
                animations_cooldown += Sprite.Animation_Frame_Time;
            }
        }
        if(wait_time_left > 0.0f) {
            wait_time_left -= delta_time * TimeManager.Instance.Multiplier;
            return;
        }
        if (Type == EntityType.Road_Path && !TimeManager.Instance.Paused) {
            if (Path == null || Path.Count == 0) {
                Map.Instance.Delete_Entity(this);
                CustomLogger.Instance.Error("Missing path");
                return;
            }

            if (Tile.Building == null || !Tile.Building.Is_Road) {
                Map.Instance.Delete_Entity(this);
                return;
            }

            Current_Movement = Mathf.Clamp01(Current_Movement + (delta_time * Speed * TimeManager.Instance.Multiplier));
            if (Current_Movement == 1.0f) {
                if (Path_Index == Path.Count - 2) {
                    Map.Instance.Delete_Entity(this);
                    return;
                }
                Tile = Map.Instance.Get_Tile_At(Next_Node.Coordinates);
                GameObject.transform.position = new Vector3(Tile.GameObject.transform.position.x, Tile.GameObject.transform.position.y + (Tile.Building.Tags.Contains(Building.Tag.Bridge) ? BRIDGE_HEIGHT : 0.0f),
                    Tile.GameObject.transform.position.z);
                Path_Index++;
                Current_Node = Path[Path_Index];
                Next_Node = Path[Path_Index + 1];
                Current_Movement = 0.0f;
            } else {
                Vector3 current = new Vector3(
                    Tile.GameObject.transform.position.x,
                    Tile.GameObject.transform.position.y + (Tile.Building.Tags.Contains(Building.Tag.Bridge) ? BRIDGE_HEIGHT : 0.0f),
                    Tile.GameObject.transform.position.z
                );
                Tile next_tile = Map.Instance.Get_Tile_At(Next_Node.Coordinates);
                Vector3 next = new Vector3(
                    next_tile.GameObject.transform.position.x,
                    next_tile.GameObject.transform.position.y + (next_tile.Building.Tags.Contains(Building.Tag.Bridge) ? BRIDGE_HEIGHT : 0.0f),
                    next_tile.GameObject.transform.position.z
                );
                GameObject.transform.position = Vector3.Lerp(
                    current,
                    next,
                    Current_Movement
                );
            }
        } else if (Type == EntityType.Ship && !TimeManager.Instance.Paused) {
            if (Path == null || Path.Count == 0) {
                Map.Instance.Delete_Entity(this);
                CustomLogger.Instance.Error("Missing path");
                return;
            }

            if (Tile.Building != null || !Tile.Has_Ship_Access) {
                Map.Instance.Delete_Entity(this);
                return;
            }

            Current_Movement = Mathf.Clamp01(Current_Movement + (delta_time * Speed * TimeManager.Instance.Multiplier));
            if (Current_Movement == 1.0f) {
                if (Path_Index == Path.Count - 2) {
                    Map.Instance.Delete_Entity(this);
                    return;
                }
                Tile = Map.Instance.Get_Tile_At(Next_Node.Coordinates);
                GameObject.transform.position = new Vector3(Tile.GameObject.transform.position.x, Tile.GameObject.transform.position.y, Tile.GameObject.transform.position.z);
                Path_Index++;
                Current_Node = Path[Path_Index];
                Next_Node = Path[Path_Index + 1];
                Current_Movement = 0.0f;
            } else {
                Vector3 current = new Vector3(
                    Tile.GameObject.transform.position.x,
                    Tile.GameObject.transform.position.y,
                    Tile.GameObject.transform.position.z
                );
                Tile next_tile = Map.Instance.Get_Tile_At(Next_Node.Coordinates);
                Vector3 next = new Vector3(
                    next_tile.GameObject.transform.position.x,
                    next_tile.GameObject.transform.position.y,
                    next_tile.GameObject.transform.position.z
                );
                GameObject.transform.position = Vector3.Lerp(
                    current,
                    next,
                    Current_Movement
                );
                bool orientation_change = false;
                if(next.x > current.x && Sprite != Sprites[1]) {
                    //Facing east
                    Sprite = Sprites[1];
                    orientation_change = true;
                } else if(next.x < current.x && Sprite != Sprites[3]) {
                    //Facing west
                    Sprite = Sprites[3];
                    orientation_change = true;
                } else if(next.y > current.y && Sprite != Sprites[0]) {
                    //Facing north
                    Sprite = Sprites[0];
                    orientation_change = true;
                } else if(next.y < current.y && Sprite != Sprites[2]) {
                    //Facign south
                    Sprite = Sprites[2];
                    orientation_change = true;
                }
                if (orientation_change) {
                    SpriteRenderer.sprite = SpriteManager.Instance.Get(Sprite.Name, Sprite.Type);
                }
                PathOrder order = orders.FirstOrDefault(x => x.Node.Coordinates.Equals(Tile.Coordinates));
                if(order != null && current_order != order) {
                    wait_time_left = order.Wait_Time;
                    current_order = order;
                }
            }
        }
    }

    public void Set_Path(List<PathfindingNode> path, float speed)
    {
        if(Type == EntityType.Static) {
            CustomLogger.Instance.Error("Trying to set path for static Entity");
            return;
        }
        if(path.Count < 2 || !path[0].Coordinates.Equals(Tile.Coordinates)) {
            CustomLogger.Instance.Error("Invalid path");
            return;
        }
        Speed = speed;
        Path = path;
        Current_Node = Path[0];
        Next_Node = Path[1];
        Current_Movement = 0.0f;
        Path_Index = 0;
    }

    public void Add_Order(PathOrder order)
    {
        if(orders.Exists(x => x.Node.Coordinates.Equals(order.Node.Coordinates))) {
            CustomLogger.Instance.Warning(string.Format("Node {0} already has orders defined", order.Node.Coordinates.ToString()));
            return;
        }
        orders.Add(order);
    }

    public void Delete(bool map_delete = false)
    {
        if (!map_delete) {
            Map.Instance.Entities.Remove(this);
            Tile.Entities.Remove(this);
            if (Spawner != null) {
                Spawner.Entities_Spawned.Remove(this);
            }
            if (City.Instance.Walkers.Contains(this)) {
                City.Instance.Walkers.Remove(this);
            }
            if (Type == EntityType.Ship) {
                City.Instance.Delete_Ship(this);
            }
        }
        GameObject.Destroy(GameObject);
    }

    public override string ToString()
    {
        return Is_Prototype ? string.Format("{0}_prototype", Internal_Name) : string.Format("{0}_#{1}", Internal_Name, Id);
    }

    public class PathOrder
    {
        public PathfindingNode Node { get; set; }
        public float Wait_Time { get; set; }

        public PathOrder(PathfindingNode node, float wait_time)
        {
            Node = node;
            Wait_Time = wait_time;
        }
    }
}
