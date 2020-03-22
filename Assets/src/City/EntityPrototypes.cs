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
