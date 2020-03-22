using System.Collections.Generic;
using UnityEngine;

public class Entity {
    private static long current_id = 0;

    public enum EntityType { Static }

    public long Id { get; private set; }
    public string Internal_Name { get; private set; }
    public EntityType Type { get; private set; }
    public List<SpriteData> Sprites { get; private set; }
    public SpriteData Sprite { get { return Sprites != null && Sprites.Count > 0 ? Sprites[0] : null; } }
    public bool Is_Prototype { get { return Id < 0; } }
    public Tile Tile { get; private set; }
    public GameObject GameObject { get; private set; }
    public SpriteRenderer SpriteRenderer { get { return GameObject == null ? null : GameObject.GetComponent<SpriteRenderer>(); } }
    public Building Spawner { get; set; }
    public int Max_Duration { get; private set; }
    public int Min_Duration { get; private set; }
    public float Current_Duration { get; private set; }

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
        Current_Duration = Min_Duration + (RNG.Instance.Next_F() * (Max_Duration - Min_Duration));

        Sprites = new List<SpriteData>();
        foreach (SpriteData data in prototype.Sprites) {
            Sprites.Add(data.Clone());
        }
        
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

        SpriteData sprite = RNG.Instance.Item(Sprites);
        SpriteRenderer.sprite = SpriteManager.Instance.Get(sprite.Name, sprite.Type);
    }

    public Entity(string name, EntityType type, List<SpriteData> sprites, int min_duartion, int max_duration)
    {
        Id = -1;
        Internal_Name = name;
        Type = type;
        Min_Duration = min_duartion;
        Max_Duration = max_duration;
        Sprites = new List<SpriteData>();
        foreach(SpriteData data in sprites) {
            Sprites.Add(data.Clone());
        }
    }

    public void Update(float delta_time)
    {
        Current_Duration -= TimeManager.Instance.Multiplier * delta_time;
        if(Current_Duration <= 0.0f) {
            Map.Instance.Delete_Entity(this);
        }
    }

    public void Delete(bool map_delete = false)
    {
        if (!map_delete) {
            Map.Instance.Entities.Remove(this);
            Tile.Entities.Remove(this);
            if (Spawner != null) {
                Spawner.Entities_Spawned.Remove(this);
            }
        }
        GameObject.Destroy(GameObject);
    }

    public override string ToString()
    {
        return Is_Prototype ? string.Format("{0}_prototype", Internal_Name) : string.Format("{0}_#{1}", Internal_Name, Id);
    }
}
