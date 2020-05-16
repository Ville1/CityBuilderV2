using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public enum Mineral { Iron, Coal, Salt, Clay, Marble, Copper, Tin, Silver, Gold, Gems }
public class Map : MonoBehaviour
{
    public enum MapState { Inactive, Normal, Generating, Loading, Saving }
    public enum MapView { None, Appeal, Minerals, Water_Flow, Ship_Access }

    public static readonly float Z_LEVEL = 0.0f;
    public static readonly Dictionary<Mineral, int> MINERAL_BASE_SPAWN_CHANCE = new Dictionary<Mineral, int>() {
        { Mineral.Iron, 8 },
        { Mineral.Coal, 6 },
        { Mineral.Salt, 5 },
        { Mineral.Clay, 8 },
        { Mineral.Marble, 4 },
        { Mineral.Copper, 7 },
        { Mineral.Tin, 7 },
        { Mineral.Silver, 4 },
        { Mineral.Gold, 2 },
        { Mineral.Gems, 1 }
    };
    public static readonly Dictionary<Mineral, float> MINERAL_VEIN_SIZE = new Dictionary<Mineral, float>() {
        { Mineral.Iron, 3.0f },
        { Mineral.Coal, 3.0f },
        { Mineral.Salt, 2.0f },
        { Mineral.Clay, 4.0f },
        { Mineral.Marble, 2.0f },
        { Mineral.Copper, 3.0f },
        { Mineral.Tin, 3.0f },
        { Mineral.Silver, 1.5f },
        { Mineral.Gold, 1.5f },
        { Mineral.Gems, 0.5f }
    };
    public static readonly Dictionary<Mineral, float> MINERAL_VEIN_RICHNESS = new Dictionary<Mineral, float>() {
        { Mineral.Iron, 0.90f },
        { Mineral.Coal, 0.65f },
        { Mineral.Salt, 0.65f },
        { Mineral.Clay, 0.75f },
        { Mineral.Marble, 0.65f },
        { Mineral.Copper, 0.75f },
        { Mineral.Tin, 0.75f },
        { Mineral.Silver, 0.65f },
        { Mineral.Gold, 0.60f },
        { Mineral.Gems, 0.50f }
    };
    public static readonly Dictionary<Mineral, string> MINERAL_SPRITES = new Dictionary<Mineral, string>() {
        { Mineral.Iron, "iron_ore" },
        { Mineral.Coal, "coal" },
        { Mineral.Salt, "salt" },
        { Mineral.Clay, "clay" },
        { Mineral.Marble, "placeholder" },
        { Mineral.Copper, "copper_ore" },
        { Mineral.Tin, "tin_ore" },
        { Mineral.Silver, "silver_ore" },
        { Mineral.Gold, "gold_ore" },
        { Mineral.Gems, "gems" }
    };
    public static readonly SpriteManager.SpriteType MINERAL_SPRITE_TYPE = SpriteManager.SpriteType.UI;
    public static readonly List<Mineral> IMPORTANT_MINERALS = new List<Mineral>() { Mineral.Iron, Mineral.Clay, Mineral.Copper, Mineral.Tin };

    public static Map Instance { get; private set; }
    
    public GameObject Tile_Container;
    public GameObject Building_Container;
    public GameObject Entity_Container;

    public int Width { get; private set; }
    public int Height { get; private set; }
    public MapState State { get; private set; }
    public List<Entity> Entities { get; private set; }
    public bool Hide_Alerts { get; set; }
    public List<Tile> Ship_Spawns { get; private set; }

    private float forest_count_setting;
    private float forest_size_setting;
    private float forest_density_setting;
    private float hills_setting;
    private float lake_count_setting;
    private float lake_size_setting;
    private float river_setting;

    private List<List<Tile>> tiles;
    private int generation_loop;
    private int generation_index_x;
    private int generation_index_y;
    private int save_load_loop;
    private int save_index_x;
    private int save_index_y;
    private int loop_progress;
    private List<Tile> fine_tuned_tiles;
    private List<Tile> lake_spawns;
    private bool rivers_generated;
    private bool city_loaded;
    private List<Mineral> minerals_spawned;
    private bool mineral_safety_spawn;
    private MapView view;
    private List<Entity> entities_to_be_deleted;
    private List<GameObject> mineral_icons;

    /// <summary>
    /// Initializiation
    /// </summary>
    private void Start()
    {
        if (Instance != null) {
            CustomLogger.Instance.Error(LogMessages.MULTIPLE_INSTANCES);
            return;
        }
        Instance = this;
        State = MapState.Inactive;
        mineral_icons = new List<GameObject>();
    }

    /// <summary>
    /// Per frame update
    /// </summary>
    private void Update()
    {
        if (State == MapState.Generating) {
            switch (generation_loop) {
                case 1:
                    Generation_Loop_1();
                    break;
                case 2:
                    Generation_Loop_2();
                    break;
                case 3:
                    Generation_Loop_3();
                    break;
                case 4:
                    Generation_Loop_4();
                    break;
            }
            return;
        } else if (State == MapState.Saving) {
            switch (save_load_loop) {
                case 1:
                    Save_Loop_1();
                    break;
                case 2:
                    Save_Loop_2();
                    break;
            }
            return;
        } else if (State == MapState.Loading) {
            switch (save_load_loop) {
                case 1:
                    Load_Loop_1();
                    break;
                case 2:
                    Load_Loop_2();
                    break;
            }
            return;
        } else if (State == MapState.Normal) {
            City.Instance.Update(Time.deltaTime);
            TilePrototypes.Instance.Update(Time.deltaTime);
            Contacts.Instance.Update(Time.deltaTime);
            for (int x = 0; x < Width; x++) {
                for (int y = 0; y < Height; y++) {
                    tiles[x][y].Update(Time.deltaTime);
                    if (View == MapView.Appeal) {
                        tiles[x][y].Show_Text(Helper.Float_To_String(tiles[x][y].Appeal, 1));
                    } else if(View == MapView.Minerals) {
                        tiles[x][y].Show_Text(tiles[x][y].Mineral_String());
                    } else if (View == MapView.Water_Flow) {
                        if (tiles[x][y].Is_Water) {
                            if (tiles[x][y].Water_Flow.HasValue) {
                                tiles[x][y].Show_Text(Helper.Abreviation(tiles[x][y].Water_Flow.Value));
                            } else {
                                tiles[x][y].Show_Text("None");
                            }
                        }
                    } else if (View == MapView.Ship_Access && tiles[x][y].Is_Water && tiles[x][y].Building == null) {
                        if (Ship_Spawns.Contains(tiles[x][y])) {
                            tiles[x][y].Show_Text("Spawn");
                        } else if (tiles[x][y].Has_Ship_Access) {
                            tiles[x][y].Show_Text("Yes");
                        } else {
                            tiles[x][y].Show_Text("No");
                        }
                    }
                }
            }
            foreach(Entity entity in entities_to_be_deleted) {
                entity.Delete();
            }
            entities_to_be_deleted.Clear();
            foreach(Entity entity in Entities) {
                entity.Update(Time.deltaTime);
            }
        }
    }
    
