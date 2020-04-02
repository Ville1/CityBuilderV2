using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class BuildingPrototypes {
    private static BuildingPrototypes instance;

    private List<Building> prototypes;

    private Building.OnBuiltDelegate Reserve_Tiles(Tile.Work_Type type)
    {
        return delegate (Building building) {
            foreach (Tile tile in building.Get_Tiles_In_Circle(building.Range)) {
                tile.Add_Workers(building, type);
            }
        };
    }

    private Building.OnHighlightDelegate Highlight_Tiles(Tile.Work_Type type)
    {
        return delegate (Building building) {
            List<Tile> worked_tiles = new List<Tile>();
            foreach (Tile tile in building.Get_Tiles_In_Circle(building.Range)) {
                if (tile.Can_Work(building, type)) {
                    worked_tiles.Add(tile);
                }
            }
            return worked_tiles;
        };
    }

    private BuildingPrototypes()
    {
        Building.OnDeconstructDelegate unreserve_tiles = delegate (Building building) {
            foreach (Tile tile in building.Get_Tiles_In_Circle(building.Range)) {
                tile.Remove_Workers(building);
            }
        };

        prototypes = new List<Building>();

        prototypes.Add(new Building("Townhall", Building.TOWN_HALL_INTERNAL_NAME, Building.UI_Category.Admin, "town_hall", Building.BuildingSize.s2x2, 1000, new Dictionary<Resource, int>(), 0, new List<Resource>() { Resource.Lumber, Resource.Stone, Resource.Tools, Resource.Wood },
            2500, 40.0f, 0, new Dictionary<Resource, float>(), 0.0f, 3.0f, 30.0f, new Dictionary<Building.Resident, int>(), 0, false, false, false, 0.0f, 25, null, null, null, null, new List<Resource>(), new List<Resource>(), 0.05f, 7.0f));
        prototypes.First(x => x.Internal_Name == Building.TOWN_HALL_INTERNAL_NAME).Sprites.Add(new SpriteData("town_hall_1"));

        prototypes.Add(new Residence("Cabin", "hut", Building.UI_Category.Housing, "hut", Building.BuildingSize.s2x2, 100, new Dictionary<Resource, int>() {
            { Resource.Wood, 100 }, { Resource.Stone, 15 }, { Resource.Tools, 10 }
        }, 100, new List<Resource>(), 0, 115, new Dictionary<Resource, float>() { { Resource.Wood, 0.05f } }, 0.01f, 0.0f, 0.0f, 0.0f, new Dictionary<Building.Resident, int>() { { Building.Resident.Peasant, 10 } }, 0.0f, null, null, null, null, new List<Resource>(), new List<Resource>(), 0.0f, 0.0f));
        prototypes.First(x => x.Internal_Name == "hut").Sprites.Add(new SpriteData("hut_1"));

        prototypes.Add(new Building("Wood Cutters Lodge", "wood_cutters_lodge", Building.UI_Category.Forestry, "wood_cutters_lodge", Building.BuildingSize.s2x2, 100, new Dictionary<Resource, int>() {
            { Resource.Wood, 75 }, { Resource.Stone, 5 }, { Resource.Tools, 15 }
        }, 90, new List<Resource>(), 0, 0.0f, 85, new Dictionary<Resource, float>(), 0.75f, 0.0f, 0, new Dictionary<Building.Resident, int>() { { Building.Resident.Peasant, 10 } }, 10, true, false, true, 4.0f, 0, Reserve_Tiles(Tile.Work_Type.Cut_Wood),
        delegate (Building building, float delta_time) {
            if (!building.Is_Operational) {
                return;
            }
            float wood = 0.0f;
            foreach(Tile tile in building.Get_Tiles_In_Circle(building.Range)) {
                if (tile.Can_Work(building, Tile.Work_Type.Cut_Wood) && tile.Building == null) {
                    if(tile.Internal_Name == "forest") {
                        wood += 1.75f;
                    } else if(tile.Internal_Name == "sparse_forest") {
                        wood += 0.75f;
                    } else if (tile.Internal_Name.StartsWith("hill_")) {
                        wood += 0.25f;
                    }
                }
            }
            wood /= 4.0f;
            float firewood = wood * building.Special_Settings.First(x => x.Name == "firewood_ratio").Slider_Value;
            wood -= firewood;
            building.Produce(Resource.Wood, wood, delta_time);
            building.Produce(Resource.Firewood, firewood, delta_time);
        }, unreserve_tiles, Highlight_Tiles(Tile.Work_Type.Cut_Wood), new List<Resource>(), new List<Resource>() { Resource.Wood, Resource.Firewood }, 0.0f, 0.0f));
        prototypes.First(x => x.Internal_Name == "wood_cutters_lodge").Special_Settings.Add(new SpecialSetting("firewood_ratio", "Firewood production", SpecialSetting.SettingType.Slider, 0.0f));

        prototypes.Add(new Building("Dirt Road", "dirt_road", Building.UI_Category.Infrastructure, "dirt_road_nesw", Building.BuildingSize.s1x1, 5, new Dictionary<Resource, int>() { { Resource.Tools, 1 } }, 10,
            new List<Resource>(), 0, 0.0f, 5, new Dictionary<Resource, float>(), 0.01f, 0.0f, 0, new Dictionary<Building.Resident, int>(), 0, false, true, true, 0.0f, 0, null, null, null, null, new List<Resource>(), new List<Resource>(), 0.0f, 0.0f));
        prototypes.First(x => x.Internal_Name == "dirt_road").Sprite.Add_Logic(delegate (Building building) {
            Tile tile = building.Tile;
            if (tile == null) {
                tile = Map.Instance.Get_Tile_At(new Coordinates((int)building.GameObject.transform.position.x, (int)building.GameObject.transform.position.y));
                if (tile == null) {
                    return "dirt_road_nesw";
                }
            }
            Tile north_tile = Map.Instance.Get_Tile_At(tile.Coordinates, Coordinates.Direction.North);
            bool north = north_tile == null || (north_tile.Building != null && north_tile.Building.Is_Road);
            bool north_stone = north_tile != null && north_tile.Building != null && north_tile.Building.Internal_Name == "cobblestone_road";
            Tile east_tile = Map.Instance.Get_Tile_At(tile.Coordinates, Coordinates.Direction.East);
            bool east = east_tile == null || (east_tile.Building != null && east_tile.Building.Is_Road);
            bool east_stone = east_tile != null && east_tile.Building != null && east_tile.Building.Internal_Name == "cobblestone_road";
            Tile south_tile = Map.Instance.Get_Tile_At(tile.Coordinates, Coordinates.Direction.South);
            bool south = south_tile == null || (south_tile.Building != null && south_tile.Building.Is_Road);
            bool south_stone = south_tile != null && south_tile.Building != null && south_tile.Building.Internal_Name == "cobblestone_road";
            Tile west_tile = Map.Instance.Get_Tile_At(tile.Coordinates, Coordinates.Direction.West);
            bool west = west_tile == null || (west_tile.Building != null && west_tile.Building.Is_Road);
            bool west_stone = west_tile != null && west_tile.Building != null && west_tile.Building.Internal_Name == "cobblestone_road";
            if (!north && !east && !south && !west) {
                return "dirt_road_nesw";
            }
            
            if (north && south && !east && !west && !south_stone && north_stone) {
                return "dirt_road_s_sto_n";
            }
            if (north && south && !east && !west && south_stone && !north_stone) {
                return "dirt_road_n_sto_s";
            }
            if (!north && !south && east && west && !east_stone && west_stone) {
                return "dirt_road_e_sto_w";
            }
            if (!north && !south && east && west && east_stone && !west_stone) {
                return "dirt_road_w_sto_e";
            }

            StringBuilder builder = new StringBuilder("dirt_road_");
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
        prototypes.First(x => x.Internal_Name == "dirt_road").Tags.Add(Building.Tag.Does_Not_Block_Wind);

        prototypes.Add(new Building("Cobblestone Road", "cobblestone_road", Building.UI_Category.Infrastructure, "road_nesw", Building.BuildingSize.s1x1, 10, new Dictionary<Resource, int>() { { Resource.Stone, 10 }, { Resource.Tools, 1 } }, 10,
            new List<Resource>(), 0, 0.0f, 10, new Dictionary<Resource, float>() { { Resource.Stone, 0.01f } }, 0.01f, 0.0f, 0, new Dictionary<Building.Resident, int>(), 0, false, true, true, 0.0f, 0, null, null, null, null, new List<Resource>(), new List<Resource>(), 0.0f, 0.0f));
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
        prototypes.First(x => x.Internal_Name == "cobblestone_road").Tags.Add(Building.Tag.Does_Not_Block_Wind);

        prototypes.Add(new Building("Wooden Bridge", "wooden_bridge", Building.UI_Category.Infrastructure, "bridge_ew", Building.BuildingSize.s1x1, 10, new Dictionary<Resource, int>() { { Resource.Lumber, 10 }, { Resource.Wood, 10 }, { Resource.Stone, 1 }, { Resource.Tools, 1 } }, 25,
            new List<Resource>(), 0, 0.0f, 50, new Dictionary<Resource, float>() { { Resource.Lumber, 0.01f }, { Resource.Wood, 0.01f } }, 0.01f, 0.0f, 0, new Dictionary<Building.Resident, int>(), 0, false, true, true, 0.0f, 0, null, null, null, null, new List<Resource>(), new List<Resource>(), 0.0f, 0.0f));
        prototypes.First(x => x.Internal_Name == "wooden_bridge").Tags.Add(Building.Tag.Does_Not_Block_Wind);
        prototypes.First(x => x.Internal_Name == "wooden_bridge").Tags.Add(Building.Tag.Bridge);
        prototypes.First(x => x.Internal_Name == "wooden_bridge").Sprites.Add(new SpriteData("bridge_ns"));
        prototypes.First(x => x.Internal_Name == "wooden_bridge").Sprites.Add(new SpriteData("bridge_e"));
        prototypes.First(x => x.Internal_Name == "wooden_bridge").Sprites.Add(new SpriteData("bridge_w"));
        prototypes.First(x => x.Internal_Name == "wooden_bridge").Permitted_Terrain.Add("water_nesw");
        prototypes.First(x => x.Internal_Name == "wooden_bridge").Permitted_Terrain.Add("water_es");
        prototypes.First(x => x.Internal_Name == "wooden_bridge").Permitted_Terrain.Add("water_sw");
        prototypes.First(x => x.Internal_Name == "wooden_bridge").Permitted_Terrain.Add("water_nw");
        prototypes.First(x => x.Internal_Name == "wooden_bridge").Permitted_Terrain.Add("water_ne");
        prototypes.First(x => x.Internal_Name == "wooden_bridge").Permitted_Terrain.Add("water_esw");
        prototypes.First(x => x.Internal_Name == "wooden_bridge").Permitted_Terrain.Add("water_nes");
        prototypes.First(x => x.Internal_Name == "wooden_bridge").Permitted_Terrain.Add("water_new");
        prototypes.First(x => x.Internal_Name == "wooden_bridge").Permitted_Terrain.Add("water_nsw");
        prototypes.First(x => x.Internal_Name == "wooden_bridge").Permitted_Terrain.Add("water_n");
        prototypes.First(x => x.Internal_Name == "wooden_bridge").Permitted_Terrain.Add("water_e");
        prototypes.First(x => x.Internal_Name == "wooden_bridge").Permitted_Terrain.Add("water_s");
        prototypes.First(x => x.Internal_Name == "wooden_bridge").Permitted_Terrain.Add("water_w");
        prototypes.First(x => x.Internal_Name == "wooden_bridge").Permitted_Terrain.Add("water_");

        prototypes.Add(new Building("Lumber Mill", "lumber_mill", Building.UI_Category.Forestry, "lumber_mill", Building.BuildingSize.s3x3, 200, new Dictionary<Resource, int>() {
            { Resource.Wood, 225 }, { Resource.Stone, 20 }, { Resource.Tools, 40 }
        }, 200, new List<Resource>(), 0, 50.0f, 250, new Dictionary<Resource, float>() { { Resource.Wood, 0.10f } }, 2.00f, 0.0f, 0, new Dictionary<Building.Resident, int>() {
            { Building.Resident.Peasant, 20 }, { Building.Resident.Citizen, 10 } }, 20, true, false, true, 0.0f, 8, null, delegate(Building building, float delta_time) {
                if (!building.Is_Operational) {
                    return;
                }
                building.Process(Resource.Wood, 20.0f, Resource.Lumber, 10.0f, delta_time);
            }, null, null, new List<Resource>() { Resource.Wood }, new List<Resource>() { Resource.Lumber }, -0.75f, 4.0f));

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
            }, null, null, new List<Resource>(), new List<Resource>(), 0.0f, 0.0f));
        prototypes.First(x => x.Internal_Name == "clear_trees").Sprite.Animation_Frame_Time = 0.5f;
        prototypes.First(x => x.Internal_Name == "clear_trees").Sprite.Animation_Sprites = new List<string>() { "chop_trees_1", "chop_trees_2" };
        prototypes.First(x => x.Internal_Name == "clear_trees").Permitted_Terrain.Add("forest");
        prototypes.First(x => x.Internal_Name == "clear_trees").Permitted_Terrain.Add("sparse_forest");
        prototypes.First(x => x.Internal_Name == "clear_trees").Tags.Add(Building.Tag.Undeletable);

        prototypes.Add(new Building("Storehouse", "storehouse", Building.UI_Category.Infrastructure, "storehouse", Building.BuildingSize.s2x2, 200, new Dictionary<Resource, int>() {
            { Resource.Stone, 30 }, { Resource.Tools, 25 }, { Resource.Lumber, 275 }
        }, 225, new List<Resource>() { Resource.Lumber, Resource.Stone, Resource.Tools, Resource.Wood, Resource.Firewood, Resource.Hide, Resource.Leather, Resource.Salt, Resource.Coal, Resource.Charcoal, Resource.Iron_Ore, Resource.Iron_Bars, Resource.Wool, Resource.Thread, Resource.Cloth, Resource.Barrels,
            Resource.Simple_Clothes, Resource.Leather_Clothes, Resource.Mechanisms, Resource.Clay, Resource.Bricks },
        2000, 65.0f, 250, new Dictionary<Resource, float>() { { Resource.Lumber, 0.05f } }, 1.0f, 0.0f, 0.0f, new Dictionary<Building.Resident, int>() { { Building.Resident.Peasant, 10 } }, 10, false, false, true, 0.0f, 16, null, null, null, null, new List<Resource>(), new List<Resource>(), 0.0f, 0.0f));
        prototypes.First(x => x.Internal_Name == "storehouse").Sprites.Add(new SpriteData("storehouse_1"));

        prototypes.Add(new Building("Cellar", "cellar", Building.UI_Category.Infrastructure, "cellar", Building.BuildingSize.s1x1, 100, new Dictionary<Resource, int>() {
            { Resource.Wood, 15 }, { Resource.Stone, 50 }, { Resource.Tools, 10 }, { Resource.Lumber, 50 }
        }, 100, new List<Resource>() { Resource.Roots, Resource.Berries, Resource.Mushrooms, Resource.Herbs, Resource.Game, Resource.Potatoes, Resource.Bread, Resource.Ale, Resource.Mutton, Resource.Corn, Resource.Fish },
        1000, 50.0f, 110, new Dictionary<Resource, float>() { { Resource.Wood, 0.05f } }, 0.5f, 0.0f, 0.0f, new Dictionary<Building.Resident, int>() { { Building.Resident.Peasant, 5 } }, 5, false, false, true, 0.0f, 14, null, null, null, null, new List<Resource>(), new List<Resource>(), 0.0f, 0.0f));
        prototypes.First(x => x.Internal_Name == "cellar").Tags.Add(Building.Tag.Does_Not_Block_Wind);

        prototypes.Add(new Building("Wood Stockpile", "wood_stockpile", Building.UI_Category.Infrastructure, "wood_stockpile", Building.BuildingSize.s2x2, 100, new Dictionary<Resource, int>() {
            { Resource.Wood, 25 }, { Resource.Stone, 5 }, { Resource.Tools, 5 }
        }, 100, new List<Resource>() { Resource.Wood, Resource.Lumber, Resource.Firewood },
        1000, 45.0f, 50, new Dictionary<Resource, float>() { { Resource.Wood, 0.01f } }, 0.25f, 0.0f, 0.0f, new Dictionary<Building.Resident, int>() { { Building.Resident.Peasant, 5 } }, 5, false, false, true, 0.0f, 14, null, null, null, null, new List<Resource>(), new List<Resource>(), 0.0f, 0.0f));
        prototypes.First(x => x.Internal_Name == "wood_stockpile").Sprites.Add(new SpriteData("wood_stockpile_1"));

        prototypes.Add(new Building("Gatherers Lodge", "gatherers_lodge", Building.UI_Category.Forestry, "gatherers_lodge", Building.BuildingSize.s2x2, 100, new Dictionary<Resource, int>() {
            { Resource.Wood, 85 }, { Resource.Stone, 10 }, { Resource.Tools, 10 }
        }, 100, new List<Resource>(), 0, 0.0f, 95, new Dictionary<Resource, float>() { { Resource.Wood, 0.05f } }, 0.75f, 0.0f, 0, new Dictionary<Building.Resident, int>() { { Building.Resident.Peasant, 5 } }, 5, true, false, true, 5.0f, 0, Reserve_Tiles(Tile.Work_Type.Forage),
        delegate (Building building, float delta_time) {
            if (!building.Is_Operational) {
                return;
            }
            float roots = 0.0f;
            float berries = 0.0f;
            float mushrooms = 0.0f;
            float herbs = 0.0f;
            foreach (Tile tile in building.Get_Tiles_In_Circle(building.Range)) {
                if (tile.Can_Work(building, Tile.Work_Type.Forage) && tile.Building == null) {
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
                    } else if (tile.Internal_Name.StartsWith("hill_")) {
                        roots     += 0.025f;
                        berries   += 0.010f;
                        mushrooms += 0.015f;
                        herbs     += 0.004f;
                    }
                }
            }
            float food_multiplier = 0.55f;
            float herb_multiplier = 1.00f;
            building.Produce(Resource.Roots, roots * food_multiplier, delta_time);
            building.Produce(Resource.Berries, berries * food_multiplier, delta_time);
            building.Produce(Resource.Mushrooms, mushrooms * food_multiplier, delta_time);
            building.Produce(Resource.Herbs, herbs * herb_multiplier, delta_time);
        }, unreserve_tiles, Highlight_Tiles(Tile.Work_Type.Forage), new List<Resource>(), new List<Resource>() { Resource.Roots, Resource.Berries, Resource.Mushrooms, Resource.Herbs }, 0.0f, 0.0f));

        prototypes.Add(new Building("Fisher's Hut", "fishers_hut", Building.UI_Category.Agriculture, "fishers_hut", Building.BuildingSize.s2x2, 90, new Dictionary<Resource, int>() {
            { Resource.Wood, 25 }, { Resource.Lumber, 60 }, { Resource.Stone, 10 }, { Resource.Tools, 10 }
        }, 115, new List<Resource>(), 0, 0.0f, 95, new Dictionary<Resource, float>() { { Resource.Lumber, 0.01f }, { Resource.Wood, 0.01f } }, 1.00f, 0.0f, 0, new Dictionary<Building.Resident, int>() { { Building.Resident.Peasant, 5 }, { Building.Resident.Citizen, 5 } }, 5, true, false, true, 7.0f, 0, Reserve_Tiles(Tile.Work_Type.Fish),
        delegate (Building building, float delta_time) {
            if (!building.Is_Operational) {
                return;
            }

            bool water_front = false;
            foreach(Tile t in Map.Instance.Get_Tiles_Around(building)) {
                if (t.Is_Water) {
                    water_front = true;
                    break;
                }
            }
            if (!water_front) {
                return;
            }

            float total_water_tiles = 0.0f;
            float own_water_tiles = 0.0f;
            foreach (Tile tile in building.Get_Tiles_In_Circle(building.Range)) {
                if (tile.Internal_Name.StartsWith("water")) {
                    total_water_tiles += 1.0f;
                    if (tile.Can_Work(building, Tile.Work_Type.Fish)) {
                        own_water_tiles += 1.0f;
                    }
                }
            }

            if(total_water_tiles == 0.0f) {
                return;
            }
            
            float prefered_water_count = 25.0f;
            float overlap_multiplier = own_water_tiles / total_water_tiles;
            float water_multiplier = own_water_tiles >= prefered_water_count ? 1.0f : own_water_tiles / prefered_water_count;

            float base_fish = 5.00f;

            float fish = (base_fish * overlap_multiplier) * water_multiplier;

            building.Produce(Resource.Fish, fish, delta_time);
        }, unreserve_tiles, Highlight_Tiles(Tile.Work_Type.Fish), new List<Resource>(), new List<Resource>() { Resource.Fish }, 0.0f, 0.0f));
        prototypes.First(x => x.Internal_Name == "fishers_hut").On_Build_Check = delegate (Building building, Tile tile, out string message) {
            message = string.Empty;
            bool water_front = false;
            foreach (Tile t in Map.Instance.Get_Tiles_Around(building)) {
                if (t.Is_Water) {
                    water_front = true;
                    break;
                }
            }
            if (!water_front) {
                message = "Must be placed on a waterfront";
                return false;
            }
            return water_front;
        };

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
            List<Resource> allowed_fuels = new List<Resource>();
            foreach (SpecialSetting setting in market.Special_Settings) {
                Resource resource = Resource.All.First(x => x.Type.ToString().ToLower() == setting.Name);
                if (setting.Toggle_Value) {
                    if (resource.Is_Fuel) {
                        allowed_fuels.Add(resource);
                    }
                    if (!market.Consumes.Contains(resource)) {
                        market.Consumes.Add(resource);
                    }
                } else if (market.Consumes.Contains(resource)) {
                    market.Consumes.Remove(resource);
                }
            }

            if (!market.Is_Operational || market.Efficency == 0.0f) {
                return;
            }
            float resources_for_full_service = Residence.RESOURCES_FOR_FULL_SERVICE;
            float income = 0.0f;
            float efficency_multiplier = (market.Efficency + 2.0f) / 3.0f;
            List<Residence> residences = new List<Residence>();
            float food_needed = 0.0f;
            float fuel_needed = 0.0f;
            float herbs_needed = 0.0f;
            float salt_needed = 0.0f;
            float clothes_needed = 0.0f;
            foreach (Building building in market.Get_Connected_Buildings(market.Road_Range).Select(x => x.Key).ToArray()) {
                if(!(building is Residence)) {
                    continue;
                }
                Residence residence = building as Residence;
                food_needed += residence.Service_Needed(Residence.ServiceType.Food) * resources_for_full_service;
                fuel_needed += residence.Service_Needed(Residence.ServiceType.Fuel) * resources_for_full_service;
                herbs_needed += residence.Service_Needed(Residence.ServiceType.Herbs) * resources_for_full_service;
                salt_needed += residence.Service_Needed(Residence.ServiceType.Salt) * resources_for_full_service;
                clothes_needed += residence.Service_Needed(Residence.ServiceType.Clothes) * resources_for_full_service;
                residences.Add(residence);
            }
            if(residences.Count == 0 || (food_needed == 0.0f && fuel_needed == 0.0f && herbs_needed == 0.0f && salt_needed == 0.0f && clothes_needed == 0.0f)) {
                return;
            }

            Resource fuel_type = market.Input_Storage.Where(x => allowed_fuels.Contains(x.Key)).OrderByDescending(x => x.Key.Value).FirstOrDefault().Key;
            if(fuel_type != null && fuel_needed > 0.0f && market.Input_Storage[fuel_type] > 0.0f) {
                float fuel_available = market.Input_Storage[fuel_type] * fuel_type.Fuel_Value;
                float fuel_supply_ratio = Math.Min(1.0f, fuel_available / fuel_needed);
                foreach(Residence residence in residences) {
                    float fuel_for_residence = residence.Service_Needed(Residence.ServiceType.Fuel) * fuel_supply_ratio * resources_for_full_service;
                    market.Input_Storage[fuel_type] -= fuel_for_residence;
                    market.Update_Delta(fuel_type, (-fuel_for_residence / delta_time) * TimeManager.Instance.Days_To_Seconds(1.0f, 1.0f));
                    residence.Serve(Residence.ServiceType.Fuel, residence.Service_Needed(Residence.ServiceType.Fuel) * fuel_supply_ratio, efficency_multiplier);
                    income += fuel_for_residence * fuel_type.Value;
                    if(market.Input_Storage[fuel_type] < 0.0f) {
                        //Rounding errors?
                        if (market.Input_Storage[fuel_type] < -0.00001f) {
                            CustomLogger.Instance.Error(string.Format("Negative fuel: {0}", fuel_type.ToString()));
                        }
                        market.Input_Storage[fuel_type] = 0.0f;
                    }
                }
            }

            if(food_needed > 0.0f) {
                float total_food = 0.0f;
                foreach(KeyValuePair<Resource, float> pair in market.Input_Storage) {
                    if (pair.Key.Is_Food) {
                        total_food += pair.Value;
                    }
                }
                
                if (total_food > 0.0f) {
                    float food_ratio = Math.Min(1.0f, total_food / food_needed);
                    float food_used = 0.0f;
                    float unique_food_count = 0.0f;
                    float min_food_ratio = -1.0f;
                    Dictionary<Resource, float> food_ratios = new Dictionary<Resource, float>();
                    foreach (KeyValuePair<Resource, float> pair in market.Input_Storage) {
                        if (pair.Key.Is_Food) {
                            float ratio = pair.Value / total_food;
                            food_ratios.Add(pair.Key, ratio);
                            if(pair.Value > 0.0f) {
                                unique_food_count += pair.Key.Food_Quality;
                                if(ratio < min_food_ratio || min_food_ratio == -1.0f) {
                                    min_food_ratio = ratio;
                                }
                            }
                        }
                    }
                    bool has_meat = false;
                    bool has_vegetables = false;
                    foreach (KeyValuePair<Resource, float> pair in market.Input_Storage) {
                        if (pair.Key.Is_Food && pair.Value > 0.0f) {
                            if(pair.Key.Food_Type == Resource.FoodType.Meat) {
                                has_meat = true;
                            } else if (pair.Key.Food_Type == Resource.FoodType.Vegetable) {
                                has_vegetables = true;
                            }
                        }
                    }
                    float disparity = (1.0f / unique_food_count) - min_food_ratio;
                    float food_quality = unique_food_count * (1.0f - disparity);
                    food_quality = (Mathf.Pow(food_quality, 0.5f) + ((food_quality - 1.0f) * 0.1f)) / 4.0f;
                    food_quality *= efficency_multiplier;
                    food_quality = Mathf.Clamp01(food_quality);
                    if (!(has_meat && has_vegetables)) {
                        food_quality *= 0.5f;
                    }
                    foreach (Residence residence in residences) {
                        float food_for_residence = (residence.Service_Needed(Residence.ServiceType.Food) * resources_for_full_service) * food_ratio;
                        food_used += food_for_residence;
                        residence.Serve(Residence.ServiceType.Food, residence.Service_Needed(Residence.ServiceType.Food) * food_ratio, food_quality);
                    }
                    foreach(KeyValuePair<Resource, float> pair in food_ratios) {
                        market.Input_Storage[pair.Key] -= pair.Value * food_used;
                        market.Update_Delta(pair.Key, (-(pair.Value * food_used) / delta_time) * TimeManager.Instance.Days_To_Seconds(1.0f, 1.0f), false);
                        income += pair.Key.Value * (pair.Value * food_used);
                        if (market.Input_Storage[pair.Key] < 0.0f) {
                            //Rounding errors?
                            if (market.Input_Storage[pair.Key] < -0.00001f) {
                                CustomLogger.Instance.Error(string.Format("Negative food: {0}", pair.Key.ToString()));
                            }
                            market.Input_Storage[pair.Key] = 0.0f;
                        }
                    }
                }
            }

            float herbs = market.Input_Storage.ContainsKey(Resource.Herbs) ? market.Input_Storage[Resource.Herbs] : 0.0f;
            if (herbs_needed != 0.0f && herbs != 0.0f) {
                float herb_ratio = Math.Min(herbs / herbs_needed, 1.0f);
                float herbs_used = 0.0f;
                foreach (Residence residence in residences) {
                    float herbs_for_residence = (residence.Service_Needed(Residence.ServiceType.Herbs) * resources_for_full_service) * herb_ratio;
                    herbs_used += herbs_for_residence;
                    residence.Serve(Residence.ServiceType.Herbs, residence.Service_Needed(Residence.ServiceType.Herbs) * herb_ratio, efficency_multiplier);
                }
                market.Input_Storage[Resource.Herbs] -= herbs_used;
                if (market.Input_Storage[Resource.Herbs] < 0.0f) {
                    //Rounding errors?
                    if (market.Input_Storage[Resource.Herbs] < -0.00001f) {
                        CustomLogger.Instance.Error("Negative herbs: {0}");
                    }
                    market.Input_Storage[Resource.Herbs] = 0.0f;
                }
                income += herbs_used * Resource.Herbs.Value;
                market.Update_Delta(Resource.Herbs, (-herbs_used / delta_time) * TimeManager.Instance.Days_To_Seconds(1.0f, 1.0f));
            }

            float salt = market.Input_Storage.ContainsKey(Resource.Salt) ? market.Input_Storage[Resource.Salt] : 0.0f;
            if (salt_needed != 0.0f && salt != 0.0f) {
                float salt_ratio = Math.Min(salt / salt_needed, 1.0f);
                float salt_used = 0.0f;
                foreach (Residence residence in residences) {
                    float salt_for_residence = (residence.Service_Needed(Residence.ServiceType.Salt) * resources_for_full_service) * salt_ratio;
                    salt_used += salt_for_residence;
                    residence.Serve(Residence.ServiceType.Salt, residence.Service_Needed(Residence.ServiceType.Salt) * salt_ratio, efficency_multiplier);
                }
                market.Input_Storage[Resource.Salt] -= salt_used;
                if (market.Input_Storage[Resource.Salt] < 0.0f) {
                    //Rounding errors?
                    if (market.Input_Storage[Resource.Salt] < -0.00001f) {
                        CustomLogger.Instance.Error("Negative salt: {0}");
                    }
                    market.Input_Storage[Resource.Salt] = 0.0f;
                }
                income += salt_used * Resource.Salt.Value;
                market.Update_Delta(Resource.Salt, (-salt_used / delta_time) * TimeManager.Instance.Days_To_Seconds(1.0f, 1.0f));
            }

            float total_clothes = market.Input_Storage[Resource.Simple_Clothes] + market.Input_Storage[Resource.Leather_Clothes];
            if(clothes_needed != 0.0f && total_clothes != 0.0f) {
                float clothing_ratio = Math.Min(total_clothes / clothes_needed, 1.0f);
                float simple_ratio = market.Input_Storage[Resource.Simple_Clothes] / total_clothes;
                float leather_ratio = market.Input_Storage[Resource.Leather_Clothes] / total_clothes;
                float clothing_quality = (simple_ratio * 0.5f) + (leather_ratio * 1.0f);
                float clothing_used = 0.0f;
                foreach (Residence residence in residences) {
                    float clothing_for_residence = (residence.Service_Needed(Residence.ServiceType.Clothes) * resources_for_full_service) * clothing_ratio;
                    clothing_used += clothing_for_residence;
                    residence.Serve(Residence.ServiceType.Clothes, residence.Service_Needed(Residence.ServiceType.Clothes) * clothing_ratio, clothing_quality);
                }
                float simple_sold = simple_ratio * clothing_used;
                market.Input_Storage[Resource.Simple_Clothes] -= simple_sold;
                income += simple_sold * Resource.Simple_Clothes.Value;
                market.Update_Delta(Resource.Simple_Clothes, (-simple_sold / delta_time) * TimeManager.Instance.Days_To_Seconds(1.0f, 1.0f));
                float leather_sold = leather_ratio * clothing_used;
                market.Input_Storage[Resource.Leather_Clothes] -= leather_sold;
                income += leather_sold * Resource.Leather_Clothes.Value;
                market.Update_Delta(Resource.Leather_Clothes, (-leather_sold / delta_time) * TimeManager.Instance.Days_To_Seconds(1.0f, 1.0f));

                if (market.Input_Storage[Resource.Simple_Clothes] < 0.0f) {
                    //Rounding errors?
                    if (market.Input_Storage[Resource.Simple_Clothes] < -0.00001f) {
                        CustomLogger.Instance.Error("Negative simple clothes: {0}");
                    }
                    market.Input_Storage[Resource.Simple_Clothes] = 0.0f;
                }
                if (market.Input_Storage[Resource.Leather_Clothes] < 0.0f) {
                    //Rounding errors?
                    if (market.Input_Storage[Resource.Leather_Clothes] < -0.00001f) {
                        CustomLogger.Instance.Error("Negative leather clothes: {0}");
                    }
                    market.Input_Storage[Resource.Leather_Clothes] = 0.0f;
                }
            }

            if (income != 0.0f) {
                market.Per_Day_Cash_Delta += (income / delta_time) * TimeManager.Instance.Days_To_Seconds(1.0f, 1.0f);
                City.Instance.Add_Cash(income);
            }//                                  v unnecessary list v special settings adds and removes stuff from consumption list MIGHT ACTUALLY BE NECESSARY, DONT REMOVE
        }, null, null, new List<Resource>() { Resource.Berries, Resource.Roots, Resource.Mushrooms, Resource.Herbs, Resource.Firewood, Resource.Charcoal, Resource.Coal, Resource.Game, Resource.Bread, Resource.Potatoes, Resource.Salt, Resource.Mutton, Resource.Corn, Resource.Fish, Resource.Simple_Clothes, Resource.Leather_Clothes }, new List<Resource>(), 0.05f, 5.0f));
        Resource prefered_fuel = Resource.All.Where(x => x.Is_Fuel).OrderByDescending(x => x.Value / x.Fuel_Value).FirstOrDefault();
        foreach(Resource resource in Resource.All) {
            if (resource.Is_Food) {
                prototypes.First(x => x.Internal_Name == "marketplace").Special_Settings.Add(new SpecialSetting(resource.ToString().ToLower(), resource.UI_Name, SpecialSetting.SettingType.Toggle, 0.0f, true));
            } else if (resource.Is_Fuel) {
                prototypes.First(x => x.Internal_Name == "marketplace").Special_Settings.Add(new SpecialSetting(resource.ToString().ToLower(), resource.UI_Name, SpecialSetting.SettingType.Toggle, 0.0f, resource == prefered_fuel));
            }
        }
        prototypes.First(x => x.Internal_Name == "marketplace").Special_Settings.Add(new SpecialSetting(Resource.Herbs.ToString().ToLower(), Resource.Herbs.UI_Name, SpecialSetting.SettingType.Toggle, 0.0f, true));
        prototypes.First(x => x.Internal_Name == "marketplace").Special_Settings.Add(new SpecialSetting(Resource.Salt.ToString().ToLower(), Resource.Salt.UI_Name, SpecialSetting.SettingType.Toggle, 0.0f, true));
        prototypes.First(x => x.Internal_Name == "marketplace").Special_Settings.Add(new SpecialSetting(Resource.Simple_Clothes.ToString().ToLower(), Resource.Simple_Clothes.UI_Name, SpecialSetting.SettingType.Toggle, 0.0f, true));
        prototypes.First(x => x.Internal_Name == "marketplace").Special_Settings.Add(new SpecialSetting(Resource.Leather_Clothes.ToString().ToLower(), Resource.Leather_Clothes.UI_Name, SpecialSetting.SettingType.Toggle, 0.0f, true));

        prototypes.Add(new Building("Hunting Lodge", "hunting_lodge", Building.UI_Category.Forestry, "hunting_lodge", Building.BuildingSize.s2x2, 100, new Dictionary<Resource, int>() {
            { Resource.Wood, 85 }, { Resource.Stone, 10 }, { Resource.Tools, 10 }
        }, 110, new List<Resource>(), 0, 0.0f, 95, new Dictionary<Resource, float>() { { Resource.Wood, 0.05f } }, 0.75f, 0.0f, 0, new Dictionary<Building.Resident, int>() { { Building.Resident.Peasant, 5 } }, 5, true, false, true, 9.0f, 0, Reserve_Tiles(Tile.Work_Type.Hunt),
        delegate (Building building, float delta_time) {
            if (!building.Is_Operational) {
                return;
            }
            float game = 0.0f;
            foreach (Tile tile in building.Get_Tiles_In_Circle(building.Range)) {
                if (tile.Building != null && tile.Building != building) {
                    if (tile.Building.Is_Road && tile.Building.Size == Building.BuildingSize.s1x1) {
                        game -= 0.05f;
                    } else {
                        game -= 1.0f;
                    }
                } else if (tile.Building == null && tile.Can_Work(building, Tile.Work_Type.Hunt)) {
                    if (tile.Internal_Name == "forest") {
                        game += 1.00f;
                    } else if (tile.Internal_Name == "sparse_forest") {
                        game += 0.75f;
                    } else if (tile.Internal_Name == "grass") {
                        game += 0.10f;
                    } else if (tile.Internal_Name == "fertile_ground") {
                        game += 0.125f;
                    } else if (tile.Internal_Name.StartsWith("hill_")) {
                        game += 0.35f;
                    }
                }
            }
            float multiplier = 0.60f;
            game = Mathf.Max((game * 0.1f) * multiplier, 0.0f);
            float hide = game * 0.50f;
            building.Produce(Resource.Game, game, delta_time);
            building.Produce(Resource.Hide, hide, delta_time);
        }, unreserve_tiles, Highlight_Tiles(Tile.Work_Type.Hunt), new List<Resource>(), new List<Resource>() { Resource.Game, Resource.Hide }, 0.0f, 0.0f));

        prototypes.Add(new Building("Quarry", "quarry", Building.UI_Category.Industry, "quarry", Building.BuildingSize.s3x3, 350, new Dictionary<Resource, int>() {
            { Resource.Wood, 65 }, { Resource.Lumber, 80 }, { Resource.Tools, 45 }
        }, 175, new List<Resource>(), 0, 0.0f, 225, new Dictionary<Resource, float>() { { Resource.Wood, 0.10f } }, 1.75f, 0.0f, 0, new Dictionary<Building.Resident, int>() { { Building.Resident.Peasant, 20 } }, 20, true, false, true, 0.0f, 0, null, delegate (Building building, float delta_time) {
            if (!building.Is_Operational) {
                return;
            }
            building.Produce(Resource.Stone, 10.0f, delta_time);
        }, null, null, new List<Resource>(), new List<Resource>() { Resource.Stone }, -0.5f, 7.0f));

        prototypes.Add(new Building("Decorative Tree", "decorative_tree", Building.UI_Category.Services, "decorative_tree", Building.BuildingSize.s1x1, 50, new Dictionary<Resource, int>() {
            { Resource.Stone, 5 }, { Resource.Tools, 1 }
        }, 25, new List<Resource>(), 0, 0.0f, 50, new Dictionary<Resource, float>(), 0.02f, 0.0f, 0, new Dictionary<Building.Resident, int>(), 0, false, false, false, 0.0f, 0, null, null, null, null,
        new List<Resource>(), new List<Resource>(), 0.5f, 3.0f));

        prototypes.Add(new Building("Mine", "mine", Building.UI_Category.Industry, "mine", Building.BuildingSize.s2x2, 200, new Dictionary<Resource, int>() {
            { Resource.Wood, 90 }, { Resource.Lumber, 90 }, { Resource.Stone, 15 }, { Resource.Tools, 40 }
        }, 250, new List<Resource>(), 0, 0.0f, 250, new Dictionary<Resource, float>() { { Resource.Wood, 0.10f } }, 2.00f, 0.0f, 0, new Dictionary<Building.Resident, int>() { { Building.Resident.Peasant, 15 } }, 15, true, false, true, 5.0f, 0, Reserve_Tiles(Tile.Work_Type.Mine), delegate (Building building, float delta_time) {
            if (!building.Is_Operational) {
                return;
            }
            float iron_ore = 0.0f;
            float coal = 0.0f;
            float salt = 0.0f;
            foreach (Tile tile in building.Get_Tiles_In_Circle(building.Range)) {
                if (tile.Can_Work(building, Tile.Work_Type.Mine)) {
                    if (tile.Minerals.ContainsKey(Mineral.Iron)) {
                        iron_ore += tile.Minerals[Mineral.Iron];
                    }
                    if (tile.Minerals.ContainsKey(Mineral.Coal)) {
                        coal += tile.Minerals[Mineral.Coal];
                    }
                    if (tile.Minerals.ContainsKey(Mineral.Salt)) {
                        salt += tile.Minerals[Mineral.Salt];
                    }
                }
            }
            float multiplier = 0.20f;
            building.Produce(Resource.Iron_Ore, iron_ore * multiplier, delta_time);
            building.Produce(Resource.Coal, coal * multiplier, delta_time);
            building.Produce(Resource.Salt, salt * multiplier, delta_time);
        }, unreserve_tiles, Highlight_Tiles(Tile.Work_Type.Mine), new List<Resource>(), new List<Resource>() { Resource.Iron_Ore, Resource.Coal, Resource.Salt }, -0.5f, 5.0f));

        prototypes.Add(new Building("Clay Pit", "clay_pit", Building.UI_Category.Industry, "clay_pit", Building.BuildingSize.s3x3, 100, new Dictionary<Resource, int>() {
            { Resource.Wood, 15 }, { Resource.Lumber, 50 }, { Resource.Stone, 5 }, { Resource.Tools, 10 }
        }, 50, new List<Resource>(), 0, 0.0f, 75, new Dictionary<Resource, float>() { { Resource.Lumber, 0.01f } }, 0.50f, 0.0f, 0, new Dictionary<Building.Resident, int>() { { Building.Resident.Peasant, 10 } }, 10, true, false, true, 0.0f, 0, null, delegate (Building building, float delta_time) {
            if (!building.Is_Operational) {
                return;
            }
            float clay = 0.0f;
            foreach (Tile t in building.Tiles) {
                if (t.Minerals.ContainsKey(Mineral.Clay)) {
                    clay += t.Minerals[Mineral.Clay];
                }
            }
            clay *= 1.75f;
            building.Produce(Resource.Clay, clay, delta_time);
        }, null, null, new List<Resource>(), new List<Resource>() { Resource.Clay }, -0.05f, 2.0f));

        prototypes.Add(new Building("Charcoal Burner", "charcoal_burner", Building.UI_Category.Forestry, "charcoal_burner", Building.BuildingSize.s2x2, 75, new Dictionary<Resource, int>() {
            { Resource.Lumber, 50 }, { Resource.Stone, 5 }, { Resource.Tools, 5 }
        }, 45, new List<Resource>(), 0, 50.0f, 55, new Dictionary<Resource, float>() { { Resource.Lumber, 0.01f } }, 0.50f, 0.0f, 0, new Dictionary<Building.Resident, int>() {
        { Building.Resident.Peasant, 5 } }, 5, true, false, true, 0.0f, 5, null, delegate (Building building, float delta_time) {
            List<Resource> fuel_types = new List<Resource>() { Resource.Firewood, Resource.Charcoal, Resource.Coal };
            Resource selected_fuel = fuel_types[building.Special_Settings.First(x => x.Name == "fuel").Dropdown_Selection];
            foreach(Resource fuel_type in fuel_types) {
                if(fuel_type != selected_fuel && building.Consumes.Contains(fuel_type)) {
                    building.Consumes.Remove(fuel_type);
                }
            }
            if (!building.Consumes.Contains(selected_fuel)) {
                building.Consumes.Add(selected_fuel);
            }
            if (!building.Is_Operational) {
                return;
            }
            float fuel_usage = selected_fuel == Resource.Firewood ? 2.5f : 1.25f;
            building.Process(new Dictionary<Resource, float>() { { Resource.Wood, 5.0f }, { selected_fuel, fuel_usage } }, new Dictionary<Resource, float>() { { Resource.Charcoal, 5.0f } }, delta_time);
        }, null, null, new List<Resource>() { Resource.Wood, Resource.Charcoal, Resource.Coal, Resource.Firewood }, new List<Resource>() { Resource.Charcoal }, -1.50f, 6.0f));
        prototypes.First(x => x.Internal_Name == "charcoal_burner").Special_Settings.Add(new SpecialSetting("fuel", "Fuel", SpecialSetting.SettingType.Dropdown, 0, false, new List<string>() { "Firewood (2.5/day)", "Charcoal (1.25/day)", "Coal (1.25/day)" }, 0));

        prototypes.Add(new Building("Foundry", "foundry", Building.UI_Category.Industry, "foundry", Building.BuildingSize.s2x2, 225, new Dictionary<Resource, int>() {
            { Resource.Lumber, 75 }, { Resource.Stone, 130 }, { Resource.Tools, 20 }
        }, 160, new List<Resource>(), 0, 50.0f, 205, new Dictionary<Resource, float>() { { Resource.Stone, 0.05f }, { Resource.Lumber, 0.01f } }, 1.00f, 0.0f, 0, new Dictionary<Building.Resident, int>() {
        { Building.Resident.Peasant, 10 } }, 10, true, false, true, 0.0f, 7, null, delegate (Building building, float delta_time) {
            List<Resource> fuel_types = new List<Resource>() { Resource.Firewood, Resource.Charcoal, Resource.Coal };
            Resource selected_fuel = fuel_types[building.Special_Settings.First(x => x.Name == "fuel").Dropdown_Selection];
            foreach (Resource fuel_type in fuel_types) {
                if (fuel_type != selected_fuel && building.Consumes.Contains(fuel_type)) {
                    building.Consumes.Remove(fuel_type);
                }
            }
            if (!building.Consumes.Contains(selected_fuel)) {
                building.Consumes.Add(selected_fuel);
            }
            if (!building.Is_Operational) {
                return;
            }
            float fuel_usage = selected_fuel == Resource.Firewood ? 10.0f : 5.0f;
            building.Process(new Dictionary<Resource, float>() { { Resource.Iron_Ore, 20.0f }, { selected_fuel, fuel_usage } }, new Dictionary<Resource, float>() { { Resource.Iron_Bars, 10.0f } }, delta_time);
        }, null, null, new List<Resource>() { Resource.Iron_Ore, Resource.Charcoal, Resource.Coal, Resource.Firewood }, new List<Resource>() { Resource.Iron_Bars }, -1.25f, 6.0f));
        prototypes.First(x => x.Internal_Name == "foundry").Special_Settings.Add(new SpecialSetting("fuel", "Fuel", SpecialSetting.SettingType.Dropdown, 0, false, new List<string>() { "Firewood (10/day)", "Charcoal (5/day)", "Coal (5/day)" }, 0));
        prototypes.First(x => x.Internal_Name == "foundry").Sprites.Add(new SpriteData("foundry_1"));

        prototypes.Add(new Building("Brick Kiln", "brick_kiln", Building.UI_Category.Industry, "brick_kiln", Building.BuildingSize.s2x2, 175, new Dictionary<Resource, int>() {
            { Resource.Lumber, 50 }, { Resource.Stone, 110 }, { Resource.Tools, 15 }
        }, 150, new List<Resource>(), 0, 50.0f, 190, new Dictionary<Resource, float>() { { Resource.Stone, 0.05f }, { Resource.Lumber, 0.01f } }, 1.00f, 0.0f, 0, new Dictionary<Building.Resident, int>() {
        { Building.Resident.Peasant, 10 }, { Building.Resident.Citizen, 5 } }, 10, true, false, true, 0.0f, 7, null, delegate (Building building, float delta_time) {
            List<Resource> fuel_types = new List<Resource>() { Resource.Firewood, Resource.Charcoal, Resource.Coal };
            Resource selected_fuel = fuel_types[building.Special_Settings.First(x => x.Name == "fuel").Dropdown_Selection];
            foreach (Resource fuel_type in fuel_types) {
                if (fuel_type != selected_fuel && building.Consumes.Contains(fuel_type)) {
                    building.Consumes.Remove(fuel_type);
                }
            }
            if (!building.Consumes.Contains(selected_fuel)) {
                building.Consumes.Add(selected_fuel);
            }
            if (!building.Is_Operational) {
                return;
            }
            float fuel_usage = selected_fuel == Resource.Firewood ? 10.0f : 5.0f;
            building.Process(new Dictionary<Resource, float>() { { Resource.Clay, 20.0f }, { selected_fuel, fuel_usage } }, new Dictionary<Resource, float>() { { Resource.Bricks, 10.0f } }, delta_time);
        }, null, null, new List<Resource>() { Resource.Clay, Resource.Charcoal, Resource.Coal, Resource.Firewood }, new List<Resource>() { Resource.Bricks }, -1.10f, 6.0f));
        prototypes.First(x => x.Internal_Name == "brick_kiln").Special_Settings.Add(new SpecialSetting("fuel", "Fuel", SpecialSetting.SettingType.Dropdown, 0, false, new List<string>() { "Firewood (10/day)", "Charcoal (5/day)", "Coal (5/day)" }, 0));

        prototypes.Add(new Building("Smithy", "smithy", Building.UI_Category.Industry, "smithy", Building.BuildingSize.s2x2, 225, new Dictionary<Resource, int>() {
            { Resource.Lumber, 65 }, { Resource.Stone, 100 }, { Resource.Tools, 30 }
        }, 150, new List<Resource>(), 0, 50.0f, 165, new Dictionary<Resource, float>() { { Resource.Stone, 0.05f }, { Resource.Lumber, 0.01f } }, 2.00f, 0.0f, 0, new Dictionary<Building.Resident, int>() {
        { Building.Resident.Peasant, 5 }, { Building.Resident.Citizen, 5 } }, 5, true, false, true, 0.0f, 6, null, delegate (Building building, float delta_time) {
            List<Resource> fuel_types = new List<Resource>() { Resource.Firewood, Resource.Charcoal, Resource.Coal };
            Resource selected_fuel = fuel_types[building.Special_Settings.First(x => x.Name == "fuel").Dropdown_Selection];
            Resource output = building.Special_Settings.First(x => x.Name == "production").Dropdown_Selection == 0 ? Resource.Tools : Resource.Mechanisms;
            foreach (Resource fuel_type in fuel_types) {
                if (fuel_type != selected_fuel && building.Consumes.Contains(fuel_type)) {
                    building.Consumes.Remove(fuel_type);
                }
            }
            if (!building.Consumes.Contains(selected_fuel)) {
                building.Consumes.Add(selected_fuel);
            }
            if(output == Resource.Tools && !building.Consumes.Contains(Resource.Lumber)) {
                building.Consumes.Add(Resource.Lumber);
            } else if (output == Resource.Mechanisms && building.Consumes.Contains(Resource.Lumber)) {
                building.Consumes.Remove(Resource.Lumber);
            }
            if (!building.Is_Operational) {
                return;
            }
            float fuel_usage = selected_fuel == Resource.Firewood ? 2.5f : 1.25f;
            float output_amount = output == Resource.Tools ? 5.0f : 2.5f;
            Dictionary<Resource, float> inputs = new Dictionary<Resource, float>() { { Resource.Iron_Bars, 5.0f }, { selected_fuel, fuel_usage } };
            if(output == Resource.Tools) {
                inputs.Add(Resource.Lumber, 1.0f);
            }
            building.Process(inputs, new Dictionary<Resource, float>() { { output, output_amount } }, delta_time);
        }, null, null, new List<Resource>() { Resource.Iron_Bars, Resource.Lumber, Resource.Charcoal, Resource.Coal, Resource.Firewood }, new List<Resource>() { Resource.Tools, Resource.Mechanisms }, -1.00f, 4.0f));
        prototypes.First(x => x.Internal_Name == "smithy").Special_Settings.Add(new SpecialSetting("production", "Production", SpecialSetting.SettingType.Dropdown, 0.0f, false, new List<string>() { Resource.Tools.UI_Name + " (5/day)", Resource.Mechanisms.UI_Name + " (2.5/day)" }, 0));
        prototypes.First(x => x.Internal_Name == "smithy").Special_Settings.Add(new SpecialSetting("fuel", "Fuel", SpecialSetting.SettingType.Dropdown, 0, false, new List<string>() { Resource.Firewood.UI_Name + " (2.5/day)", Resource.Charcoal.UI_Name + " (1.25/day)", Resource.Coal.UI_Name + " (1.25/day)" }, 0));

        prototypes.Add(new Building("Small Farm", "small_farm", Building.UI_Category.Agriculture, "small_farm", Building.BuildingSize.s2x2, 125, new Dictionary<Resource, int>() {
            { Resource.Lumber, 110 }, { Resource.Stone, 15 }, { Resource.Tools, 10 }
        }, 100, new List<Resource>(), 0, 0.0f, 135, new Dictionary<Resource, float>() { { Resource.Lumber, 0.05f } }, 1.00f, 0.0f, 0, new Dictionary<Building.Resident, int>() { { Building.Resident.Peasant, 10 } }, 10, true, false, true, 5.0f, 0, Reserve_Tiles(Tile.Work_Type.Farm),
        delegate (Building building, float delta_time) {
            if (!building.Is_Operational) {
                return;
            }
            float potatoes = 0.0f;
            foreach (Tile tile in building.Get_Tiles_In_Circle(building.Range)) {
                if (tile.Building != null && tile.Building.Internal_Name == "potato_field" && tile.Can_Work(building, Tile.Work_Type.Farm)) {
                    if (tile.Internal_Name == "grass") {
                        potatoes += 0.10f;
                    } else if (tile.Internal_Name == "fertile_ground") {
                        potatoes += 0.15f;
                    }
                }
            }
            float multiplier = 1.045f;
            building.Produce(Resource.Potatoes, potatoes * multiplier, delta_time);
        }, unreserve_tiles, Highlight_Tiles(Tile.Work_Type.Farm), new List<Resource>(), new List<Resource>() { Resource.Potatoes }, 0.0f, 0.0f));

        prototypes.Add(new Building("Potato Field", "potato_field", Building.UI_Category.Agriculture, "potato_field", Building.BuildingSize.s1x1, 50, new Dictionary<Resource, int>() {
            { Resource.Tools, 1 }
        }, 10, new List<Resource>(), 0, 0.0f, 20, new Dictionary<Resource, float>(), 0.0f, 0.0f, 0, new Dictionary<Building.Resident, int>(), 0, false, false, false, 0.0f, 0, null, null, null, null,
        new List<Resource>(), new List<Resource>(), -0.05f, 3.0f));
        prototypes.First(x => x.Internal_Name == "potato_field").Tags.Add(Building.Tag.Does_Not_Block_Wind);

        prototypes.Add(new Building("Brewery", "brewery", Building.UI_Category.Agriculture, "brewery", Building.BuildingSize.s2x2, 90, new Dictionary<Resource, int>() {
            { Resource.Lumber, 120 }, { Resource.Stone, 15 }, { Resource.Tools, 15 }
        }, 110, new List<Resource>(), 0, 50.0f, 135, new Dictionary<Resource, float>() { { Resource.Lumber, 0.05f } }, 1.00f, 0.0f, 0, new Dictionary<Building.Resident, int>() {
        { Building.Resident.Peasant, 5 }, { Building.Resident.Citizen, 5 } }, 5, true, false, true, 0.0f, 7, null, delegate (Building building, float delta_time) {
            if (!building.Is_Operational) {
                return;
            }
            building.Process(new Dictionary<Resource, float>() { { Resource.Potatoes, 5.0f }, { Resource.Barrels, 2.5f } }, new Dictionary<Resource, float>() { { Resource.Ale, 5.0f } }, delta_time);
        }, null, null, new List<Resource>() { Resource.Potatoes, Resource.Barrels }, new List<Resource>() { Resource.Ale }, 0.0f, 0.0f));

        prototypes.Add(new Building("Tavern", "tavern", Building.UI_Category.Services, "tavern", Building.BuildingSize.s2x2, 175, new Dictionary<Resource, int>() {
            { Resource.Lumber, 175 }, { Resource.Stone, 175 }, { Resource.Tools, 15 }
        }, 205, new List<Resource>(), 0, 0.0f, 350, new Dictionary<Resource, float>() {
            { Resource.Lumber, 0.05f }, { Resource.Stone, 0.05f }
        }, 1.5f, 0.0f, 0.0f, new Dictionary<Building.Resident, int>() {
            { Building.Resident.Peasant, 10 },
            { Building.Resident.Citizen, 10 }
        }, 10, true, false, true, 0.0f, 8, null, delegate (Building tavern, float delta_time) {
            foreach (SpecialSetting setting in tavern.Special_Settings) {
                if (setting.Type == SpecialSetting.SettingType.Toggle) {
                    Resource resource = Resource.All.FirstOrDefault(x => x.Type.ToString().ToLower() == setting.Name);
                    if (resource != null) {
                        if (setting.Toggle_Value && !tavern.Consumes.Contains(resource)) {
                            tavern.Consumes.Add(resource);
                        } else if (!setting.Toggle_Value && tavern.Consumes.Contains(resource)) {
                            tavern.Consumes.Remove(resource);
                        }
                    }
                }
            }
            List<Resource> fuel_types = new List<Resource>() { Resource.Firewood, Resource.Charcoal, Resource.Coal };
            Resource selected_fuel = fuel_types[tavern.Special_Settings.First(x => x.Name == "fuel").Dropdown_Selection];
            foreach (Resource fuel_type in fuel_types) {
                if (fuel_type != selected_fuel && tavern.Consumes.Contains(fuel_type)) {
                    tavern.Consumes.Remove(fuel_type);
                }
            }
            if (!tavern.Consumes.Contains(selected_fuel)) {
                tavern.Consumes.Add(selected_fuel);
            }
            if (!tavern.Is_Operational || tavern.Efficency == 0.0f) {
                return;
            }
            float income = 0.0f;
            List<Residence> residences = new List<Residence>();
            float ale_needed = 0.0f;
            foreach (Building building in tavern.Get_Connected_Buildings(tavern.Road_Range).Select(x => x.Key).ToArray()) {
                if (!(building is Residence)) {
                    continue;
                }
                Residence residence = building as Residence;
                ale_needed += residence.Service_Needed(Residence.ServiceType.Tavern) * Residence.RESOURCES_FOR_FULL_SERVICE;
                residences.Add(residence);
            }
            if (residences.Count == 0 || ale_needed == 0.0f) {
                return;
            }
            
            float fuel = tavern.Input_Storage[selected_fuel];
            float fuel_used_per_day = selected_fuel == Resource.Firewood ? 0.5f : 0.25f;
            tavern.Input_Storage[selected_fuel] = Mathf.Max(0.0f, tavern.Input_Storage[selected_fuel] - Building.Calculate_Actual_Amount(fuel_used_per_day, delta_time));
            tavern.Update_Delta(selected_fuel, -fuel_used_per_day);
            fuel = tavern.Input_Storage[selected_fuel];

            if (ale_needed > 0.0f) {
                float food_needed = ale_needed * 0.1f;
                float total_food = 0.0f;
                float total_ale = tavern.Input_Storage.ContainsKey(Resource.Ale) ? tavern.Input_Storage[Resource.Ale] : 0.0f;
                foreach (KeyValuePair<Resource, float> pair in tavern.Input_Storage) {
                    if (pair.Key.Is_Food) {
                        total_food += pair.Value;
                    }
                }

                if (total_ale > 0.0f && fuel > 0.0f) {
                    float ale_ratio = Math.Min(1.0f, total_ale / ale_needed);
                    float food_ratio = Math.Min(1.0f, total_food / food_needed);
                    float food_used = 0.0f;
                    float ale_used = 0.0f;
                    float unique_food_count = 0.0f;
                    float min_food_ratio = -1.0f;
                    float food_quality = 0.0f;
                    Dictionary<Resource, float> food_ratios = null;
                    if (total_food != 0.0f) {
                        food_ratios = new Dictionary<Resource, float>();
                        foreach (KeyValuePair<Resource, float> pair in tavern.Input_Storage) {
                            if (pair.Key.Is_Food) {
                                float ratio = pair.Value / total_food;
                                food_ratios.Add(pair.Key, ratio);
                                if (pair.Value > 0.0f) {
                                    unique_food_count += pair.Key.Food_Quality;
                                    if (ratio < min_food_ratio || min_food_ratio == -1.0f) {
                                        min_food_ratio = ratio;
                                    }
                                }
                            }
                        }
                        bool has_meat = false;
                        bool has_vegetables = false;
                        foreach (KeyValuePair<Resource, float> pair in tavern.Input_Storage) {
                            if (pair.Key.Is_Food && pair.Value > 0.0f) {
                                if (pair.Key.Food_Type == Resource.FoodType.Meat) {
                                    has_meat = true;
                                } else if (pair.Key.Food_Type == Resource.FoodType.Vegetable) {
                                    has_vegetables = true;
                                }
                            }
                        }
                        float disparity = (1.0f / unique_food_count) - min_food_ratio;
                        food_quality = unique_food_count * (1.0f - disparity);
                        food_quality = (Mathf.Pow(food_quality, 0.5f) + ((food_quality - 1.0f) * 0.1f)) / 4.0f;
                        food_quality *= ((tavern.Efficency + 2.0f) / 3.0f);
                        food_quality *= 1.5f;
                        food_quality = Mathf.Clamp01(food_quality);
                        if (!(has_meat && has_vegetables)) {
                            food_quality *= 0.5f;
                        }
                    }
                    foreach (Residence residence in residences) {
                        float ale_for_residence = (residence.Service_Needed(Residence.ServiceType.Tavern) * Residence.RESOURCES_FOR_FULL_SERVICE) * ale_ratio;
                        ale_used += ale_for_residence;
                        food_used += ((ale_for_residence * 0.1f) * food_ratio);
                        residence.Serve(Residence.ServiceType.Tavern, residence.Service_Needed(Residence.ServiceType.Tavern) * ale_ratio, tavern.Efficency * ((food_quality + 1.0f) / 2.0f));
                    }
                    tavern.Input_Storage[Resource.Ale] -= ale_used;
                    income += Resource.Ale.Value * ale_used;
                    tavern.Update_Delta(Resource.Ale, (-ale_used / delta_time) * TimeManager.Instance.Days_To_Seconds(1.0f, 1.0f), false);
                    if (tavern.Input_Storage[Resource.Ale] < 0.0f) {
                        //Rounding errors?
                        if (tavern.Input_Storage[Resource.Ale] < -0.00001f) {
                            CustomLogger.Instance.Error("Negative ale");
                        }
                        tavern.Input_Storage[Resource.Ale] = 0.0f;
                    }
                    if (food_ratios != null) {
                        foreach (KeyValuePair<Resource, float> pair in food_ratios) {
                            tavern.Input_Storage[pair.Key] -= pair.Value * food_used;
                            tavern.Update_Delta(pair.Key, (-(pair.Value * food_used) / delta_time) * TimeManager.Instance.Days_To_Seconds(1.0f, 1.0f), false);
                            income += pair.Key.Value * (pair.Value * food_used);
                            if (tavern.Input_Storage[pair.Key] < 0.0f) {
                                //Rounding errors?
                                if (tavern.Input_Storage[pair.Key] < -0.00001f) {
                                    CustomLogger.Instance.Error(string.Format("Negative food: {0}", pair.Key.ToString()));
                                }
                                tavern.Input_Storage[pair.Key] = 0.0f;
                            }
                        }
                    }
                } else {
                    tavern.Show_Alert("alert_no_resources");
                }
            }
            if (income != 0.0f) {
                tavern.Per_Day_Cash_Delta += (income / delta_time) * TimeManager.Instance.Days_To_Seconds(1.0f, 1.0f);
                City.Instance.Add_Cash(income);
            }
        }, null, null, new List<Resource>() { Resource.Berries, Resource.Roots, Resource.Mushrooms, Resource.Firewood, Resource.Charcoal, Resource.Coal, Resource.Game, Resource.Bread, Resource.Potatoes, Resource.Ale, Resource.Mutton, Resource.Corn, Resource.Fish }, new List<Resource>(), 0.05f, 3.0f));
        prototypes.First(x => x.Internal_Name == "tavern").Special_Settings.Add(new SpecialSetting("fuel", "Fuel", SpecialSetting.SettingType.Dropdown, 0, false, new List<string>() { "Firewood (0.5/day)", "Charcoal (0.25/day)", "Coal (0.25/day)" }, 0));
        foreach (Resource resource in Resource.All) {
            if (resource.Is_Food) {
                prototypes.First(x => x.Internal_Name == "tavern").Special_Settings.Add(new SpecialSetting(resource.ToString().ToLower(), string.Format("Serve {0}", resource.UI_Name.ToLower()), SpecialSetting.SettingType.Toggle, 0.0f, true));
            }
        }

        prototypes.Add(new Building("Chapel", "chapel", Building.UI_Category.Services, "chapel", Building.BuildingSize.s2x2, 250, new Dictionary<Resource, int>() {
            { Resource.Lumber, 60 }, { Resource.Stone, 175 }, { Resource.Tools, 20 }
        }, 210, new List<Resource>(), 0, 0.0f, 235, new Dictionary<Resource, float>() { { Resource.Stone, 0.05f }, { Resource.Lumber, 0.01f } }, 2.00f, 0.0f, 0, new Dictionary<Building.Resident, int>() {
        { Building.Resident.Citizen, 5 } }, 5, true, false, true, 0.0f, 12, null, delegate (Building chapel, float delta_time) {
            if (!chapel.Is_Operational) {
                return;
            }
            foreach (Building building in chapel.Get_Connected_Buildings(chapel.Road_Range).Select(x => x.Key).ToArray()) {
                if (building is Residence) {
                    (building as Residence).Serve(Residence.ServiceType.Chapel, 1.0f, chapel.Efficency);
                }
            }
        }, null, null, new List<Resource>(), new List<Resource>(), 0.05f, 5.0f));

        prototypes.Add(new Building("Shepherd's Hut", "shepherds_hut", Building.UI_Category.Agriculture, "shepherds_hut", Building.BuildingSize.s2x2, 85, new Dictionary<Resource, int>() {
            { Resource.Wood, 25 }, { Resource.Lumber, 20 }, { Resource.Stone, 10 }, { Resource.Tools, 10 }
        }, 85, new List<Resource>(), 0, 0.0f, 75, new Dictionary<Resource, float>() { { Resource.Wood, 0.01f }, { Resource.Lumber, 0.01f } }, 0.75f, 0.0f, 0, new Dictionary<Building.Resident, int>() { { Building.Resident.Peasant, 5 } }, 5, true, false, true, 4.0f, 0, Reserve_Tiles(Tile.Work_Type.Forage),
        delegate (Building building, float delta_time) {
            if (!building.Is_Operational) {
                return;
            }
            float wool = 0.0f;
            int max_sheep = Mathf.RoundToInt(5.0f * building.Efficency);
            List<Tile> sheep_spawns = new List<Tile>();
            foreach (Tile tile in building.Get_Tiles_In_Circle(building.Range)) {
                if (tile.Can_Work(building, Tile.Work_Type.Forage) && tile.Building == null) {
                    if (tile.Internal_Name == "grass") {
                        wool += 1.00f;
                        if(tile.Entities.Count == 0) {
                            sheep_spawns.Add(tile);
                        }
                    } else if (tile.Internal_Name == "fertile_ground") {
                        wool += 2.00f;
                        if (tile.Entities.Count == 0) {
                            sheep_spawns.Add(tile);
                        }
                    } else if (tile.Internal_Name == "sparse_forest") {
                        wool += 0.50f;
                    } else if (tile.Internal_Name == "forest") {
                        wool += 0.25f;
                    } else if (tile.Internal_Name.StartsWith("hill_")) {
                        wool += 0.85f;
                        if (tile.Entities.Count == 0) {
                            sheep_spawns.Add(tile);
                        }
                    }
                }
            }
            while(sheep_spawns.Count > 0 && building.Entities_Spawned.Count < max_sheep) {
                Tile spawn = RNG.Instance.Item(sheep_spawns);
                Entity sheep = new Entity(EntityPrototypes.Instance.Get("sheep"), spawn, building);
                sheep_spawns.Remove(spawn);
            }
            wool /= 9.0f;
            float mutton = wool * 0.5f;
            float hide = mutton * 0.4f;
            building.Produce(Resource.Wool, wool, delta_time);
            building.Produce(Resource.Mutton, mutton, delta_time);
            building.Produce(Resource.Hide, hide, delta_time);
        }, unreserve_tiles, Highlight_Tiles(Tile.Work_Type.Forage), new List<Resource>(), new List<Resource>() { Resource.Mutton, Resource.Wool, Resource.Hide }, 0.0f, 0.0f));

        prototypes.Add(new Residence("Homestead", "homestead", Building.UI_Category.Housing, "homestead", Building.BuildingSize.s3x3, 200, new Dictionary<Resource, int>() {
            { Resource.Wood, 100 }, { Resource.Lumber, 100 }, { Resource.Stone, 20 }, { Resource.Tools, 15 }
        }, 175, new List<Resource>(), 0, 220, new Dictionary<Resource, float>() { { Resource.Wood, 0.05f }, { Resource.Lumber, 0.05f } }, 2.25f, 0.0f, 0.0f, 0.05f, new Dictionary<Building.Resident, int>() { { Building.Resident.Peasant, 15 } }, 3.75f,
        delegate (Building building) {
            foreach (Tile tile in building.Get_Tiles_In_Circle(building.Range)) {
                tile.Add_Workers(building, Tile.Work_Type.Cut_Wood);
                tile.Add_Workers(building, Tile.Work_Type.Farm);
                tile.Add_Workers(building, Tile.Work_Type.Fish);
            }
        },
        delegate (Building building, float delta_time) {
            if (!building.Is_Operational) {
                return;
            }
            float efficency = (building as Residence).Current_Residents[Building.Resident.Peasant] / (float)(building as Residence).Resident_Space[Building.Resident.Peasant];
            if(efficency == 0.0f) {
                return;
            }
            float potatoes = 0.0f;
            float wood = 0.0f;
            float fish = 0.0f;
            foreach (Tile tile in building.Get_Tiles_In_Circle(building.Range)) {
                if (tile.Building != null && tile.Building.Internal_Name == "potato_field" && tile.Can_Work(building, Tile.Work_Type.Farm)) {
                    if (tile.Internal_Name == "grass") {
                        potatoes += 0.10f;
                    } else if (tile.Internal_Name == "fertile_ground") {
                        potatoes += 0.15f;
                    }
                }
                if (tile.Can_Work(building, Tile.Work_Type.Cut_Wood) && tile.Building == null) {
                    if (tile.Internal_Name == "forest") {
                        wood += 1.75f;
                    } else if (tile.Internal_Name == "sparse_forest") {
                        wood += 0.75f;
                    } else if (tile.Internal_Name.StartsWith("hill_")) {
                        wood += 0.25f;
                    }
                }
                if (tile.Can_Work(building, Tile.Work_Type.Fish) && tile.Internal_Name.StartsWith("water")) {
                    fish += 0.10f;
                }
            }

            potatoes *= efficency;
            wood *= efficency;
            fish *= efficency;

            float potato_multiplier = 0.883f;
            float wood_multiplier = 0.094f;
            float fish_multiplier = 0.90f;
            building.Produce(Resource.Potatoes, potatoes * potato_multiplier, delta_time);
            building.Produce(Resource.Wood, wood * wood_multiplier, delta_time);
            building.Produce(Resource.Fish, fish * fish_multiplier, delta_time);
        }, unreserve_tiles, Highlight_Tiles(Tile.Work_Type.Forage), new List<Resource>(), new List<Resource>() { Resource.Potatoes, Resource.Fish, Resource.Wood }, 0.0f, 0.0f));

        prototypes.Add(new Residence("Abode", "abode", Building.UI_Category.Housing, "abode", Building.BuildingSize.s2x2, 100, new Dictionary<Resource, int>() {
            { Resource.Lumber, 50 }, { Resource.Stone, 75 }, { Resource.Tools, 10 }
        }, 150, new List<Resource>(), 0, 125, new Dictionary<Resource, float>() { { Resource.Lumber, 0.025f }, { Resource.Stone, 0.025f } }, 0.01f, 0.0f, 0.0f, 0.0f, new Dictionary<Building.Resident, int>() { { Building.Resident.Citizen, 5 } },
        0.0f, null, null, null, null, new List<Resource>(), new List<Resource>(), 0.0f, 0.0f));
        prototypes.First(x => x.Internal_Name == "abode").Sprites.Add(new SpriteData("abode_1"));

        prototypes.Add(new Residence("Townhouse", "townhouse", Building.UI_Category.Housing, "townhouse", Building.BuildingSize.s2x2, 200, new Dictionary<Resource, int>() {
            { Resource.Bricks, 200 }, { Resource.Lumber, 85 }, { Resource.Stone, 20 }, { Resource.Tools, 15 }
        }, 200, new List<Resource>(), 0, 305, new Dictionary<Resource, float>() { { Resource.Bricks, 0.05f }, { Resource.Lumber, 0.05f } }, 0.15f, 0.0f, 0.0f, -0.1f, new Dictionary<Building.Resident, int>() { { Building.Resident.Citizen, 20 } },
        0.0f, null, null, null, null, new List<Resource>(), new List<Resource>(), 0.0f, 0.0f));
        prototypes.First(x => x.Internal_Name == "townhouse").Sprites.Add(new SpriteData("townhouse_1"));
        prototypes.First(x => x.Internal_Name == "townhouse").Sprites.Add(new SpriteData("townhouse_2"));
        prototypes.First(x => x.Internal_Name == "townhouse").Sprites.Add(new SpriteData("townhouse_3"));

        prototypes.Add(new Building("Tax Office", "tax_office", Building.UI_Category.Admin, "tax_office", Building.BuildingSize.s2x2, 150, new Dictionary<Resource, int>() {
            { Resource.Lumber, 90 }, { Resource.Stone, 90 }, { Resource.Tools, 10 }
        }, 225, new List<Resource>(), 0, 0.0f, 235, new Dictionary<Resource, float>() { { Resource.Stone, 0.025f }, { Resource.Lumber, 0.025f } }, 1.50f, 0.0f, 0, new Dictionary<Building.Resident, int>() {
        { Building.Resident.Citizen, 5 } }, 5, true, false, true, 0.0f, 15, null, delegate (Building office, float delta_time) {
            if (!office.Is_Operational) {
                return;
            }
            int tax_rate = office.Special_Settings.First(x => x.Name == "tax_rate").Dropdown_Selection;
            float tax_rate_multiplier = 1.0f;
            switch (tax_rate) {
                case 0: //Very low
                    tax_rate_multiplier = 0.1f;
                    break;
                case 1: //Low
                    tax_rate_multiplier = 0.25f;
                    break;
                case 2: //Medium
                    tax_rate_multiplier = 0.5f;
                    break;
                case 3: //High
                    tax_rate_multiplier = 0.75f;
                    break;
                case 4: //Very high
                    tax_rate_multiplier = 1.0f;
                    break;
            }
            float income = 0.0f;
            Dictionary<Building.Resident, float> base_tax_income = new Dictionary<Building.Resident, float>() {
                { Building.Resident.Peasant, 0.035f },
                { Building.Resident.Citizen, 0.05f },
                { Building.Resident.Noble, 0.15f }
            };
            foreach (Building building in office.Get_Connected_Buildings(office.Road_Range).Select(x => x.Key).ToArray()) {
                if (building is Residence) {
                    (building as Residence).Serve(Residence.ServiceType.Taxes, 1.0f, tax_rate_multiplier);
                    foreach(Building.Resident resident in Enum.GetValues(typeof(Building.Resident))) {
                        income += (building as Residence).Current_Residents[resident] * base_tax_income[resident] * tax_rate_multiplier;
                    }
                }
            }
            if (income != 0.0f) {
                income *= office.Efficency;
                office.Per_Day_Cash_Delta += (income / delta_time) * TimeManager.Instance.Days_To_Seconds(1.0f, 1.0f);
                City.Instance.Add_Cash(income);
            }
        }, null, null, new List<Resource>(), new List<Resource>(), 0.0f, 0.0f));
        prototypes.First(x => x.Internal_Name == "tax_office").Special_Settings.Add(new SpecialSetting("tax_rate", "Tax rate", SpecialSetting.SettingType.Dropdown, 0.0f, false, new List<string>() { "Very low", "Low", "Medium", "High", "Very high" }, 2));
        prototypes.First(x => x.Internal_Name == "tax_office").Sprites.Add(new SpriteData("tax_office_1"));

        prototypes.Add(new Building("Weaver's Workshop", "weavers_workshop", Building.UI_Category.Textile, "weavers_workshop", Building.BuildingSize.s2x2, 100, new Dictionary<Resource, int>() {
            { Resource.Lumber, 80 }, { Resource.Stone, 10 }, { Resource.Tools, 10 }
        }, 90, new List<Resource>(), 0, 50.0f, 90, new Dictionary<Resource, float>() { { Resource.Lumber, 0.05f } }, 1.00f, 0.0f, 0, new Dictionary<Building.Resident, int>() {
        { Building.Resident.Peasant, 5 }, { Building.Resident.Citizen, 5 } }, 5, true, false, true, 0.0f, 7, null, delegate (Building building, float delta_time) {
            if (!building.Is_Operational) {
                return;
            }
            Resource output = building.Special_Settings.First(x => x.Name == "output").Dropdown_Selection == 0 ? Resource.Cloth : Resource.Thread;
            float output_amount = building.Special_Settings.First(x => x.Name == "output").Dropdown_Selection == 0 ? 2.5f : 5.0f;
            building.Process(Resource.Wool, 5.0f, output, output_amount, delta_time);
        }, null, null, new List<Resource>() { Resource.Wool }, new List<Resource>() { Resource.Cloth, Resource.Thread }, 0.0f, 0.0f));
        prototypes.First(x => x.Internal_Name == "weavers_workshop").Special_Settings.Add(new SpecialSetting("output", "Production", SpecialSetting.SettingType.Dropdown, 0, false, new List<string>() { Resource.Cloth.UI_Name + " (2.5/day)", Resource.Thread.UI_Name + " (5/day)" }, 0));
        prototypes.First(x => x.Internal_Name == "weavers_workshop").Sprites.Add(new SpriteData("weavers_workshop_1"));

        prototypes.Add(new Building("Tannery", "tannery", Building.UI_Category.Textile, "tannery", Building.BuildingSize.s2x2, 100, new Dictionary<Resource, int>() {
            { Resource.Lumber, 75 }, { Resource.Stone, 10 }, { Resource.Tools, 10 }
        }, 85, new List<Resource>(), 0, 50.0f, 85, new Dictionary<Resource, float>() { { Resource.Lumber, 0.025f } }, 0.75f, 0.0f, 0, new Dictionary<Building.Resident, int>() {
        { Building.Resident.Peasant, 5 } }, 5, true, false, true, 0.0f, 7, null, delegate (Building building, float delta_time) {
            if (!building.Is_Operational) {
                return;
            }
            building.Process(Resource.Hide, 5.0f, Resource.Leather, 5.0f , delta_time);
        }, null, null, new List<Resource>() { Resource.Hide }, new List<Resource>() { Resource.Leather }, -1.25f, 6.0f));
        prototypes.First(x => x.Internal_Name == "tannery").Sprites.Add(new SpriteData("tannery_1"));

        prototypes.Add(new Building("Workshop", "workshop", Building.UI_Category.Industry, "workshop", Building.BuildingSize.s2x2, 185, new Dictionary<Resource, int>() {
            { Resource.Lumber, 200 }, { Resource.Stone, 25 }, { Resource.Tools, 25 }
        }, 130, new List<Resource>(), 0, 50.0f, 225, new Dictionary<Resource, float>() { { Resource.Lumber, 0.05f } }, 1.00f, 1.0f, 15.0f, new Dictionary<Building.Resident, int>() {
        { Building.Resident.Peasant, 10 }, { Building.Resident.Citizen, 10 } }, 10, true, false, true, 0.0f, 7, null, delegate (Building building, float delta_time) {
            List<Resource> fuel_types = new List<Resource>() { Resource.Firewood, Resource.Charcoal, Resource.Coal };
            Resource selected_fuel = fuel_types[building.Special_Settings.First(x => x.Name == "fuel").Dropdown_Selection];
            int production_selection = building.Special_Settings.First(x => x.Name == "production").Dropdown_Selection;
            Dictionary<Resource, float> inputs = new Dictionary<Resource, float>();
            Dictionary<Resource, float> outputs = new Dictionary<Resource, float>();
            switch (production_selection) {
                case 0:
                    inputs.Add(Resource.Lumber, 5.0f);
                    outputs.Add(Resource.Barrels, 10.0f);
                    break;
                case 1:
                    inputs.Add(Resource.Iron_Bars, 10.0f);
                    float fuel_amount = selected_fuel == Resource.Firewood ? 5.0f : 2.5f;
                    inputs.Add(selected_fuel, fuel_amount);
                    outputs.Add(Resource.Mechanisms, 5.0f);
                    break;
                case 2:
                    inputs.Add(Resource.Wood, 5.0f);
                    outputs.Add(Resource.Firewood, 5.0f);
                    break;
                case 3:
                    inputs.Add(Resource.Lumber, 5.0f);
                    outputs.Add(Resource.Firewood, 5.0f);
                    break;
                case 4:
                    inputs.Add(Resource.Wood, 5.0f);
                    outputs.Add(Resource.Lumber, 2.5f);
                    break;
            }
            building.Consumes.Clear();
            building.Produces.Clear();
            foreach(KeyValuePair<Resource, float> pair in inputs) {
                building.Consumes.Add(pair.Key);
            }
            foreach (KeyValuePair<Resource, float> pair in outputs) {
                building.Produces.Add(pair.Key);
            }
            foreach(KeyValuePair<Resource, float> pair in building.Output_Storage) {
                building.Produces.Add(pair.Key);
            }
            if (!building.Is_Operational) {
                return;
            }
            building.Process(inputs, outputs, delta_time);
        }, null, null, new List<Resource>() { Resource.Wood, Resource.Lumber, Resource.Iron_Bars, Resource.Firewood, Resource.Charcoal, Resource.Coal }, new List<Resource>() { Resource.Barrels, Resource.Lumber, Resource.Firewood, Resource.Mechanisms }, -0.10f, 4.0f));
        prototypes.First(x => x.Internal_Name == "workshop").Special_Settings.Add(new SpecialSetting("production", "Production", SpecialSetting.SettingType.Dropdown, 0.0f, false, new List<string>() {
            Resource.Barrels.UI_Name + " (10/day)", Resource.Mechanisms.UI_Name + " (5/day)", Resource.Firewood.UI_Name + " (w) (5/day)", Resource.Firewood.UI_Name + " (l) (5/day)", Resource.Lumber + " (2.5/day)" }, 0));
        prototypes.First(x => x.Internal_Name == "workshop").Special_Settings.Add(new SpecialSetting("fuel", "Fuel (" + Resource.Mechanisms.UI_Name.Substring(0, 4) + ")", SpecialSetting.SettingType.Dropdown, 0, false, new List<string>() { Resource.Firewood.UI_Name + " (2.5/day)", Resource.Charcoal.UI_Name + " (1.25/day)", Resource.Coal.UI_Name + " (1.25/day)" }, 0));

        prototypes.Add(new Building("Tailor's Shop", "tailors_shop", Building.UI_Category.Textile, "tailors_shop", Building.BuildingSize.s2x2, 100, new Dictionary<Resource, int>() {
            { Resource.Lumber, 75 }, { Resource.Stone, 30 }, { Resource.Tools, 15 }
        }, 150, new List<Resource>(), 0, 50.0f, 100, new Dictionary<Resource, float>() { { Resource.Lumber, 0.05f } }, 1.25f, 0.0f, 0, new Dictionary<Building.Resident, int>() {
        { Building.Resident.Peasant, 5 }, { Building.Resident.Citizen, 5 } }, 5, true, false, true, 0.0f, 6, null, delegate (Building building, float delta_time) {
            if (!building.Is_Operational) {
                return;
            }
            int production_selection = building.Special_Settings.First(x => x.Name == "production").Dropdown_Selection;
            Resource output = null;
            float output_amount = 0.0f;
            Dictionary<Resource, float> inputs = new Dictionary<Resource, float>();
            switch (production_selection) {
                case 0:
                    output = Resource.Simple_Clothes;
                    output_amount = 2.5f;
                    inputs.Add(Resource.Cloth, 2.5f);
                    inputs.Add(Resource.Thread, 1.5f);
                    break;
                case 1:
                    output = Resource.Leather_Clothes;
                    output_amount = 2.5f;
                    inputs.Add(Resource.Cloth, 2.5f);
                    inputs.Add(Resource.Thread, 1.5f);
                    inputs.Add(Resource.Leather, 2.5f);
                    break;
            }
            building.Process(inputs, new Dictionary<Resource, float>() { { output, output_amount } }, delta_time);
        }, null, null, new List<Resource>() { Resource.Cloth, Resource.Thread, Resource.Leather }, new List<Resource>() { Resource.Simple_Clothes, Resource.Leather_Clothes }, 0.0f, 0.0f));
        prototypes.First(x => x.Internal_Name == "tailors_shop").Sprites.Add(new SpriteData("tailors_shop_1"));
        prototypes.First(x => x.Internal_Name == "tailors_shop").Special_Settings.Add(new SpecialSetting("production", "Production", SpecialSetting.SettingType.Dropdown, 0, false, new List<string>() { "Simple clothes", "Leather clothes" }, 0));

        prototypes.Add(new Building("Farmhouse", "farmhouse", Building.UI_Category.Agriculture, "farmhouse", Building.BuildingSize.s3x3, 200, new Dictionary<Resource, int>() {
            { Resource.Lumber, 220 }, { Resource.Stone, 20 }, { Resource.Tools, 20 }
        }, 190, new List<Resource>(), 0, 0.0f, 240, new Dictionary<Resource, float>() { { Resource.Lumber, 0.05f } }, 1.00f, 0.0f, 0, new Dictionary<Building.Resident, int>() { { Building.Resident.Peasant, 15 } }, 15, true, false, true, 6.5f, 0, Reserve_Tiles(Tile.Work_Type.Farm),
        delegate (Building building, float delta_time) {
            if (!building.Is_Operational) {
                return;
            }
            float potatoes = 0.0f;
            float wheat = 0.0f;
            float corn = 0.0f;
            foreach (Tile tile in building.Get_Tiles_In_Circle(building.Range)) {
                if (tile.Building != null && tile.Can_Work(building, Tile.Work_Type.Farm)) {
                    if (tile.Building.Internal_Name == "potato_field") {
                        if (tile.Internal_Name == "grass") {
                            potatoes += 0.10f;
                        } else if (tile.Internal_Name == "fertile_ground") {
                            potatoes += 0.15f;
                        }
                    } else if(tile.Building.Internal_Name == "wheat_field") {
                        if (tile.Internal_Name == "grass") {
                            wheat += 0.10f;
                        } else if (tile.Internal_Name == "fertile_ground") {
                            wheat += 0.15f;
                        }
                    } else if (tile.Building.Internal_Name == "corn_field") {
                        if (tile.Internal_Name == "grass") {
                            corn += 0.135f;
                        } else if (tile.Internal_Name == "fertile_ground") {
                            corn += 0.2025f;
                        }
                    }
                }
            }
            float multiplier = 0.813f;
            building.Produce(Resource.Potatoes, potatoes * multiplier, delta_time);
            building.Produce(Resource.Wheat, wheat * multiplier, delta_time);
            building.Produce(Resource.Corn, corn * multiplier, delta_time);
        }, unreserve_tiles, Highlight_Tiles(Tile.Work_Type.Farm), new List<Resource>(), new List<Resource>() { Resource.Potatoes, Resource.Wheat, Resource.Corn }, 0.0f, 0.0f));

        prototypes.Add(new Building("Wheat Field", "wheat_field", Building.UI_Category.Agriculture, "wheat_field", Building.BuildingSize.s1x1, 50, new Dictionary<Resource, int>() {
            { Resource.Tools, 1 }
        }, 10, new List<Resource>(), 0, 0.0f, 25, new Dictionary<Resource, float>(), 0.0f, 0.0f, 0, new Dictionary<Building.Resident, int>(), 0, false, false, false, 0.0f, 0, null, null, null, null,
        new List<Resource>(), new List<Resource>(), 0.0f, 0.0f));
        prototypes.First(x => x.Internal_Name == "wheat_field").Tags.Add(Building.Tag.Does_Not_Block_Wind);

        prototypes.Add(new Building("Corn Field", "corn_field", Building.UI_Category.Agriculture, "corn_field", Building.BuildingSize.s1x1, 50, new Dictionary<Resource, int>() {
            { Resource.Tools, 1 }
        }, 10, new List<Resource>(), 0, 0.0f, 25, new Dictionary<Resource, float>(), 0.0f, 0.0f, 0, new Dictionary<Building.Resident, int>(), 0, false, false, false, 0.0f, 0, null, null, null, null,
        new List<Resource>(), new List<Resource>(), 0.0f, 0.0f));
        prototypes.First(x => x.Internal_Name == "corn_field").Tags.Add(Building.Tag.Does_Not_Block_Wind);

        prototypes.Add(new Building("Silo", "silo", Building.UI_Category.Infrastructure, "silo", Building.BuildingSize.s2x2, 150, new Dictionary<Resource, int>() {
            { Resource.Lumber, 75 }, { Resource.Stone, 110 }, { Resource.Tools, 15 }
        }, 100, new List<Resource>() { Resource.Wheat, Resource.Flour, Resource.Corn },
        2500, 75.0f, 190, new Dictionary<Resource, float>() { { Resource.Stone, 0.025f }, { Resource.Lumber, 0.01f } }, 0.5f, 0.0f, 0.0f, new Dictionary<Building.Resident, int>() { { Building.Resident.Peasant, 5 } }, 5, false, false, true, 0.0f, 17, null, null, null, null, new List<Resource>(), new List<Resource>(), 0.0f, 0.0f));
        prototypes.First(x => x.Internal_Name == "silo").Sprites.Add(new SpriteData("silo_1"));

        prototypes.Add(new Building("Windmill", "windmill", Building.UI_Category.Agriculture, "windmill", Building.BuildingSize.s2x2, 160, new Dictionary<Resource, int>() {
            { Resource.Lumber, 65 }, { Resource.Stone, 130 }, { Resource.Cloth, 20 }, { Resource.Mechanisms, 10 }, { Resource.Tools, 30 }
        }, 180, new List<Resource>(), 0, 50.0f, 300, new Dictionary<Resource, float>() { { Resource.Stone, 0.05f }, { Resource.Lumber, 0.01f } }, 1.50f, 0.0f, 0, new Dictionary<Building.Resident, int>() {
            { Building.Resident.Peasant, 5 }, { Building.Resident.Citizen, 5 } }, 5, true, false, true, 3.0f, 8, null, delegate (Building building, float delta_time) {
                if (!building.Is_Operational) {
                    return;
                }
                float blocked_tiles = 0.0f;
                float total_tiles = 0.0f;
                foreach (Tile tile in building.Get_Tiles_In_Circle(building.Range)) {
                    total_tiles += 1.0f;
                    if(tile.Internal_Name == "forest" || tile.Internal_Name.StartsWith("hill_") || (tile.Building != null && tile.Building != building && !tile.Building.Tags.Contains(Building.Tag.Does_Not_Block_Wind))) {
                        blocked_tiles += 1.0f;
                    } else if(tile.Internal_Name == "sparse_forest") {
                        blocked_tiles += 0.5f;
                    }
                }
                float efficency = 1.0f - (blocked_tiles / total_tiles);
                building.Process(Resource.Wheat, 20.0f * efficency, Resource.Flour, 20.0f * efficency, delta_time);
            }, null, null, new List<Resource>() { Resource.Wheat }, new List<Resource>() { Resource.Flour }, 0.0f, 0.0f));
        prototypes.First(x => x.Internal_Name == "windmill").Sprite.Animation_Frame_Time = 0.5f;
        prototypes.First(x => x.Internal_Name == "windmill").Sprite.Animation_Sprites = new List<string>() { "windmill", "windmill_1" };

        prototypes.Add(new Building("Watermill", "watermill", Building.UI_Category.Agriculture, "watermill", Building.BuildingSize.s2x2, 200, new Dictionary<Resource, int>() {
            { Resource.Lumber, 90 }, { Resource.Bricks, 180 }, { Resource.Stone, 50 }, { Resource.Mechanisms, 15 }, { Resource.Tools, 35 }
        }, 250, new List<Resource>(), 0, 50.0f, 365, new Dictionary<Resource, float>() { { Resource.Bricks, 0.05f }, { Resource.Lumber, 0.02f } }, 1.75f, 0.0f, 0, new Dictionary<Building.Resident, int>() {
            { Building.Resident.Peasant, 5 }, { Building.Resident.Citizen, 5 } }, 5, true, false, true, 0.0f, 8,
            delegate(Building building) {
                List<Tile> wheel_tiles = new List<Tile>();
                string wheel_prototype = null;
                switch (building.Selected_Sprite) {
                    case 0:
                        wheel_tiles = Map.Instance.Get_Tiles(building.Tile.Coordinates.Shift(new Coordinates(2, 0)), 2, 2);
                        wheel_prototype = "waterwheel";
                        break;
                    case 1:
                        wheel_tiles = Map.Instance.Get_Tiles(building.Tile.Coordinates.Shift(new Coordinates(-2, 0)), 2, 2);
                        wheel_prototype = "waterwheel_flip";
                        break;
                    case 2:
                        wheel_tiles = Map.Instance.Get_Tiles(building.Tile.Coordinates.Shift(new Coordinates(0, -2)), 2, 2);
                        wheel_prototype = "waterwheel_horizontal";
                        break;
                    case 3:
                        wheel_tiles = Map.Instance.Get_Tiles(building.Tile.Coordinates.Shift(new Coordinates(0, 2)), 2, 2);
                        wheel_prototype = "waterwheel_horizontal_flip";
                        break;
                }
                bool blocked = false;
                foreach(Tile t in wheel_tiles) {
                    if(t.Building != null || !t.Is_Water) {
                        blocked = true;
                        break;
                    }
                }
                if (!blocked) {
                    Building wheel = new Building(Get(wheel_prototype), wheel_tiles[0], wheel_tiles, false);
                    City.Instance.Add_Building(wheel);
                }
            }, delegate (Building building, float delta_time) {
                if (!building.Is_Operational) {
                    return;
                }
                Building wheel = null;
                switch (building.Selected_Sprite) {
                    case 0:
                        wheel = Map.Instance.Get_Tiles(building.Tile.Coordinates.Shift(new Coordinates(2, 0)), 2, 2)[0].Building;
                        break;
                    case 1:
                        wheel = Map.Instance.Get_Tiles(building.Tile.Coordinates.Shift(new Coordinates(-2, 0)), 2, 2)[0].Building;
                        break;
                    case 2:
                        wheel = Map.Instance.Get_Tiles(building.Tile.Coordinates.Shift(new Coordinates(0, -2)), 2, 2)[0].Building;
                        break;
                    case 3:
                        wheel = Map.Instance.Get_Tiles(building.Tile.Coordinates.Shift(new Coordinates(0, 2)), 2, 2)[0].Building;
                        break;
                }
                if (wheel == null) {
                    building.Show_Alert("alert_general");
                    return;
                }
                building.Process(Resource.Wheat, 30.0f, Resource.Flour, 30.0f, delta_time);
            }, null, delegate(Building building) {
                List<Tile> wheel_tiles = new List<Tile>();
                switch (building.Selected_Sprite) {
                    case 0:
                        wheel_tiles = Map.Instance.Get_Tiles(building.Tile.Coordinates.Shift(new Coordinates(2, 0)), 2, 2);
                        break;
                    case 1:
                        wheel_tiles = Map.Instance.Get_Tiles(building.Tile.Coordinates.Shift(new Coordinates(-2, 0)), 2, 2);
                        break;
                    case 2:
                        wheel_tiles = Map.Instance.Get_Tiles(building.Tile.Coordinates.Shift(new Coordinates(0, -2)), 2, 2);
                        break;
                    case 3:
                        wheel_tiles = Map.Instance.Get_Tiles(building.Tile.Coordinates.Shift(new Coordinates(0, 2)), 2, 2);
                        break;
                }
                if(wheel_tiles.Count == 0) {
                    CustomLogger.Instance.Warning("No wheel tiles");
                }
                return wheel_tiles;
            }, new List<Resource>() { Resource.Wheat }, new List<Resource>() { Resource.Flour }, 0.0f, 0.0f));
        prototypes.First(x => x.Internal_Name == "watermill").Sprites.Add(new SpriteData("watermill_1"));
        prototypes.First(x => x.Internal_Name == "watermill").Sprites.Add(new SpriteData("watermill_2"));
        prototypes.First(x => x.Internal_Name == "watermill").Sprites.Add(new SpriteData("watermill_3"));
        prototypes.First(x => x.Internal_Name == "watermill").On_Build_Check = delegate (Building building, Tile tile, out string message) {
            message = string.Empty;
            List<Tile> wheel_tiles = new List<Tile>();
            switch (building.Selected_Sprite) {
                case 0:
                    wheel_tiles = Map.Instance.Get_Tiles(tile.Coordinates.Shift(new Coordinates(2, 0)), 2, 2);
                    break;
                case 1:
                    wheel_tiles = Map.Instance.Get_Tiles(tile.Coordinates.Shift(new Coordinates(-2, 0)), 2, 2);
                    break;
                case 2:
                    wheel_tiles = Map.Instance.Get_Tiles(tile.Coordinates.Shift(new Coordinates(0, -2)), 2, 2);
                    break;
                case 3:
                    wheel_tiles = Map.Instance.Get_Tiles(tile.Coordinates.Shift(new Coordinates(0, 2)), 2, 2);
                    break;
            }
            if (wheel_tiles.Count < 4) {
                message = "Nor enough space foe waterwheel";
                return false;
            }
            foreach (Tile wheel_tile in wheel_tiles) {
                if (!wheel_tile.Is_Water) {
                    message = "Nor enough space foe waterwheel";
                    return false;
                }
            }
            int north_flow = 0;
            int east_flow = 0;
            int south_flow = 0;
            int west_flow = 0;
            foreach(Tile wheel_tile in wheel_tiles) {
                if (wheel_tile.Water_Flow == Coordinates.Direction.North || wheel_tile.Water_Flow == Coordinates.Direction.North_East || wheel_tile.Water_Flow == Coordinates.Direction.North_West) {
                    north_flow++;
                } else if(wheel_tile.Water_Flow == Coordinates.Direction.East || wheel_tile.Water_Flow == Coordinates.Direction.North_East || wheel_tile.Water_Flow == Coordinates.Direction.South_East) {
                    east_flow++;
                } else if(wheel_tile.Water_Flow == Coordinates.Direction.South || wheel_tile.Water_Flow == Coordinates.Direction.South_East || wheel_tile.Water_Flow == Coordinates.Direction.South_West) {
                    south_flow++;
                } else if (wheel_tile.Water_Flow == Coordinates.Direction.West || wheel_tile.Water_Flow == Coordinates.Direction.South_West || wheel_tile.Water_Flow == Coordinates.Direction.North_West) {
                    west_flow++;
                }
            }
            int total_flow = north_flow + east_flow + south_flow + west_flow;
            if(total_flow < 3) {
                message = "Not enough water flow";
                return false;
            }
            return true;
        };

        prototypes.Add(new Building("Waterwheel", "waterwheel", Building.UI_Category.Unbuildable, "waterwheel", Building.BuildingSize.s2x2, 50, new Dictionary<Resource, int>(), 0, new List<Resource>(), 0, 0.0f, 0, new Dictionary<Resource, float>(), 0.0f, 0.0f, 0, new Dictionary<Building.Resident, int>(), 0, false, false, false, 0.0f, 0, null, null, null, null, new List<Resource>(), new List<Resource>(), 0.0f, 0.0f));
        prototypes.First(x => x.Internal_Name == "waterwheel").Sprite.Animation_Frame_Time = 0.10f;
        prototypes.First(x => x.Internal_Name == "waterwheel").Sprite.Animation_Sprites = new List<string>() { "waterwheel", "waterwheel_1", "waterwheel_2" };

        prototypes.Add(new Building("Waterwheel", "waterwheel_flip", Building.UI_Category.Unbuildable, "waterwheel_flip", Building.BuildingSize.s2x2, 50, new Dictionary<Resource, int>(), 0, new List<Resource>(), 0, 0.0f, 0, new Dictionary<Resource, float>(), 0.0f, 0.0f, 0, new Dictionary<Building.Resident, int>(), 0, false, false, false, 0.0f, 0, null, null, null, null, new List<Resource>(), new List<Resource>(), 0.0f, 0.0f));
        prototypes.First(x => x.Internal_Name == "waterwheel_flip").Sprite.Animation_Frame_Time = 0.10f;
        prototypes.First(x => x.Internal_Name == "waterwheel_flip").Sprite.Animation_Sprites = new List<string>() { "waterwheel_flip", "waterwheel_1_flip", "waterwheel_2_flip" };

        prototypes.Add(new Building("Waterwheel", "waterwheel_horizontal", Building.UI_Category.Unbuildable, "waterwheel_3", Building.BuildingSize.s2x2, 50, new Dictionary<Resource, int>(), 0, new List<Resource>(), 0, 0.0f, 0, new Dictionary<Resource, float>(), 0.0f, 0.0f, 0, new Dictionary<Building.Resident, int>(), 0, false, false, false, 0.0f, 0, null, null, null, null, new List<Resource>(), new List<Resource>(), 0.0f, 0.0f));
        prototypes.First(x => x.Internal_Name == "waterwheel_horizontal").Sprite.Animation_Frame_Time = 0.10f;
        prototypes.First(x => x.Internal_Name == "waterwheel_horizontal").Sprite.Animation_Sprites = new List<string>() { "waterwheel_3", "waterwheel_4", "waterwheel_5" };

        prototypes.Add(new Building("Waterwheel", "waterwheel_horizontal_flip", Building.UI_Category.Unbuildable, "waterwheel_3_flip", Building.BuildingSize.s2x2, 50, new Dictionary<Resource, int>(), 0, new List<Resource>(), 0, 0.0f, 0, new Dictionary<Resource, float>(), 0.0f, 0.0f, 0, new Dictionary<Building.Resident, int>(), 0, false, false, false, 0.0f, 0, null, null, null, null, new List<Resource>(), new List<Resource>(), 0.0f, 0.0f));
        prototypes.First(x => x.Internal_Name == "waterwheel_horizontal_flip").Sprite.Animation_Frame_Time = 0.10f;
        prototypes.First(x => x.Internal_Name == "waterwheel_horizontal_flip").Sprite.Animation_Sprites = new List<string>() { "waterwheel_3_flip", "waterwheel_4_flip", "waterwheel_5_flip" };

        prototypes.Add(new Building("Bakery", "bakery", Building.UI_Category.Agriculture, "bakery", Building.BuildingSize.s2x2, 125, new Dictionary<Resource, int>() {
            { Resource.Lumber, 50 }, { Resource.Stone, 40 }, { Resource.Bricks, 100 }, { Resource.Tools, 25 }
        }, 225, new List<Resource>(), 0, 50.0f, 210, new Dictionary<Resource, float>() { { Resource.Bricks, 0.05f }, { Resource.Lumber, 0.01f } }, 1.25f, 0.0f, 0, new Dictionary<Building.Resident, int>() {
            { Building.Resident.Citizen, 5 } }, 5, true, false, true, 0.0f, 6, null, delegate (Building building, float delta_time) {
                List<Resource> fuel_types = new List<Resource>() { Resource.Firewood, Resource.Charcoal, Resource.Coal };
                Resource selected_fuel = fuel_types[building.Special_Settings.First(x => x.Name == "fuel").Dropdown_Selection];
                foreach (Resource fuel_type in fuel_types) {
                    if (fuel_type != selected_fuel && building.Consumes.Contains(fuel_type)) {
                        building.Consumes.Remove(fuel_type);
                    }
                }
                if (!building.Consumes.Contains(selected_fuel)) {
                    building.Consumes.Add(selected_fuel);
                }
                if (!building.Is_Operational) {
                    return;
                }
                float fuel_usage = selected_fuel == Resource.Firewood ? 2.5f : 1.25f;
                building.Process(new Dictionary<Resource, float>() { { Resource.Flour, 10.0f }, { selected_fuel, fuel_usage } }, new Dictionary<Resource, float>() { { Resource.Bread, 30.0f } }, delta_time);
            }, null, null, new List<Resource>() { Resource.Flour, Resource.Firewood, Resource.Charcoal, Resource.Coal }, new List<Resource>() { Resource.Bread }, 0.0f, 0.0f));
        prototypes.First(x => x.Internal_Name == "bakery").Special_Settings.Add(new SpecialSetting("fuel", "Fuel", SpecialSetting.SettingType.Dropdown, 0, false, new List<string>() { Resource.Firewood.UI_Name + " (2.5/day)", Resource.Charcoal.UI_Name + " (1.25/day)", Resource.Coal.UI_Name + " (1.25/day)" }, 0));
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
