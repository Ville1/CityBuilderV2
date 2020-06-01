using System.Collections.Generic;
using System.Linq;

public class EntityPrototypes {
    private static EntityPrototypes instance;

    private List<Entity> prototypes;

    private EntityPrototypes()
    {
        prototypes = new List<Entity>();
        prototypes.Add(new Entity("sheep", Entity.EntityType.Static, new List<SpriteData>() { new SpriteData("sheep_1", SpriteManager.SpriteType.Entity), new SpriteData("sheep_2", SpriteManager.SpriteType.Entity) },
            10, 60));
        prototypes.Add(new Entity("fishing_boat", Entity.EntityType.Static, new List<SpriteData> {
            new SpriteData(SpriteManager.SpriteType.Entity, new List<string>() { "fishing_boat_1", "fishing_boat_2", "fishing_boat_3" }, 3.0f),
            new SpriteData(SpriteManager.SpriteType.Entity, new List<string>() { "fishing_boat_b_1", "fishing_boat_b_2", "fishing_boat_b_3" }, 3.0f)
        }, 30, 120));
        prototypes.Add(new Entity("walker", Entity.EntityType.Road_Path, new List<SpriteData> {
            new SpriteData(SpriteManager.SpriteType.Entity, new List<string>() { "walker_a_1", "walker_a_2" }, 2.0f),
            new SpriteData(SpriteManager.SpriteType.Entity, new List<string>() { "walker_b_1", "walker_b_2" }, 2.0f),
            new SpriteData(SpriteManager.SpriteType.Entity, new List<string>() { "walker_c_1", "walker_c_2" }, 2.0f)
        }, -1, -1));
        prototypes.Add(new Entity("ship", Entity.EntityType.Ship, new List<SpriteData> {
            new SpriteData(SpriteManager.SpriteType.Entity, new List<string>() { "ship_n_1", "ship_n_2", "ship_n_3" }, 2.0f),
            new SpriteData(SpriteManager.SpriteType.Entity, new List<string>() { "ship_e_1", "ship_e_2", "ship_e_3" }, 2.0f),
            new SpriteData(SpriteManager.SpriteType.Entity, new List<string>() { "ship_s_1", "ship_s_2", "ship_s_3" }, 2.0f),
            new SpriteData(SpriteManager.SpriteType.Entity, new List<string>() { "ship_w_1", "ship_w_2", "ship_w_3" }, 2.0f)
        }, -1, -1));
    }

    public static EntityPrototypes Instance
    {
        get {
            if (instance == null) {
                instance = new EntityPrototypes();
            }
            return instance;
        }
    }

    public Entity Get(string internal_name)
    {
        Entity entity = prototypes.FirstOrDefault(x => x.Internal_Name == internal_name);
        if (entity == null) {
            CustomLogger.Instance.Error(string.Format("Entity not found: {0}", internal_name));
            return null;
        }
        return entity;
    }
}