    public void Start_Generation(int width, int height, float forest_count, float forest_size, float forest_density, float hills, float lake_count, float lake_size, float rivers)
    {
        Delete();
        City.Instance.Delete();
        TimeManager.Instance.Paused = true;
        TimeManager.Instance.Reset_Time();
        NotificationManager.Instance.Clear_Notifications();
        Active = false;
        State = MapState.Generating;
        View = MapView.None;
        Entities = new List<Entity>();
        entities_to_be_deleted = new List<Entity>();
        Width = width;
        Height = height;
        forest_count_setting = Mathf.Clamp01(forest_count);
        forest_size_setting = Mathf.Clamp01(forest_size);
        forest_density_setting = Mathf.Clamp01(forest_density);
        hills_setting = Mathf.Clamp01(hills);
        lake_count_setting = Mathf.Clamp01(lake_count);
        lake_size_setting = Mathf.Clamp01(lake_size);
        river_setting = Mathf.Clamp01(rivers);

        generation_loop = 1;
        generation_index_x = 0;
        generation_index_y = 0;
        loop_progress = 0;
        fine_tuned_tiles = new List<Tile>();
        lake_spawns = new List<Tile>();
        rivers_generated = false;
        minerals_spawned = new List<Mineral>();
        mineral_safety_spawn = false;
        Ship_Spawns = new List<Tile>();

        tiles = new List<List<Tile>>();
        for(int x = 0; x < width; x++) {
            tiles.Add(new List<Tile>());
            for(int y = 0; y < height; y++) {
                tiles[x].Add(null);
            }
        }

        ProgressBarManager.Instance.Active = true;
        Update_Progress();
    }

    private void Generation_Loop_1()
    {
        Tile tile = new Tile(generation_index_x, generation_index_y, TilePrototypes.Instance.Get(RNG.Instance.Next(0, 100) < (100 - Mathf.RoundToInt(2.0f * forest_count_setting)) ? "grass" : "forest"));
        if(lake_count_setting > 0.0f && RNG.Instance.Next(0, 1000) < Mathf.RoundToInt(lake_count_setting * 6.0f)) {
            tile.Change_To(TilePrototypes.Instance.Get("water_nesw"));
            lake_spawns.Add(tile);
        }
        tiles[generation_index_x][generation_index_y] = tile;

        int hill_chance = (int)(hills_setting * 2.0f);
        if(hill_chance > 0 && RNG.Instance.Next(0, 100) < hill_chance) {
            tile.Change_To(TilePrototypes.Instance.Get("hill_7"));
        }

        loop_progress++;
        generation_index_x++;
        if(generation_index_x == Width) {
            generation_index_x = 0;
            generation_index_y++;
        }
        if(generation_index_y == Height) {
            generation_index_x = 0;
            generation_index_y = 0;
            loop_progress = 0;
            generation_loop++;
        }
        Update_Progress();
    }

