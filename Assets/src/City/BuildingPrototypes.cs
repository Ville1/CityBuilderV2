using System.Collections.Generic;
using System.Linq;
using System.Text;

public class BuildingPrototypes {
    private static BuildingPrototypes instance;

    private List<Building> prototypes;

    private BuildingPrototypes()
    {
        prototypes = new List<Building>();

        prototypes.Add(new Building("Townhall", Building.TOWN_HALL_INTERNAL_NAME, Building.UI_Category.Admin, "town_hall", Building.BuildingSize.s2x2, 1000, new Dictionary<Resource, int>(), 0, new List<Resource>() { Resource.Lumber, Resource.Stone, Resource.Tools, Resource.Wood },
            2000, 10.0f, 0, new Dictionary<Resource, float>(), 0.0f, 3.0f, 50.0f, new Dictionary<Building.Resident, int>(), 0, false, false, false, 0.0f, 10, null, null, null, null, new List<Resource>(), new List<Resource>()));

        prototypes.Add(new Residence("Cabin", "hut", Building.UI_Category.Housing, "hut", Building.BuildingSize.s2x2, 100, new Dictionary<Resource, int>() {
            { Resource.Wood, 100 }, { Resource.Stone, 15 }, { Resource.Tools, 10 }
        }, 100, new List<Resource>(), 0, 115, new Dictionary<Resource, float>() { { Resource.Wood, 0.05f } }, 0.0f, 0.0f, 0.0f, new Dictionary<Building.Resident, int>() { { Building.Resident.Peasant, 10 } }, null, null, null, null, new List<Resource>(), new List<Resource>()));

        prototypes.Add(new Building("Wood Cutters Lodge", "wood_cutters_lodge", Building.UI_Category.Forestry, "wood_cutters_lodge", Building.BuildingSize.s2x2, 100, new Dictionary<Resource, int>() {
            { Resource.Wood, 75 }, { Resource.Stone, 5 }, { Resource.Tools, 15 }
        }, 90, new List<Resource>(), 0, 0.0f, 85, new Dictionary<Resource, float>(), 0.75f, 0.0f, 0, new Dictionary<Building.Resident, int>() { { Building.Resident.Peasant, 10 } }, 10, true, false, true, 5.0f, 0, delegate(Building building) {
            foreach (Tile tile in building.Get_Tiles_In_Circle(building.Range)) {
                tile.Worked_By.Add(building);
            }
        }, delegate (Building building, float delta_time) {
            if (!building.Is_Operational) {
                return;
            }
            float wood = 0.0f;
            foreach(Tile tile in building.Get_Tiles_In_Circle(building.Range)) {
                if (tile.Worked_By.FirstOrDefault(x => x.Internal_Name == building.Internal_Name) == building) {
                    if(tile.Internal_Name == "forest") {
                        wood += 0.75f;
                    } else if(tile.Internal_Name == "sparse_forest") {
                        wood += 1.75f;
                    }
                }
            }
            wood /= 5.0f;
            float firewood = wood * building.Special_Settings.First(x => x.Name == "firewood_ratio").Slider_Value;
            wood -= firewood;
            building.Produce(Resource.Wood, wood, delta_time);
            building.Produce(Resource.Firewood, firewood, delta_time);
        }, delegate(Building building) {
            foreach (Tile tile in building.Get_Tiles_In_Circle(building.Range)) {
                if (tile.Worked_By.Contains(building)) {
                    tile.Worked_By.Remove(building);
                }
            }
        }, delegate (Building building) {
            List<Tile> worked_tiles = new List<Tile>();
            foreach (Tile tile in building.Get_Tiles_In_Circle(building.Range)) {
                Building b = tile.Worked_By.FirstOrDefault(x => x.Internal_Name == building.Internal_Name);
                if (tile.Worked_By.FirstOrDefault(x => x.Internal_Name == building.Internal_Name) == building) {
                    worked_tiles.Add(tile);
                }
            }
            return worked_tiles;
        }, new List<Resource>(), new List<Resource>() { Resource.Wood, Resource.Firewood }));
        prototypes.First(x => x.Internal_Name == "wood_cutters_lodge").Special_Settings.Add(new SpecialSetting("firewood_ratio", "Firewood production", SpecialSetting.SettingType.Slider, 0.0f));

        prototypes.Add(new Building("Cobblestone Road", "cobblestone_road", Building.UI_Category.Infrastructure, "road_nesw", Building.BuildingSize.s1x1, 10, new Dictionary<Resource, int>() { { Resource.Stone, 10 }, { Resource.Tools, 1 } }, 10,
            new List<Resource>(), 0, 0.0f, 10, new Dictionary<Resource, float>() { { Resource.Stone, 0.01f } }, 0.0f, 0.0f, 0, new Dictionary<Building.Resident, int>(), 0, false, true, true, 0.0f, 0, null, null, null, null, new List<Resource>(), new List<Resource>()));
        prototypes.First(x => x.Internal_Name == "cobblestone_road").Sprite.Add_Logic(delegate (Building building) {
            Tile tile = building.Tile;
            if (tile == null) {
                tile = Map.Instance.Get_Tile_At(new Coordinates((int)building.GameObject.transform.position.x, (int)building.GameObject.transform.position.y));
                if (tile == null) {
                    return "road_nesw";
                }
            }
            Tile north_tile = Map.Instance.Get_Tile_At(tile.Coordinates, Coordinates.Direction.North);
            bool north = north_tile == null || (north_tile.Building != null && north_tile.Building.Is_Road);
            Tile east_tile = Map.Instance.Get_Tile_At(tile.Coordinates, Coordinates.Direction.East);
            bool east = east_tile == null || (east_tile.Building != null && east_tile.Building.Is_Road);
            Tile south_tile = Map.Instance.Get_Tile_At(tile.Coordinates, Coordinates.Direction.South);
            bool south = south_tile == null || (south_tile.Building != null && south_tile.Building.Is_Road);
            Tile west_tile = Map.Instance.Get_Tile_At(tile.Coordinates, Coordinates.Direction.West);
            bool west = west_tile == null || (west_tile.Building != null && west_tile.Building.Is_Road);
            if (!north && !east && !south && !west) {
                return "road_nesw";
            }
            StringBuilder builder = new StringBuilder("road_");
            if (north) {
                builder.Append("n");
            }
            if (east) {
                builder.Append("e");
            }
            if (south) {
                builder.Append("s");
            }
            if (west) {
                builder.Append("w");
            }
            return builder.ToString();
        });
        prototypes.Add(new Building("Lumber Mill", "lumber_mill", Building.UI_Category.Forestry, "lumber_mill", Building.BuildingSize.s3x3, 200, new Dictionary<Resource, int>() {
            { Resource.Wood, 225 }, { Resource.Stone, 20 }, { Resource.Tools, 40 }
        }, 200, new List<Resource>(), 0, 50.0f, 250, new Dictionary<Resource, float>() { { Resource.Wood, 0.10f } }, 2.00f, 0.0f, 0, new Dictionary<Building.Resident, int>() {
            { Building.Resident.Peasant, 20 }, { Building.Resident.Citizen, 10 } }, 20, true, false, true, 0.0f, 7, null, delegate(Building building, float delta_time) {
                if (!building.Is_Operational) {
                    return;
                }
                building.Process(Resource.Wood, 20.0f, Resource.Lumber, 10.0f, delta_time);
            }, null, null, new List<Resource>() { Resource.Wood }, new List<Resource>() { Resource.Lumber }));

        prototypes.Add(new Building("Clear Trees", "clear_trees", Building.UI_Category.Forestry, "axe", Building.BuildingSize.s1x1, 1, new Dictionary<Resource, int>() { { Resource.Tools, 1 } }, 5, new List<Resource>(),
            0, 0.0f, 0, new Dictionary<Resource, float>(), 0.0f, 0.0f, 0, new Dictionary<Building.Resident, int>(), 0, false, false, false, 0.0f, 0, null, delegate (Building building, float delta_time) {
                if (!building.Is_Operational) {
                    return;
                }
                float required_progress = building.Tile.Internal_Name == "forest" ? 5.0f : 2.5f;
                float progress = building.Data.ContainsKey("chop_progress") ? (float)building.Data["chop_progress"] : 0.0f;
                progress += delta_time;
                if (building.Data.ContainsKey("chop_progress")) {
                    building.Data["chop_progress"] = progress;
                } else {
                    building.Data.Add("chop_progress", progress);
                }
                if (progress >= required_progress) {
                    building.Storage.Add(Resource.Wood, building.Tile.Internal_Name == "forest" ? 5.0f : 2.5f);
                    building.Tile.Change_To(TilePrototypes.Instance.Get("grass"));
                    building.Deconstruct(true, false);
                }
            }, null, null, new List<Resource>(), new List<Resource>()));
        prototypes.First(x => x.Internal_Name == "clear_trees").Sprite.Animation_Frame_Time = 0.5f;
        prototypes.First(x => x.Internal_Name == "clear_trees").Sprite.Animation_Sprites = new List<string>() { "chop_trees_1", "chop_trees_2" };
        prototypes.First(x => x.Internal_Name == "clear_trees").Permitted_Terrain.Add("forest");
        prototypes.First(x => x.Internal_Name == "clear_trees").Permitted_Terrain.Add("sparse_forest");
        prototypes.First(x => x.Internal_Name == "clear_trees").Tags.Add(Building.Tag.Undeletable);

        prototypes.Add(new Building("Storehouse", "storehouse", Building.UI_Category.Infrastructure, "storehouse", Building.BuildingSize.s2x2, 200, new Dictionary<Resource, int>() {
            { Resource.Stone, 30 }, { Resource.Tools, 25 }, { Resource.Lumber, 275 }
        }, 225, new List<Resource>() { Resource.Lumber, Resource.Stone, Resource.Tools, Resource.Wood },
        2000, 25.0f, 250, new Dictionary<Resource, float>() { { Resource.Lumber, 0.05f } }, 1.0f, 0.0f, 0.0f, new Dictionary<Building.Resident, int>() { { Building.Resident.Peasant, 10 } }, 10, false, false, true, 0.0f, 10, null, null, null, null, new List<Resource>(), new List<Resource>()));

        prototypes.Add(new Building("Cellar", "cellar", Building.UI_Category.Infrastructure, "cellar", Building.BuildingSize.s1x1, 100, new Dictionary<Resource, int>() {
            { Resource.Wood, 15 }, { Resource.Stone, 50 }, { Resource.Tools, 10 }, { Resource.Lumber, 50 }
        }, 100, new List<Resource>() { Resource.Roots, Resource.Berries, Resource.Mushrooms, Resource.Herbs },
        1000, 25.0f, 110, new Dictionary<Resource, float>() { { Resource.Wood, 0.05f } }, 0.5f, 0.0f, 0.0f, new Dictionary<Building.Resident, int>() { { Building.Resident.Peasant, 5 } }, 5, false, false, true, 0.0f, 10, null, null, null, null, new List<Resource>(), new List<Resource>()));

        prototypes.Add(new Building("Wood Stockpile", "wood_stockpile", Building.UI_Category.Infrastructure, "wood_stockpile", Building.BuildingSize.s2x2, 100, new Dictionary<Resource, int>() {
            { Resource.Wood, 25 }, { Resource.Stone, 5 }, { Resource.Tools, 5 }
        }, 100, new List<Resource>() { Resource.Wood, Resource.Lumber, Resource.Firewood },
        1000, 25.0f, 50, new Dictionary<Resource, float>() { { Resource.Wood, 0.01f } }, 0.25f, 0.0f, 0.0f, new Dictionary<Building.Resident, int>() { { Building.Resident.Peasant, 5 } }, 5, false, false, true, 0.0f, 10, null, null, null, null, new List<Resource>(), new List<Resource>()));

        prototypes.Add(new Building("Gatherers Lodge", "gatherers_lodge", Building.UI_Category.Forestry, "gatherers_lodge", Building.BuildingSize.s2x2, 100, new Dictionary<Resource, int>() {
            { Resource.Wood, 85 }, { Resource.Stone, 10 }, { Resource.Tools, 10 }
        }, 100, new List<Resource>(), 0, 0.0f, 95, new Dictionary<Resource, float>() { { Resource.Wood, 0.05f } }, 0.75f, 0.0f, 0, new Dictionary<Building.Resident, int>() { { Building.Resident.Peasant, 5 } }, 5, true, false, true, 6.0f, 0, delegate (Building building) {
            foreach (Tile tile in building.Get_Tiles_In_Circle(building.Range)) {
                tile.Worked_By.Add(building);
            }
        }, delegate (Building building, float delta_time) {
            if (!building.Is_Operational) {
                return;
            }
            float roots = 0.0f;
            float berries = 0.0f;
            float mushrooms = 0.0f;
            float herbs = 0.0f;
            foreach (Tile tile in building.Get_Tiles_In_Circle(building.Range)) {
                if (tile.Worked_By.FirstOrDefault(x => x.Internal_Name == building.Internal_Name) == building && tile.Building == null) {
                    if (tile.Internal_Name == "grass") {
                        roots     += 0.010f;
                        berries   += 0.025f;
                        mushrooms += 0.005f;
                        herbs     += 0.005f;
                    } else if (tile.Internal_Name == "fertile_ground") {
                        roots     += 0.015f;
                        berries   += 0.045f;
                        mushrooms += 0.010f;
                        herbs     += 0.025f;
                    } else if (tile.Internal_Name == "sparse_forest") {
                        roots     += 0.045f;
                        berries   += 0.025f;
                        mushrooms += 0.045f;
                        herbs     += 0.005f;
                    } else if (tile.Internal_Name == "forest") {
                        roots     += 0.045f;
                        berries   += 0.020f;
                        mushrooms += 0.050f;
                        herbs     += 0.010f;
                    }
                }
            }
            float multiplier = 1.0f;
            building.Produce(Resource.Roots, roots * multiplier, delta_time);
            building.Produce(Resource.Berries, berries * multiplier, delta_time);
            building.Produce(Resource.Mushrooms, mushrooms * multiplier, delta_time);
            building.Produce(Resource.Herbs, herbs * multiplier, delta_time);
        }, delegate (Building building) {
            foreach (Tile tile in building.Get_Tiles_In_Circle(building.Range)) {
                if (tile.Worked_By.Contains(building)) {
                    tile.Worked_By.Remove(building);
                }
            }
        }, delegate (Building building) {
            List<Tile> worked_tiles = new List<Tile>();
            foreach (Tile tile in building.Get_Tiles_In_Circle(building.Range)) {
                Building b = tile.Worked_By.FirstOrDefault(x => x.Internal_Name == building.Internal_Name);
                if (tile.Worked_By.FirstOrDefault(x => x.Internal_Name == building.Internal_Name) == building) {
                    worked_tiles.Add(tile);
                }
            }
            return worked_tiles;
        }, new List<Resource>(), new List<Resource>() { Resource.Roots, Resource.Berries, Resource.Mushrooms, Resource.Herbs }));

        prototypes.Add(new Building("Marketplace", "marketplace", Building.UI_Category.Services, "marketplace", Building.BuildingSize.s3x3, 150, new Dictionary<Resource, int>() {
            { Resource.Lumber, 20 },
            { Resource.Stone, 90 },
            { Resource.Tools, 10 }
        }, 110, new List<Resource>(), 0, 0.0f, 110, new Dictionary<Resource, float>() {
            { Resource.Stone, 0.1f }
        }, 1.0f, 0.0f, 0.0f, new Dictionary<Building.Resident, int>() {
            { Building.Resident.Peasant, 10 },
            { Building.Resident.Citizen, 10 }
        }, 10, true, true, true, 0.0f, 10, null, delegate (Building market, float delta_time) {
            if (!market.Is_Operational) {
                return;
            }
            foreach(Building building in market.Get_Connected_Buildings(market.Road_Range).Select(x => x.Key).ToArray()) {
                if(!(building is Residence)) {
                    continue;
                }
                Residence residence = building as Residence;
                residence.Serve(Residence.ServiceType.Food, 0.1f, 0.5f);
            }
        }, null, null, new List<Resource>() { Resource.Berries, Resource.Roots, Resource.Mushrooms, Resource.Herbs, Resource.Firewood }, new List<Resource>()));
    }

    public static BuildingPrototypes Instance
    {
        get {
            if (instance == null) {
                instance = new BuildingPrototypes();
            }
            return instance;
        }
    }

    public Building Get(string internal_name)
    {
        Building building = prototypes.FirstOrDefault(x => x.Internal_Name == internal_name);
        if(building == null) {
            CustomLogger.Instance.Error(string.Format("Building not found: {0}", building));
        }
        return building;
    }

    public bool Is_Residence(string internal_name)
    {
        return prototypes.Exists(x => x is Residence && x.Internal_Name == internal_name);
    }

    public Residence Get_Residence(string internal_name)
    {
        Residence building = (Residence)prototypes.FirstOrDefault(x => x is Residence && x.Internal_Name == internal_name);
        if (building == null) {
            CustomLogger.Instance.Error(string.Format("Residence not found: {0}", building));
        }
        return building;
    }

    public List<Building> Get(Building.UI_Category category)
    {
        return prototypes.Where(x => x.Category == category).ToList();
    }
}
