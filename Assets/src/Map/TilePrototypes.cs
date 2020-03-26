using System.Collections.Generic;
using System.Linq;

public class TilePrototypes {
    private static TilePrototypes instance;

    private List<Tile> prototypes;
    private Dictionary<Tile, int> animation_indices;
    private Dictionary<Tile, float> animation_cooldowns;

    private TilePrototypes()
    {
        prototypes = new List<Tile>();

        prototypes.Add(new Tile("placeholder", "Placeholder", "terrain_placeholder", false, 0.0f, 0.0f, true));
        prototypes.Add(new Tile("grass", "Grass", "grass", true, 0.0f, 0.0f, true));
        prototypes.Add(new Tile("fertile_ground", "Fertile ground", "fertile_ground", true, 1.0f, 1.0f, true));
        prototypes.Add(new Tile("sparse_forest", "Sparse forest", "sparse_forest", false, 1.0f, 1.5f, true));
        prototypes.Add(new Tile("forest", "Forest", "forest", false, 1.0f, 2.0f, true));

        prototypes.Add(new Tile("hill_1", "Hill", "hill_1", false, 0.05f, 4.0f, true));
        prototypes.Add(new Tile("hill_2", "Hill", "hill_2", false, 0.05f, 4.0f, true));
        prototypes.Add(new Tile("hill_3", "Hill", "hill_3", false, 0.05f, 4.0f, true));
        prototypes.Add(new Tile("hill_4", "Hill", "hill_4", false, 0.05f, 4.0f, true));
        prototypes.Add(new Tile("hill_5", "Hill", "hill_5", false, 0.05f, 4.0f, true));
        prototypes.Add(new Tile("hill_6", "Hill", "hill_6", false, 0.05f, 4.0f, true));
        prototypes.Add(new Tile("hill_7", "Hill", "hill_7", false, 0.05f, 4.0f, true));
        prototypes.Add(new Tile("hill_8", "Hill", "hill_8", false, 0.05f, 4.0f, true));
        prototypes.Add(new Tile("hill_9", "Hill", "hill_9", false, 0.05f, 4.0f, true));

        float water_appeal = 0.01f;
        float water_appeal_range = 6.0f;
        float water_framerate = 2.0f;
        prototypes.Add(new Tile("water_nesw", "Water", false, water_appeal, water_appeal_range, false, new List<string>() { "water_nesw_1", "water_nesw_2", "water_nesw_3" }, true, water_framerate));
        prototypes.Add(new Tile("water_es", "Water", false, water_appeal, water_appeal_range, false, new List<string>() { "water_es_1", "water_es_2", "water_es_3" }, true, water_framerate));
        prototypes.Add(new Tile("water_sw", "Water", false, water_appeal, water_appeal_range, false, new List<string>() { "water_sw_1", "water_sw_2", "water_sw_3" }, true, water_framerate));
        prototypes.Add(new Tile("water_nw", "Water", false, water_appeal, water_appeal_range, false, new List<string>() { "water_nw_1", "water_nw_2", "water_nw_3" }, true, water_framerate));
        prototypes.Add(new Tile("water_ne", "Water", false, water_appeal, water_appeal_range, false, new List<string>() { "water_ne_1", "water_ne_2", "water_ne_3" }, true, water_framerate));
        prototypes.Add(new Tile("water_esw", "Water", false, water_appeal, water_appeal_range, false, new List<string>() { "water_esw_1", "water_esw_2", "water_esw_3" }, true, water_framerate));
        prototypes.Add(new Tile("water_nes", "Water", false, water_appeal, water_appeal_range, false, new List<string>() { "water_nes_1", "water_nes_2", "water_nes_3" }, true, water_framerate));
        prototypes.Add(new Tile("water_new", "Water", false, water_appeal, water_appeal_range, false, new List<string>() { "water_new_1", "water_new_2", "water_new_3" }, true, water_framerate));
        prototypes.Add(new Tile("water_nsw", "Water", false, water_appeal, water_appeal_range, false, new List<string>() { "water_nsw_1", "water_nsw_2", "water_nsw_3" }, true, water_framerate));
        prototypes.Add(new Tile("water_n", "Water", false, water_appeal, water_appeal_range, false, new List<string>() { "water_n_1", "water_n_2", "water_n_3" }, true, water_framerate));
        prototypes.Add(new Tile("water_e", "Water", false, water_appeal, water_appeal_range, false, new List<string>() { "water_e_1", "water_e_2", "water_e_3" }, true, water_framerate));
        prototypes.Add(new Tile("water_s", "Water", false, water_appeal, water_appeal_range, false, new List<string>() { "water_s_1", "water_s_2", "water_s_3" }, true, water_framerate));
        prototypes.Add(new Tile("water_w", "Water", false, water_appeal, water_appeal_range, false, new List<string>() { "water_w_1", "water_w_2", "water_w_3" }, true, water_framerate));
        prototypes.Add(new Tile("water_", "Water", false, water_appeal, water_appeal_range, false, new List<string>() { "water_1", "water_2", "water_3" }, true, water_framerate));

        animation_indices = new Dictionary<Tile, int>();
        animation_cooldowns = new Dictionary<Tile, float>();
        foreach(Tile prototype in prototypes) {
            if (prototype.Sync_Animation) {
                animation_indices.Add(prototype, 0);
                animation_cooldowns.Add(prototype, 1.0f / prototype.Animation_Framerate);
            }
        }
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

    public bool Exists(string internal_name)
    {
        return prototypes.FirstOrDefault(x => x.Internal_Name == internal_name) != null;
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

    public void Update(float delta_time)
    {
        List<Tile> next_frames = new List<Tile>();
        foreach(KeyValuePair<Tile, int> pair in animation_indices) {
            animation_cooldowns[pair.Key] -= delta_time;
            if(animation_cooldowns[pair.Key] <= 0.0f) {
                animation_cooldowns[pair.Key] += 1.0f / pair.Key.Animation_Framerate;
                next_frames.Add(pair.Key);
            }
        }
        foreach(Tile prototype in next_frames) {
            animation_indices[prototype]++;
            if(animation_indices[prototype] == prototype.Animation_Sprites.Count) {
                animation_indices[prototype] = 0;
            }
        }
    }

    public int Animation_Index(string internal_name)
    {
        Tile prototype = animation_indices.Select(x => x.Key).FirstOrDefault(x => x.Internal_Name == internal_name);
        if (prototype == null) {
            CustomLogger.Instance.Error(string.Format("{0} does not have synced animation", internal_name));
            return 0;
        }
        return animation_indices[prototype];
    }
}