    private void Generation_Loop_2()
    {
        Tile tile = tiles[generation_index_x][generation_index_y];
        if (!rivers_generated) {
            rivers_generated = true;
            //Rivers
            int river_count = Mathf.RoundToInt((river_setting * Mathf.Min(Width, Height) * 0.075f) + (-1.0f + (RNG.Instance.Next_F() * (river_setting * 4.0f))));
            for (int i = 0; i < river_count; i++) {
                Coordinates.Direction starting_side = RNG.Instance.Item(Coordinates.Directly_Adjacent_Directions);
                Tile starting_tile = null;
                bool ship_spawn_created = false;
                int length = Mathf.RoundToInt((RNG.Instance.Next_F() + 0.5f) * Mathf.Sqrt(Mathf.Pow(Width, 2) + Mathf.Pow(Height, 2)));
                //TODO:? If river does not end at the adge of the map it will be flowing in the wrong direction. Unless there is suppoused to be a cave instead of a spring
                switch (starting_side) {
                    case Coordinates.Direction.North:
                        starting_tile = tiles[RNG.Instance.Next(0, Width - 1)][Height - 1];
                        break;
                    case Coordinates.Direction.East:
                        starting_tile = tiles[Width - 1][RNG.Instance.Next(0, Height - 1)];
                        break;
                    case Coordinates.Direction.South:
                        starting_tile = tiles[RNG.Instance.Next(0, Width - 1)][0];
                        break;
                    case Coordinates.Direction.West:
                        starting_tile = tiles[0][RNG.Instance.Next(0, Height - 1)];
                        break;
                }
                Tile next_tile = starting_tile;
                Coordinates.Direction direction = Helper.Rotate(starting_side, 4);
                Coordinates.Direction last_direction = direction;
                int current_length = 0;
                while (next_tile != null && current_length < length) {
                    next_tile.Change_To(TilePrototypes.Instance.Get("water_nesw"));
                    next_tile.Water_Flow = direction;
                    last_direction = direction;
                    if (Is_Edge_Tile(next_tile) && !ship_spawn_created) {
                        Ship_Spawns.Add(next_tile);
                        next_tile.Has_Ship_Access = true;
                        ship_spawn_created = true;
                    }
                    if (RNG.Instance.Next(0, 100) <= 60) {
                        direction = Helper.Rotate(direction, -1 + RNG.Instance.Next(0, 2));
                    }
                    foreach(KeyValuePair<Coordinates.Direction, Tile> pair in Get_Adjanced_Tiles(next_tile, true)) {
                        if(pair.Key != direction && !pair.Value.Is_Water && RNG.Instance.Next(0, 100) < 60 + Mathf.RoundToInt(35.0f * river_setting)) {
                            pair.Value.Change_To(TilePrototypes.Instance.Get("water_nesw"));
                            pair.Value.Water_Flow = last_direction;
                        }
                    }
                    next_tile = Get_Tile_At(next_tile.Coordinates, direction);
                    current_length++;
                }
            }
        }

        //Spread
        int spread_range = 2 + Mathf.RoundToInt(forest_size_setting * 2.0f);
        int water_range = 2 + Mathf.RoundToInt(lake_size_setting * 2.0f);
        int spread_chance_base = 50 + Mathf.RoundToInt(forest_density_setting * 35.0f);
        float fertility = (forest_count_setting + forest_density_setting + forest_size_setting + lake_count_setting + lake_size_setting + river_setting + 6.0f) / 12.0f;
        int spread_range_fertility = 2 + Mathf.RoundToInt(fertility * 2.0f);
        if (tile.Internal_Name == "hill_7") {
            List<Tile> tiles = Get_Tiles(tile.Coordinates, 3, 3);
            bool blocked = false;
            foreach (Tile hill_tile in tiles) {
                if (hill_tile == tile) {
                    continue;
                }
                if(hill_tile.Internal_Name.StartsWith("hill") || hill_tile.Internal_Name.StartsWith("water")) {
                    blocked = true;
                    break;
                }
            }
            if (!blocked) {
                foreach (Tile hill_tile in tiles) {
                    if (hill_tile == tile) {
                        continue;
                    }
                    if (hill_tile.X == tile.X + 1 && hill_tile.Y == tile.Y) {
                        hill_tile.Change_To(TilePrototypes.Instance.Get("hill_8"));
                    } else if (hill_tile.X == tile.X + 2 && hill_tile.Y == tile.Y) {
                        hill_tile.Change_To(TilePrototypes.Instance.Get("hill_9"));
                    } else if (hill_tile.X == tile.X && hill_tile.Y == tile.Y + 1) {
                        hill_tile.Change_To(TilePrototypes.Instance.Get("hill_4"));
                    } else if (hill_tile.X == tile.X + 1 && hill_tile.Y == tile.Y + 1) {
                        hill_tile.Change_To(TilePrototypes.Instance.Get("hill_5"));
                    } else if (hill_tile.X == tile.X + 2 && hill_tile.Y == tile.Y + 1) {
                        hill_tile.Change_To(TilePrototypes.Instance.Get("hill_6"));
                    } else if (hill_tile.X == tile.X && hill_tile.Y == tile.Y + 2) {
                        hill_tile.Change_To(TilePrototypes.Instance.Get("hill_1"));
                    } else if (hill_tile.X == tile.X + 1 && hill_tile.Y == tile.Y + 2) {
                        hill_tile.Change_To(TilePrototypes.Instance.Get("hill_2"));
                    } else if (hill_tile.X == tile.X + 2 && hill_tile.Y == tile.Y + 2) {
                        hill_tile.Change_To(TilePrototypes.Instance.Get("hill_3"));
                    } else {
                        hill_tile.Change_To(TilePrototypes.Instance.Get("placeholder"));
                    }
                }
            } else {
                tile.Change_To(TilePrototypes.Instance.Get(RNG.Instance.Next(0, 100) < Mathf.RoundToInt((forest_size_setting * 50.0f) + (forest_count_setting * 50.0f)) ? "sparse_forest" : "grass"));
            }
        } else if (tile.Internal_Name == "forest") {
            foreach (Tile t in Get_Tiles(tile.Coordinates.Shift(new Coordinates(-spread_range, -spread_range)), spread_range * 2 + 1, spread_range * 2 + 1)) {
                if(t.Internal_Name != "grass") {
                    continue;
                }
                float distance = tile.Coordinates.Distance(t.Coordinates);
                float distance_multiplier = Mathf.Pow(0.40f, distance) + ((0.60f * forest_size_setting) / distance + 0.1f);
                int chance = Mathf.RoundToInt(spread_chance_base * distance_multiplier);
                if(RNG.Instance.Next(0, 100) <= chance) {
                    t.Change_To(TilePrototypes.Instance.Get("sparse_forest"));
                }
            }
        } else if(tile.Internal_Name == "sparse_forest") {
            foreach (Tile t in Get_Tiles(tile.Coordinates.Shift(new Coordinates(-spread_range, -spread_range)), spread_range * 2 + 1, spread_range * 2 + 1)) {
                if (t.Internal_Name != "grass") {
                    continue;
                }
                float distance = tile.Coordinates.Distance(t.Coordinates);
                float distance_multiplier = Mathf.Pow(0.40f, distance) + ((0.60f * forest_size_setting) / distance + 0.1f);
                int chance = Mathf.RoundToInt((spread_chance_base / 10.0f) * distance_multiplier);
                if (RNG.Instance.Next(0, 100) <= chance) {
                    t.Change_To(TilePrototypes.Instance.Get("sparse_forest"));
                }
            }
        } else if (tile.Internal_Name.StartsWith("hill_")) {
            foreach (Tile t in Get_Tiles(tile.Coordinates.Shift(new Coordinates(-spread_range, -spread_range)), spread_range * 2 + 1, spread_range * 2 + 1)) {
                if (t.Internal_Name != "grass") {
                    continue;
                }
                float distance = tile.Coordinates.Distance(t.Coordinates);
                float distance_multiplier = Mathf.Pow(0.40f, distance) + ((0.60f * forest_size_setting) / distance + 0.1f);
                int chance = Mathf.RoundToInt((spread_chance_base / 20.0f) * distance_multiplier);
                if (RNG.Instance.Next(0, 100) <= chance) {
                    t.Change_To(TilePrototypes.Instance.Get("sparse_forest"));
                }
            }
        } else if (lake_spawns.Contains(tile)) {
            foreach (Tile t in Get_Tiles(tile.Coordinates.Shift(new Coordinates(-water_range, -water_range)), water_range * 2 + 1, water_range * 2 + 1)) {
                if (t.Internal_Name.StartsWith("hill_")) {
                    continue;
                }
                float distance = tile.Coordinates.Distance(t.Coordinates);
                if (distance < water_range * 0.75f || RNG.Instance.Next(0, 100) < 25) {
                    t.Change_To(TilePrototypes.Instance.Get("water_nesw"));
                }
            }
        } else if (tile.Internal_Name.StartsWith("fertile_ground")) {
            foreach (Tile t in Get_Tiles(tile.Coordinates.Shift(new Coordinates(-spread_range_fertility, -spread_range_fertility)), spread_range_fertility * 2 + 1, spread_range_fertility * 2 + 1)) {
                if (t.Internal_Name != "grass") {
                    continue;
                }
                float distance = tile.Coordinates.Distance(t.Coordinates);
                float distance_multiplier = Mathf.Pow(0.40f, distance) + ((0.60f * fertility) / distance + 0.1f);
                int chance = Mathf.RoundToInt(15.0f * distance_multiplier);
                if (RNG.Instance.Next(0, 1000) <= chance) {
                    t.Change_To(TilePrototypes.Instance.Get("fertile_ground"));
                }
            }
        } else if (tile.Internal_Name.StartsWith("water")) {
            foreach (Tile t in Get_Tiles(tile.Coordinates.Shift(new Coordinates(-spread_range_fertility, -spread_range_fertility)), spread_range_fertility * 2 + 1, spread_range_fertility * 2 + 1)) {
                if (t.Internal_Name != "grass") {
                    continue;
                }
                float distance = tile.Coordinates.Distance(t.Coordinates);
                float distance_multiplier = Mathf.Pow(0.40f, distance) + ((0.60f * fertility) / distance + 0.1f);
                int chance = Mathf.RoundToInt((fertility * 15.0f) * distance_multiplier);
                if (RNG.Instance.Next(0, 1000) <= chance) {
                    t.Change_To(TilePrototypes.Instance.Get("fertile_ground"));
                } else {
                    distance_multiplier = Mathf.Pow(0.40f, distance) + ((0.60f * forest_size_setting) / distance + 0.1f);
                    chance = Mathf.RoundToInt((spread_chance_base / 30.0f) * distance_multiplier);
                    if (RNG.Instance.Next(0, 100) <= chance) {
                        t.Change_To(TilePrototypes.Instance.Get("sparse_forest"));
                    }
                }
            }
        }

        if (tile.Minerals.Count == 0 && tile.Can_Have_Minerals) {
            List<Mineral> spawn = new List<Mineral>();
            bool water_front = false;
            foreach(KeyValuePair<Coordinates.Direction, Tile> pair in Get_Adjanced_Tiles(tile, true)) {
                if (pair.Value.Is_Water) {
                    water_front = true;
                    break;
                }
            }
            foreach (Mineral mineral in Enum.GetValues(typeof(Mineral))) {
                int chance = MINERAL_BASE_SPAWN_CHANCE[mineral];
                if(mineral != Mineral.Clay && tile.Internal_Name.StartsWith("hill_")) {
                    chance = Mathf.RoundToInt(1.25f * chance);
                } else if(mineral == Mineral.Clay && water_front) {
                    chance = Mathf.RoundToInt(1.35f * chance);
                }
                if (RNG.Instance.Next(0, 10000) < chance) {
                    spawn.Add(mineral);
                }
            }
            if (spawn.Count > 0) {
                Mineral mineral = spawn.Count == 1 ? spawn[0] : RNG.Instance.Item(spawn);
                if (!minerals_spawned.Contains(mineral)) {
                    minerals_spawned.Add(mineral);
                }
                float amount = 3.0f * (MINERAL_VEIN_RICHNESS[mineral] * (0.25f + (1.50f * RNG.Instance.Next_F())));
                tile.Minerals.Add(mineral, amount);
                tile.Mineral_Spawns.Add(mineral);
                foreach (Tile t in Get_Tiles_In_Circle(tile.Coordinates, MINERAL_VEIN_SIZE[mineral])) {
                    if (!t.Can_Have_Minerals) {
                        continue;
                    }
                    float distance = t.Coordinates.Distance(tile.Coordinates);
                    int chance = Mathf.RoundToInt(((MINERAL_VEIN_RICHNESS[mineral] * 35.0f) + (MINERAL_BASE_SPAWN_CHANCE[mineral] * 0.5f)) * (((MINERAL_VEIN_SIZE[mineral] - distance) + 1) / (MINERAL_VEIN_SIZE[mineral]) + 1));
                    if (RNG.Instance.Next(0, 100) < chance) {
                        amount = 3.0f * (MINERAL_VEIN_RICHNESS[mineral] * (0.25f + (1.50f * RNG.Instance.Next_F())));
                        if (t.Minerals.ContainsKey(mineral)) {
                            t.Minerals[mineral] = amount;
                        } else {
                            t.Minerals.Add(mineral, amount);
                        }
                    }
                }
            }
        }

        loop_progress++;
        generation_index_x++;
        if (generation_index_x == Width) {
            generation_index_x = 0;
            generation_index_y++;
        }
        if (generation_index_y == Height) {
            generation_index_x = 0;
            generation_index_y = 0;
            loop_progress = 0;
            generation_loop++;
        }
        Update_Progress();
    }

