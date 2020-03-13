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
            2000, 10.0f, 0, new Dictionary<Resource, float>(), 0.0f, 3.0f, 50.0f, new Dictionary<Building.Resident, int>(), 0, false, false, false, 0.0f, 10, null, null, null, new List<Resource>(), new List<Resource>()));
        prototypes.Add(new Residence("Cabin", "hut", Building.UI_Category.Housing, "hut", Building.BuildingSize.s2x2, 100, new Dictionary<Resource, int>() {
            { Resource.Wood, 100 }, { Resource.Stone, 15 }, { Resource.Tools, 10 }
        }, 100, new List<Resource>(), 0, 115, new Dictionary<Resource, float>() { { Resource.Wood, 0.05f } }, 0.0f, 0.0f, 0.0f, new Dictionary<Building.Resident, int>() { { Building.Resident.Peasant, 10 } }, null, null, null, new List<Resource>(), new List<Resource>()));
        prototypes.Add(new Building("Wood Cutters Lodge", "wood_cutters_lodge", Building.UI_Category.Forestry, "wood_cutters_lodge", Building.BuildingSize.s2x2, 100, new Dictionary<Resource, int>() {
            { Resource.Wood, 75 }, { Resource.Stone, 5 }, { Resource.Tools, 15 }
        }, 90, new List<Resource>(), 0, 0.0f, 85, new Dictionary<Resource, float>() { { Resource.Wood, 0.05f } }, 0.75f, 0.0f, 0, new Dictionary<Building.Resident, int>() { { Building.Resident.Peasant, 10 } }, 10, true, false, true, 5.0f, 0, null, delegate (Building building, float delta_time) {
            if (!building.Is_Operational) {
                return;
            }
            building.Produce(Resource.Wood, 1.0f, delta_time);
        }, null, new List<Resource>(), new List<Resource>() { Resource.Wood }));
        prototypes.Add(new Building("Cobblestone Road", "cobblestone_road", Building.UI_Category.Infrastructure, "road_nesw", Building.BuildingSize.s1x1, 10, new Dictionary<Resource, int>() { { Resource.Stone, 10 }, { Resource.Tools, 1 } }, 10,
            new List<Resource>(), 0, 0.0f, 10, new Dictionary<Resource, float>() { { Resource.Stone, 0.01f } }, 0.0f, 0.0f, 0, new Dictionary<Building.Resident, int>(), 0, false, true, true, 0.0f, 0, null, null, null, new List<Resource>(), new List<Resource>()));
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
            { Building.Resident.Peasant, 20 }, { Building.Resident.Citizen, 10 } }, 20, true, false, true, 0.0f, 7, null, null, null, new List<Resource>() { Resource.Wood }, new List<Resource>() { Resource.Lumber }));
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
            }, null, new List<Resource>(), new List<Resource>()));
        prototypes.First(x => x.Internal_Name == "clear_trees").Sprite.Animation_Frame_Time = 0.5f;
        prototypes.First(x => x.Internal_Name == "clear_trees").Sprite.Animation_Sprites = new List<string>() { "chop_trees_1", "chop_trees_2" };
        prototypes.First(x => x.Internal_Name == "clear_trees").Permitted_Terrain.Add("forest");
        prototypes.First(x => x.Internal_Name == "clear_trees").Permitted_Terrain.Add("sparse_forest");
        prototypes.First(x => x.Internal_Name == "clear_trees").Tags.Add(Building.Tag.Undeletable);

        prototypes.Add(new Building("Storehouse", "storehouse", Building.UI_Category.Infrastructure, "storehouse", Building.BuildingSize.s2x2, 200, new Dictionary<Resource, int>() {
            { Resource.Stone, 30 }, { Resource.Tools, 25 }//, { Resource.Lumber, 275 }
        }, 225, new List<Resource>() { Resource.Lumber, Resource.Stone, Resource.Tools, Resource.Wood },
        2000, 25.0f, 250, new Dictionary<Resource, float>() { { Resource.Lumber, 0.05f } }, 1.0f, 0.0f, 0.0f, new Dictionary<Building.Resident, int>() { { Building.Resident.Peasant, 10 } }, 10, true, false, true, 0.0f, 10, null, null, null, new List<Resource>(), new List<Resource>()));
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
