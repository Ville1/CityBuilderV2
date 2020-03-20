using System.Collections.Generic;
using System.Linq;

public class TilePrototypes {
    private static TilePrototypes instance;

    private List<Tile> prototypes;

    private TilePrototypes()
    {
        prototypes = new List<Tile>();

        prototypes.Add(new Tile("grass", "Grass", "grass", true, 0.0f, 0.0f));
        prototypes.Add(new Tile("fertile_ground", "Fertile ground", "fertile_ground", true, 1.0f, 1.0f));
        prototypes.Add(new Tile("sparse_forest", "Sparse forest", "sparse_forest", false, 2.0f, 1.5f));
        prototypes.Add(new Tile("forest", "Forest", "forest", false, 3.0f, 2.0f));
    }

    public static TilePrototypes Instance
    {
        get {
            if (instance == null) {
                instance = new TilePrototypes();
            }
            return instance;
        }
    }

    public Tile Get(string internal_name)
    {
        Tile tile = prototypes.FirstOrDefault(x => x.Internal_Name == internal_name);
        if(tile == null) {
            CustomLogger.Instance.Error(string.Format("Tile not found: {0}", internal_name));
            return null;
        }
        return tile;
    }
}