    private void Generation_Loop_3()
    {
        Tile tile = tiles[generation_index_x][generation_index_y];

        List<Tile> tiles_around = new List<Tile>();
        int fine_tuned = 0;
        foreach (Coordinates.Direction direction in Coordinates.Directly_Adjacent_Directions) {
            Tile t = Get_Tile_At(tile.Coordinates, direction);
            if (t != null) {
                tiles_around.Add(t);
                if (fine_tuned_tiles.Contains(t)) {
                    fine_tuned++;
                }
            }
        }
        int forests = tiles_around.Where(x => x.Internal_Name == "forest" || x.Internal_Name == "sparse_forest").ToArray().Length;
        int dense_forests = tiles_around.Where(x => x.Internal_Name == "forest").ToArray().Length;

        if (tile.Internal_Name == "sparse_forest" && forests >= 3) {
            int dense_chance = 35 + Mathf.RoundToInt(35.0f * forest_density_setting) + dense_forests;
            if (RNG.Instance.Next(0, 100) < dense_chance) {
                tile.Change_To(TilePrototypes.Instance.Get("forest"));
                fine_tuned_tiles.Add(tile);
            }
        } else if (tile.Internal_Name == "sparse_forest" && forests <= 2 && fine_tuned <= 1) {
            int grass_chance = 55 - Mathf.RoundToInt(10.0f * forest_count_setting) - forests;
            if (RNG.Instance.Next(0, 100) < grass_chance) {
                tile.Change_To(TilePrototypes.Instance.Get("grass"));
                fine_tuned_tiles.Add(tile);
            }
        } else if (tile.Internal_Name == "grass" && forests >= 3) {
            int forest_chance = 50 + Mathf.RoundToInt(25.0f * forest_density_setting) + dense_forests;
            int dense_chance = 35 + Mathf.RoundToInt(35.0f * forest_density_setting) + dense_forests;
            if (RNG.Instance.Next(0, 100) < forest_chance) {
                if(RNG.Instance.Next(0, 100) < dense_chance) {
                    tile.Change_To(TilePrototypes.Instance.Get("forest"));
                } else {
                    tile.Change_To(TilePrototypes.Instance.Get("sparse_forest"));
                }
                fine_tuned_tiles.Add(tile);
            }
        } else if (tile.Internal_Name == "grass" && RNG.Instance.Next(0, 100) <= Mathf.RoundToInt(4.0f + ((forest_count_setting + forest_density_setting + forest_size_setting) / 1.5f))) {
            tile.Change_To(TilePrototypes.Instance.Get("fertile_ground"));
            fine_tuned_tiles.Add(tile);
        }

        if (!mineral_safety_spawn) {
            mineral_safety_spawn = true;
            int max_tries = 10000;
            bool tile_not_found = false;
            foreach(Mineral mineral in IMPORTANT_MINERALS) {
                if (!minerals_spawned.Contains(mineral)) {
                    Tile random_tile = Get_Tile_At(RNG.Instance.Next(0, Width - 1), RNG.Instance.Next(0, Height - 1));
                    int try_count = 0;
                    while (!random_tile.Can_Have_Minerals) {
                        random_tile = Get_Tile_At(RNG.Instance.Next(0, Width - 1), RNG.Instance.Next(0, Height - 1));
                        try_count++;
                        if(try_count == max_tries) {
                            tile_not_found = true;
                            break;
                        }
                    }
                    if (tile_not_found) {
                        break;
                    }
                    float amount = 1.5f * (MINERAL_VEIN_RICHNESS[mineral] * (0.25f + (1.50f * RNG.Instance.Next_F())));
                    random_tile.Minerals.Add(mineral, amount);
                    random_tile.Mineral_Spawns.Add(mineral);
                    foreach (Tile t in Get_Tiles_In_Circle(random_tile.Coordinates, MINERAL_VEIN_SIZE[mineral] * 0.5f)) {
                        float distance = t.Coordinates.Distance(random_tile.Coordinates);
                        int chance = Mathf.RoundToInt(((MINERAL_VEIN_RICHNESS[mineral] * 35.0f) + (MINERAL_BASE_SPAWN_CHANCE[mineral] * 0.5f)) * (((MINERAL_VEIN_SIZE[mineral] - distance) + 1) / (MINERAL_VEIN_SIZE[mineral]) + 1));
                        if (t.Can_Have_Minerals && RNG.Instance.Next(0, 100) < chance) {
                            amount = 1.5f * (MINERAL_VEIN_RICHNESS[mineral] * (0.25f + (1.50f * RNG.Instance.Next_F())));
                            if (t.Minerals.ContainsKey(mineral)) {
                                t.Minerals[mineral] = amount;
                            } else {
                                t.Minerals.Add(mineral, amount);
                            }
                        }
                    }
                }
            }
        }

        loop_progress++;
        generation_index_x++;
        if (generation_index_x == Width) {
            generation_index_x = 0;
            generation_index_y++;
        }
        if (generation_index_y == Height) {
            generation_index_x = 0;
            generation_index_y = 0;
            loop_progress = 0;
            generation_loop++;
        }
        Update_Progress();
    }

    private void Generation_Loop_4()
    {
        Tile tile = tiles[generation_index_x][generation_index_y];

        if(tile.Internal_Name == "water_nesw") {
            Dictionary<Coordinates.Direction, Tile> adjanced_tiles = Get_Adjanced_Tiles(tile);
            StringBuilder name_builder = new StringBuilder("water_");
            foreach(Coordinates.Direction direction in Coordinates.Directly_Adjacent_Directions) {
                if(!adjanced_tiles.ContainsKey(direction) || adjanced_tiles[direction].Internal_Name.StartsWith("water_")) {
                    name_builder.Append(direction.ToString().ToLower()[0]);
                }
            }
            string internal_name = name_builder.ToString();
            if (internal_name != tile.Internal_Name && TilePrototypes.Instance.Exists(internal_name)) {
                tile.Change_To(TilePrototypes.Instance.Get(internal_name));
            }
        }

        loop_progress++;
        generation_index_x++;
        if (generation_index_x == Width) {
            generation_index_x = 0;
            generation_index_y++;
        }
        if (generation_index_y == Height) {
            Finish_Generation();
        } else {
            Update_Progress();
        }
    }

    public void Finish_Generation()
    {
        Update_Ship_Access();
        fine_tuned_tiles.Clear();
        lake_spawns.Clear();
        Update_Appeal();
        Contacts.Instance.Generate_New();
        State = MapState.Normal;
        ProgressBarManager.Instance.Active = false;
        City.Instance.Start_New("PLACEHOLDER");
        Active = true;
        CameraManager.Instance.Set_Camera_Location(Get_Tile_At(Width / 2, Height / 2).Coordinates.Vector);
        BuildMenuManager.Instance.Interactable = true;
        TopGUIManager.Instance.Active = true;
    }
    
    private void Update_Progress()
    {
        if(State == MapState.Generating) {
            float max = Width * Height * 4;
            float current = ((generation_loop - 1) * (Width * Height)) + loop_progress;
            float progress = current / max;
            string message = "Generating map...";
            switch (generation_loop) {
                case 1:
                    message = "Generating map layout...";
                    break;
                case 2:
                    message = "Generating terrain...";
                    break;
                case 3:
                    message = "Fine tuning terrain...";
                    break;
                case 4:
                    message = "Finalizing...";
                    break;
            }
            ProgressBarManager.Instance.Show(message, progress);
        } else if(State == MapState.Saving) {
            float max = (Width * Height) + City.Instance.Buildings.Count;
            float current = save_load_loop == 1 ? loop_progress : (Width * Height) + loop_progress;
            float progress = current / max;
            ProgressBarManager.Instance.Show("Saving...", progress);
        } else if (State == MapState.Loading) {
            float max = (Width * Height) + SaveManager.Instance.Data.City.Buildings.Count + 10.0f;
            float current = save_load_loop == 1 ? loop_progress : (Width * Height) + loop_progress + (city_loaded ? 10.0f : 0.0f);
            float progress = current / max;
            ProgressBarManager.Instance.Show("Loading...", progress);
        }
    }

    public void Start_Saving(string path)
    {
        State = MapState.Saving;
        SaveManager.Instance.Start_Saving(path);
        save_index_x = 0;
        save_index_y = 0;
        loop_progress = 0;
        save_load_loop = 1;
        ProgressBarManager.Instance.Active = true;
        Update_Progress();
    }

    private void Save_Loop_1()
    {
        Tile tile = tiles[save_index_x][save_index_y];
        SaveManager.Instance.Add(tile.Save_Data());

        loop_progress++;
        save_index_x++;
        if (save_index_x == Width) {
            save_index_x = 0;
            save_index_y++;
        }
        if (save_index_y == Height) {
            save_index_x = 0;
            save_index_y = 0;
            loop_progress = 0;
            save_load_loop++;
        }
        Update_Progress();
    }

    private void Save_Loop_2()
    {
        if(City.Instance.Buildings.Count == 0) {
            Finish_Saving();
            return;
        }
        Building building = City.Instance.Buildings[loop_progress];
        SaveManager.Instance.Add(building.Save_Data());

        loop_progress++;
        if (loop_progress == City.Instance.Buildings.Count) {
            Finish_Saving();
        } else {
            Update_Progress();
        }
    }

    private void Finish_Saving()
    {
        SaveManager.Instance.Finish_Saving();
        State = MapState.Normal;
        ProgressBarManager.Instance.Active = false;
    }

    public void Start_Loading(string path)
    {
        Delete();
        City.Instance.Delete();
        TimeManager.Instance.Paused = true;
        TimeManager.Instance.Reset_Time();
        NotificationManager.Instance.Clear_Notifications();
        Active = false;
        State = MapState.Loading;
        Entities = new List<Entity>();
        entities_to_be_deleted = new List<Entity>();
        SaveManager.Instance.Start_Loading(path);
        save_load_loop = 1;
        loop_progress = 0;
        save_index_x = 0;
        save_index_y = 0;
        city_loaded = false;
        Width = SaveManager.Instance.Data.Map.Width;
        Height = SaveManager.Instance.Data.Map.Height;
        Building.Reset_Current_Id();

        tiles = new List<List<Tile>>();
        for (int x = 0; x < Width; x++) {
            tiles.Add(new List<Tile>());
            for (int y = 0; y < Height; y++) {
                tiles[x].Add(null);
            }
        }

        ProgressBarManager.Instance.Active = true;
        Update_Progress();
    }

    private void Load_Loop_1()
    {
        tiles[save_index_x][save_index_y] = new Tile(save_index_x, save_index_y, TilePrototypes.Instance.Get(SaveManager.Instance.Get_Tile(save_index_x, save_index_y).Internal_Name));

        loop_progress++;
        save_index_x++;
        if (save_index_x == Width) {
            save_index_x = 0;
            save_index_y++;
        }
        if (save_index_y == Height) {
            save_index_x = 0;
            save_index_y = 0;
            loop_progress = 0;
            save_load_loop++;
        }
        Update_Progress();
    }

    private void Load_Loop_2()
    {
        if (!city_loaded) {
            City.Instance.Load(SaveManager.Instance.Data.City);
            city_loaded = true;
            return;
        }
        if (SaveManager.Instance.Data.City.Buildings.Count == 0) {
            Finish_Loading();
            return;
        }
        BuildingSaveData data = SaveManager.Instance.Data.City.Buildings[loop_progress];
        if (!data.Is_Residence) {
            Building building = new Building(data);
            City.Instance.Buildings.Add(building);
        } else {
            Residence residence = new Residence(data);
            City.Instance.Buildings.Add(residence);
        }

        loop_progress++;
        if (loop_progress == SaveManager.Instance.Data.City.Buildings.Count) {
            Finish_Loading();
        } else {
            Update_Progress();
        }
    }

    private void Finish_Loading()
    {
        for (int x = 0; x < Width; x++) {
            for (int y = 0; y < Height; y++) {
                TileSaveData save_data = SaveManager.Instance.Get_Tile(x, y);
                foreach (WorkSaveData data in save_data.Worked_By) {
                    Building building = City.Instance.Buildings.FirstOrDefault(b => b.Id == data.Id);
                    if (building == null) {
                        CustomLogger.Instance.Error(string.Format("Building not found #{0}", data.Id));
                    } else {
                        tiles[x][y].Worked_By.Add(new Tile.WorkData() { Building = building, Type = (Tile.Work_Type)data.Type });
                    }
                }
                foreach(MineralSaveData mineral_data in save_data.Minerals) {
                    tiles[x][y].Minerals.Add((Mineral)mineral_data.Mineral, mineral_data.Amount);
                }
                foreach (int mineral_i in save_data.Mineral_Spawns) {
                    tiles[x][y].Mineral_Spawns.Add((Mineral)mineral_i);
                }
                tiles[x][y].Water_Flow = save_data.Water_Flow != -1 ? (Coordinates.Direction)save_data.Water_Flow : (Coordinates.Direction?)null;
            }
        }

        Ship_Spawns = new List<Tile>();
        foreach(CoordinateSaveData spawn in SaveManager.Instance.Data.Map.Ship_Spawns) {
            Ship_Spawns.Add(tiles[spawn.X][spawn.Y]);
        }
        Update_Ship_Access();

        Update_Appeal();
        TimeManager.Instance.Set_Time(SaveManager.Instance.Data.Days);
        SaveManager.Instance.Finish_Loading();
        State = MapState.Normal;
        View = MapView.None;
        ProgressBarManager.Instance.Active = false;
        Active = true;
        CameraManager.Instance.Set_Camera_Location(Get_Tile_At(Width / 2, Height / 2).Coordinates.Vector);
        BuildMenuManager.Instance.Interactable = true;
        TopGUIManager.Instance.Active = true;
    }

    private void Update_Appeal()
    {
        for (int x = 0; x < Width; x++) {
            for (int y = 0; y < Height; y++) {
                tiles[x][y].Appeal = 0.0f;
            }
        }
        for (int x = 0; x < Width; x++) {
            for (int y = 0; y < Height; y++) {
                Tile tile = tiles[x][y];
                float appeal = tile.Base_Appeal;
                float range = tile.Base_Appeal_Range;
                if(tile.Building != null && tile.Building.Is_Built) {
                    appeal = tile.Building.Appeal;
                    range = tile.Building.Appeal_Range;
                }
                if(appeal == 0.0f && range == 0.0f) {
                    continue;
                }
                tile.Appeal += appeal;
                if (range == 0.0f) {
                    continue;
                }
                List<Tile> affected_tiles = Get_Tiles_In_Circle(tile.Coordinates, range);
                foreach(Tile affected in affected_tiles) {
                    if(affected == tile) {
                        continue;
                    }
                    affected.Appeal += Tile.Calculate_Appeal_Effect(tile.Coordinates, appeal, range, affected.Coordinates);
                }
            }
        }
    }

    public MapView View {
        get {
            return view;
        }
        set {
            if(view == value) {
                return;
            }
            view = value;
            for(int x = 0; x < Width; x++) {
                for(int y = 0; y < Height; y++) {
                    switch (view) {
                        case MapView.Appeal:
                            tiles[x][y].Show_Text(Helper.Float_To_String(tiles[x][y].Appeal, 1));
                            break;
                        case MapView.Minerals:
                            tiles[x][y].Show_Text(tiles[x][y].Mineral_String());
                            int mineral_icon_index = 0;
                            List<GameObject> new_icons = new List<GameObject>();
                            float icon_spacing = 0.75f;
                            foreach(Mineral mineral in tiles[x][y].Mineral_Spawns) {
                                GameObject icon = GameObject.Instantiate(
                                    PrefabManager.Instance.Mineral,
                                    new Vector3(
                                        tiles[x][y].GameObject.transform.position.x + (mineral_icon_index * icon_spacing),
                                        tiles[x][y].GameObject.transform.position.y,
                                        tiles[x][y].GameObject.transform.position.z
                                    ),
                                    Quaternion.identity,
                                    Entity_Container.transform
                                );
                                icon.name = string.Format("{0}_{1}_{2}", x, y, mineral.ToString());
                                icon.GetComponentInChildren<SpriteRenderer>().sprite = SpriteManager.Instance.Get(MINERAL_SPRITES[mineral], MINERAL_SPRITE_TYPE);
                                icon.GetComponentInChildren<TextMesh>().text = "<b>" + Helper.Snake_Case_To_UI(mineral.ToString(), true) + "</b>";
                                icon.GetComponentInChildren<TextMesh>().gameObject.GetComponentInChildren<MeshRenderer>().sortingLayerName = "Icon text";
                                mineral_icons.Add(icon);
                                new_icons.Add(icon);
                                mineral_icon_index++;
                            }
                            if(new_icons.Count >= 2) {
                                foreach(GameObject icon in new_icons) {
                                    icon.transform.position = new Vector3(
                                        icon.transform.position.x - (((new_icons.Count - 1) * 0.5f) * icon_spacing),
                                        icon.transform.position.y,
                                        icon.transform.position.z
                                    );
                                }
                            }
                            break;
                        case MapView.Ship_Access:
                            if (tiles[x][y].Is_Water && tiles[x][y].Building == null) {
                                if (Ship_Spawns.Contains(tiles[x][y])) {
                                    tiles[x][y].Show_Text("Spawn");
                                } else if (tiles[x][y].Has_Ship_Access) {
                                    tiles[x][y].Show_Text("Yes");
                                } else {
                                    tiles[x][y].Show_Text("No");
                                }
                            }
                            break;
                        default:
                            tiles[x][y].Hide_Text();
                            break;
                    }
                }
            }
            if(view != MapView.Minerals) {
                foreach(GameObject icon in mineral_icons) {
                    GameObject.Destroy(icon);
                }
                mineral_icons.Clear();
            }
        }
    }

    public void Delete_Entity(Entity entity)
    {
        if (!entities_to_be_deleted.Contains(entity)) {
            entities_to_be_deleted.Add(entity);
        }
    }

    public Tile Get_Tile_At(int x, int y, Coordinates.Direction? offset = null)
    {
        return Get_Tile_At(new Coordinates(x, y), offset);
    }
    
    public Tile Get_Tile_At(Coordinates coordinates, Coordinates.Direction? offset = null)
    {
        if (offset == null) {
            if (coordinates.X < 0 || coordinates.Y < 0 || coordinates.X >= Width || coordinates.Y >= Height) {
                return null;
            }
            return tiles[coordinates.X][coordinates.Y];
        }
        coordinates.Shift((Coordinates.Direction)offset);
        if (coordinates.X < 0 || coordinates.Y < 0 || coordinates.X >= Width || coordinates.Y >= Height) {
            return null;
        }
        return tiles[coordinates.X][coordinates.Y];
    }

    public bool Is_Edge_Tile(Tile tile)
    {
        return tile.Coordinates.X == 0 || tile.Coordinates.Y == 0 || tile.Coordinates.X == Width - 1 || tile.Coordinates.Y == Height - 1;
    }

    /// <summary>
    /// Returns list of tiles in the rectangle
    /// </summary>
    /// <param name="coordinates"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <returns></returns>
    public List<Tile> Get_Tiles(Coordinates coordinates, int width, int height)
    {
        return Get_Tiles(coordinates.X, coordinates.Y, width, height);
    }

    /// <summary>
    /// Returns list of tiles in the rectangle
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <returns></returns>
    public List<Tile> Get_Tiles(int x, int y, int width, int height)
    {
        List<Tile> tiles = new List<Tile>();
        for (int x_i = x; x_i < width + x; x_i++) {
            for (int y_i = y; y_i < height + y; y_i++) {
                Tile tile = Get_Tile_At(x_i, y_i);
                if (tile != null) {
                    tiles.Add(tile);
                }
            }
        }
        return tiles;
    }

    /// <summary>
    /// No diagonals
    /// </summary>
    /// <param name="building"></param>
    /// <returns></returns>
    public List<Tile> Get_Tiles_Around(Building building)
    {
        List<Tile> tiles = new List<Tile>();
        Tile tile = building.Tile;
        if(tile == null) {
            //Preview
            tile = Get_Tile_At(new Coordinates((int)building.GameObject.transform.position.x, (int)building.GameObject.transform.position.y));
            if(tile == null) {
                return new List<Tile>();
            }
        }
        if(building.Size == Building.BuildingSize.s1x1) {
            foreach(Coordinates.Direction direction in Coordinates.Directly_Adjacent_Directions) {
                Tile t = Get_Tile_At(tile.Coordinates, direction);
                if(t != null) {
                    tiles.Add(t);
                }
            }
        } else {
            int x_start = tile.Coordinates.X - 1;
            int x_end = tile.Coordinates.X + building.Width;
            int y_start = tile.Coordinates.Y - 1;
            int y_end = tile.Coordinates.Y + building.Height;
            for(int x = x_start; x <= x_end; x++) {
                Tile north = Get_Tile_At(x, y_end);
                if(north != null && x != x_start && x != x_end) {
                    tiles.Add(north);
                }
                Tile south = Get_Tile_At(x, y_start);
                if(south != null && x != x_start && x != x_end) {
                    tiles.Add(south);
                }
            }
            for(int y = y_start + 1; y <= y_end - 1; y++) {
                Tile west = Get_Tile_At(x_start, y);
                if(west != null) {
                    tiles.Add(west);
                }
                Tile east = Get_Tile_At(x_end, y);
                if(east != null) {
                    tiles.Add(east);
                }
            }
        }
        return tiles;
    }

    public List<Building> Get_Buildings_Around(Building building)
    {
        List<Building> buildings = new List<Building>();
        foreach(Tile tile in Get_Tiles_Around(building)) {
            if(tile != null && tile.Building != null && !buildings.Contains(tile.Building)) {
                buildings.Add(tile.Building);
            }
        }
        return buildings;
    }
    
    public Dictionary<Coordinates.Direction, Tile> Get_Adjanced_Tiles(Tile tile, bool diagonal = false)
    {
        Dictionary<Coordinates.Direction, Tile> tiles = new Dictionary<Coordinates.Direction, Tile>();
        Dictionary<Coordinates.Direction, Tile> possible_tiles = new Dictionary<Coordinates.Direction, Tile>();
        if (!diagonal) {
            possible_tiles.Add(Coordinates.Direction.North, Get_Tile_At(tile.X, tile.Y, Coordinates.Direction.North));
            possible_tiles.Add(Coordinates.Direction.East, Get_Tile_At(tile.X, tile.Y, Coordinates.Direction.East));
            possible_tiles.Add(Coordinates.Direction.South, Get_Tile_At(tile.X, tile.Y, Coordinates.Direction.South));
            possible_tiles.Add(Coordinates.Direction.West, Get_Tile_At(tile.X, tile.Y, Coordinates.Direction.West));
        } else {
            foreach (Coordinates.Direction direction in Enum.GetValues(typeof(Coordinates.Direction))) {
                possible_tiles.Add(direction, Get_Tile_At(tile.X, tile.Y, direction));
            }
        }
        foreach (KeyValuePair<Coordinates.Direction, Tile> possible_tile in possible_tiles) {
            if (possible_tile.Value != null) {
                tiles.Add(possible_tile.Key, possible_tile.Value);
            }
        }
        return tiles;
    }

    /// <summary>
    /// Returns tiles in a circle around specified point
    /// </summary>
    /// <param name="coordinates"></param>
    /// <param name="range"></param>
    /// <param name="plus_half_x"></param>
    /// <param name="plus_half_y"></param>
    /// <returns></returns>
    public List<Tile> Get_Tiles_In_Circle(Coordinates coordinates, float range, bool plus_half_x = false, bool plus_half_y = false)
    {
        return Get_Tiles_In_Circle(coordinates.X, coordinates.Y, range, plus_half_x, plus_half_y);
    }

    /// <summary>
    /// Returns tiles in a circle around specified point
    /// </summary>
    /// <param name="x_p"></param>
    /// <param name="y_p"></param>
    /// <param name="range"></param>
    /// <param name="plus_half_x"></param>
    /// <param name="plus_half_y"></param>
    /// <returns></returns>
    public List<Tile> Get_Tiles_In_Circle(int x_p, int y_p, float range, bool plus_half_x = false, bool plus_half_y = false)
    {
        List<Tile> list = new List<Tile>();

        bool searching = true;
        int loop_index = 0;
        while (searching) {
            bool insert = false;

            int loop_x = x_p - 1 - loop_index;
            int loop_x_max = x_p + 1 + loop_index;
            if (plus_half_x) {
                loop_x = x_p - loop_index;
                loop_x_max = x_p + 1 + loop_index;
            }
            int loop_y = y_p - 1 - loop_index;
            int loop_y_max = y_p + 1 + loop_index;
            if (plus_half_y) {
                loop_y = y_p - loop_index;
                loop_y_max = y_p + 1 + loop_index;
            }
            float x_p_2 = x_p + (Convert.ToInt32(plus_half_x) * 0.5f);
            float y_p_2 = y_p + (Convert.ToInt32(plus_half_y) * 0.5f);

            for (int x = loop_x; x <= loop_x_max; x++) {
                for (int y = loop_y; y <= loop_y_max; y++) {
                    if (x == loop_x || x == loop_x_max || y == loop_y || y == loop_y_max) {
                        Tile tile = Get_Tile_At(x, y);
                        if (tile != null && range >= Mathf.Sqrt((x_p_2 - tile.X) * (x_p_2 - tile.X) + (y_p_2 - tile.Y) * (y_p_2 - tile.Y))) {
                            list.Add(tile);
                            insert = true;
                        }
                    }
                }
            }

            loop_index++;
            if (!insert) {
                searching = false;
            }
        }

        return list;
    }

    /// <summary>
    /// Returns tiles in line between two points
    /// </summary>
    /// <param name="tile_1"></param>
    /// <param name="tile_2"></param>
    /// <returns></returns>
    public List<Tile> Get_Tiles_In_Line(Tile tile_1, Tile tile_2)
    {
        return Get_Tiles_In_Line(tile_1.Coordinates, tile_2.Coordinates);
    }

    /// <summary>
    /// Returns tiles in line between two points
    /// </summary>
    /// <param name="point_1"></param>
    /// <param name="point_2"></param>
    /// <returns></returns>
    public List<Tile> Get_Tiles_In_Line(Coordinates point_1, Coordinates point_2)
    {
        List<Tile> line = new List<Tile>();
        foreach(Coordinates c in Pathfinding.Straight_Line(point_1, point_2)) {
            line.Add(Get_Tile_At(c));
        }
        return line;
    }

    public void Update_Ship_Access()
    {
        for(int x = 0; x < Width; x++) {
            for (int y = 0; y < Height; y++) {
                tiles[x][y].Has_Ship_Access = false;
            }
        }
        foreach(Tile spawn in Ship_Spawns) {
            Update_Ship_Access_Recursive(spawn);
        }
    }

    private void Update_Ship_Access_Recursive(Tile tile)
    {
        if(!tile.Is_Water || (tile.Building != null && !tile.Building.Is_Deconstructing) || tile.Has_Ship_Access) {
            return;
        }
        tile.Has_Ship_Access = true;
        foreach(Tile t in Get_Adjanced_Tiles(tile).Select(x => x.Value).ToArray()) {
            Update_Ship_Access_Recursive(t);
        }
    }

    /// <summary>
    /// Are map's tile's GameObjects active?
    /// </summary>
    public bool Active
    {
        get {
            return Tile_Container.activeSelf;
        }
        set {
            if (Tile_Container.activeSelf == value) {
                return;
            }
            Tile_Container.SetActive(value);
        }
    }

    /// <summary>
    /// Deletes all tile GameObjects
    /// </summary>
    public void Delete()
    {
        if (State != MapState.Normal) {
            return;
        }
        for (int x = 0; x < Width; x++) {
            for (int y = 0; y < Height; y++) {
                tiles[x][y].Delete();
            }
        }
        if(Entities != null) {
            foreach(Entity entity in Entities) {
                entity.Delete(true);
            }
            Entities.Clear();
        }
        foreach(GameObject icon in mineral_icons) {
            GameObject.Destroy(icon);
        }
        mineral_icons.Clear();
    }
}
