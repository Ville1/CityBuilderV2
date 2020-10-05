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

    private void On_Harbor_Built(Building building) {
        Dictionary<Coordinates.Direction, int> docks = new Dictionary<Coordinates.Direction, int>() {
            { Coordinates.Direction.North, 2 },
            { Coordinates.Direction.East, 3 },
            { Coordinates.Direction.South, 0 },
            { Coordinates.Direction.West, 1 }
        };
        Tile center_tile = Map.Instance.Get_Tile_At(building.Tile.Coordinates.Shift(new Coordinates(1, 1)));
        foreach (KeyValuePair<Coordinates.Direction, int> dock_data in docks) {
            Coordinates c = new Coordinates(center_tile.Coordinates);
            c.Shift(dock_data.Key);
            c.Shift(dock_data.Key);
            Tile tile = Map.Instance.Get_Tile_At(c);
            if (tile != null && tile.Is_Water && tile.Has_Ship_Access && tile.Building == null) {
                Building dock = new Building(Instance.Get("dock"), tile, new List<Tile>() { tile }, false);
                dock.Selected_Sprite = dock_data.Value;
                City.Instance.Add_Building(dock);
                building.Data.Add(Building.DOCK_ID_KEY, dock.Id.ToString());
                break;
            }
        }
    }

    //TODO: Refund ship is this harbor has one?
    private void On_Harbor_Deconstruct(Building building) {
        Building dock = City.Instance.Buildings.FirstOrDefault(x => x.Id == long.Parse(building.Data[Building.DOCK_ID_KEY]));
        if (dock != null) {
            dock.Deconstruct(true, false);
        }
    }

    private bool On_Harbor_Build_Check(Building building, Tile tile, out string message)
    {
        Tile center_tile = Map.Instance.Get_Tile_At(tile.Coordinates.Shift(new Coordinates(1, 1)));
        foreach (Coordinates.Direction direction in Coordinates.Directly_Adjacent_Directions) {
            Coordinates c = new Coordinates(center_tile.Coordinates);
            Coordinates.Direction opposite = Helper.Rotate(direction, 4);
            c.Shift(direction);
            c.Shift(direction);
            List<Tile> waterfront = new List<Tile>() { Map.Instance.Get_Tile_At(c) };
            foreach (Coordinates.Direction shift_direction in Coordinates.Directly_Adjacent_Directions) {
                if (shift_direction == direction || shift_direction == opposite) {
                    continue;
                }
                waterfront.Add(Map.Instance.Get_Tile_At(c, shift_direction));
            }
            bool valid = true;
            foreach (Tile t in waterfront) {
                if (!t.Is_Water || !t.Has_Ship_Access || t.Building != null) {
                    valid = false;
                    break;
                }
            }
            if (valid) {
                message = null;
                return true;
            }
        }
        message = "Requires straight waterfront with space for dock and ship access.";
        return false;
    }

    private bool On_Harbor_Ship_Build_Check(Building building, Tile tile, out string message)
    {
        if(!On_Harbor_Build_Check(building, tile, out message)) {
            return false;
        }
        foreach(Building b in City.Instance.Buildings) {
            if(b.Internal_Name == "shipyard" && b.Data.ContainsKey("ship_count") && b.Data.ContainsKey("has_ship_access") && int.Parse(b.Data["ship_count"]) > 0) {
                message = null;
                return true;
            }
        }
        message = "Ship required";
        return false;
    }

    private void On_Harbor_Ship_Building_Start(Building building)
    {
        foreach (Building b in City.Instance.Buildings) {
            if (b.Internal_Name == "shipyard" && b.Data.ContainsKey("ship_count") && b.Data.ContainsKey("has_ship_access") && int.Parse(b.Data["ship_count"]) > 0) {
                b.Data["ship_count"] = (int.Parse(b.Data["ship_count"]) - 1).ToString();
                return;
            }
        }
    }

    private bool Waterfront_Check(Building building, Tile tile, out string message) {
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
        prototypes.First(x => x.Internal_Name == "wood_cutters_lodge").Tags.Add(Building.Tag.Does_Not_Disrupt_Hunting);

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
        prototypes.First(x => x.Internal_Name == "dirt_road").Tags.Add(Building.Tag.Does_Not_Disrupt_Hunting);
        prototypes.First(x => x.Internal_Name == "dirt_road").Tags.Add(Building.Tag.No_Notification_On_Build);

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
        prototypes.First(x => x.Internal_Name == "cobblestone_road").Tags.Add(Building.Tag.No_Notification_On_Build);

        prototypes.Add(new Building("Wooden Bridge", "wooden_bridge", Building.UI_Category.Infrastructure, "bridge_ew", Building.BuildingSize.s1x1, 10, new Dictionary<Resource, int>() { { Resource.Lumber, 10 }, { Resource.Wood, 10 }, { Resource.Stone, 1 }, { Resource.Tools, 1 } }, 25,
            new List<Resource>(), 0, 0.0f, 50, new Dictionary<Resource, float>() { { Resource.Lumber, 0.01f }, { Resource.Wood, 0.01f } }, 0.01f, 0.0f, 0, new Dictionary<Building.Resident, int>(), 0, false, true, true, 0.0f, 0, null, null, null, null, new List<Resource>(), new List<Resource>(), 0.0f, 0.0f));
        prototypes.First(x => x.Internal_Name == "wooden_bridge").Tags.Add(Building.Tag.Does_Not_Block_Wind);
        prototypes.First(x => x.Internal_Name == "wooden_bridge").Tags.Add(Building.Tag.Bridge);
        prototypes.First(x => x.Internal_Name == "wooden_bridge").Tags.Add(Building.Tag.Does_Not_Disrupt_Hunting);
        prototypes.First(x => x.Internal_Name == "wooden_bridge").Tags.Add(Building.Tag.No_Notification_On_Build);
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
                float progress = building.Data.ContainsKey("chop_progress") ? float.Parse(building.Data["chop_progress"]) : 0.0f;
                progress += delta_time;
                if (building.Data.ContainsKey("chop_progress")) {
                    building.Data["chop_progress"] = progress.ToString();
                } else {
                    building.Data.Add("chop_progress", progress.ToString());
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
            Resource.Simple_Clothes, Resource.Leather_Clothes, Resource.Mechanisms, Resource.Clay, Resource.Bricks, Resource.Marble, Resource.Coffee, Resource.Copper_Ore, Resource.Copper_Bars, Resource.Tin_Ore, Resource.Tin_Bars, Resource.Bronze_Bars, Resource.Pewter_Bars, Resource.Pewterware,
            Resource.Furniture, Resource.Pig_Iron, Resource.Steel_Bars, Resource.Simple_Jewelry, Resource.Opulent_Jewelry, Resource.Furs, Resource.Silk, Resource.Fine_Clothes, Resource.Luxury_Clothes, Resource.Fine_Jewelry, Resource.Silver_Ore, Resource.Silver_Bars, Resource.Gold_Ore, Resource.Gold_Bars, Resource.Gems,
            Resource.Sand, Resource.Potash, Resource.Glass, Resource.Glassware },
        2000, 65.0f, 250, new Dictionary<Resource, float>() { { Resource.Lumber, 0.05f } }, 1.0f, 0.0f, 0.0f, new Dictionary<Building.Resident, int>() { { Building.Resident.Peasant, 10 } }, 10, false, false, true, 0.0f, 16, null, null, null, null, new List<Resource>(), new List<Resource>(), 0.0f, 0.0f));
        prototypes.First(x => x.Internal_Name == "storehouse").Sprites.Add(new SpriteData("storehouse_1"));

        prototypes.Add(new Building("Cellar", "cellar", Building.UI_Category.Infrastructure, "cellar", Building.BuildingSize.s1x1, 100, new Dictionary<Resource, int>() {
            { Resource.Wood, 15 }, { Resource.Stone, 50 }, { Resource.Tools, 10 }, { Resource.Lumber, 50 }
        }, 100, new List<Resource>() { Resource.Roots, Resource.Berries, Resource.Mushrooms, Resource.Herbs, Resource.Game, Resource.Potatoes, Resource.Bread, Resource.Ale, Resource.Mutton, Resource.Corn, Resource.Fish, Resource.Bananas, Resource.Oranges, Resource.Beer, Resource.Rum,
            Resource.Wine, Resource.Pretzels, Resource.Cake, Resource.Salted_Fish, Resource.Salted_Meat, Resource.Grapes, Resource.Lobsters },
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
        prototypes.First(x => x.Internal_Name == "gatherers_lodge").Tags.Add(Building.Tag.Does_Not_Disrupt_Hunting);

        prototypes.Add(new Building("Fisher's Hut", "fishers_hut", Building.UI_Category.Coastal, "fishers_hut", Building.BuildingSize.s2x2, 90, new Dictionary<Resource, int>() {
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
            List<Tile> boat_spawns = new List<Tile>();
            int max_boats = Mathf.RoundToInt(2 * building.Efficency);
            foreach (Tile tile in building.Get_Tiles_In_Circle(building.Range)) {
                if (tile.Internal_Name.StartsWith("water")) {
                    total_water_tiles += 1.0f;
                    if (tile.Can_Work(building, Tile.Work_Type.Fish)) {
                        own_water_tiles += 1.0f;
                        if(tile.Building == null) {
                            boat_spawns.Add(tile);
                        }
                    }
                }
            }

            if(total_water_tiles == 0.0f) {
                return;
            }

            if(boat_spawns.Count != 0 && building.Entities_Spawned.Count < max_boats) {
                Tile spawn = RNG.Instance.Item(boat_spawns);
                Entity boat = new Entity(EntityPrototypes.Instance.Get("fishing_boat"), spawn, building);
            }
            
            float prefered_water_count = 25.0f;
            float overlap_multiplier = own_water_tiles / total_water_tiles;
            float water_multiplier = own_water_tiles >= prefered_water_count ? 1.0f : own_water_tiles / prefered_water_count;

            float base_fish = 5.00f;

            float fish = (base_fish * overlap_multiplier) * water_multiplier;

            building.Produce(Resource.Fish, fish, delta_time);
        }, unreserve_tiles, Highlight_Tiles(Tile.Work_Type.Fish), new List<Resource>(), new List<Resource>() { Resource.Fish }, 0.0f, 0.0f));
        prototypes.First(x => x.Internal_Name == "fishers_hut").On_Build_Check = Waterfront_Check;
        prototypes.First(x => x.Internal_Name == "fishers_hut").Tags.Add(Building.Tag.Does_Not_Disrupt_Hunting);

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
                    market.Check_Input_Storage(fuel_type);
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
                    bool has_delicacies = false;
                    foreach (KeyValuePair<Resource, float> pair in market.Input_Storage) {
                        if (pair.Key.Is_Food && pair.Value > 0.0f) {
                            if(pair.Key.Food_Type == Resource.FoodType.Meat) {
                                has_meat = true;
                            } else if (pair.Key.Food_Type == Resource.FoodType.Vegetable) {
                                has_vegetables = true;
                            } else if (pair.Key.Food_Type == Resource.FoodType.Delicacy) {
                                has_delicacies = true;
                            }
                        }
                    }
                    float disparity = unique_food_count != 0.0f ? (1.0f / unique_food_count) - min_food_ratio : 1.0f;
                    float food_quality = Math.Max(unique_food_count * (1.0f - disparity), 0.0f);
                    food_quality = (Mathf.Pow(food_quality, 0.5f) + ((food_quality - 1.0f) * 0.1f)) / 4.0f;
                    food_quality *= efficency_multiplier;
                    food_quality = Mathf.Clamp01(food_quality);
                    if (!(has_meat && has_vegetables)) {
                        food_quality *= 0.5f;
                    }
                    if (has_delicacies) {
                        food_quality *= 0.1f;
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
                        market.Check_Input_Storage(pair.Key);
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
                market.Check_Input_Storage(Resource.Herbs);
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
                market.Check_Input_Storage(Resource.Salt);
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
                market.Check_Input_Storage(Resource.Simple_Clothes);
                market.Check_Input_Storage(Resource.Leather_Clothes);
            }

            if (income != 0.0f) {
                market.Per_Day_Cash_Delta += (income / delta_time) * TimeManager.Instance.Days_To_Seconds(1.0f, 1.0f);
                City.Instance.Add_Cash(income);
            }//                                  v unnecessary list v special settings adds and removes stuff from consumption list MIGHT ACTUALLY BE NECESSARY, DONT REMOVE
        }, null, null, new List<Resource>() { Resource.Berries, Resource.Roots, Resource.Mushrooms, Resource.Herbs, Resource.Firewood, Resource.Charcoal, Resource.Coal, Resource.Game, Resource.Bread, Resource.Potatoes, Resource.Salt, Resource.Mutton, Resource.Corn, Resource.Fish, Resource.Bananas,
            Resource.Oranges, Resource.Grapes, Resource.Pretzels, Resource.Cake, Resource.Simple_Clothes, Resource.Leather_Clothes, Resource.Salted_Fish, Resource.Salted_Meat, Resource.Lobsters }, new List<Resource>(), 0.05f, 5.0f));
        Resource prefered_fuel = Resource.All.Where(x => x.Is_Fuel).OrderByDescending(x => x.Value / x.Fuel_Value).FirstOrDefault();
        foreach(Resource resource in Resource.All) {
            if (resource.Is_Food) {
                prototypes.First(x => x.Internal_Name == "marketplace").Special_Settings.Add(new SpecialSetting(resource.ToString().ToLower(), resource.UI_Name, SpecialSetting.SettingType.Toggle, 0.0f, resource.Food_Type != Resource.FoodType.Delicacy));
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
                if (tile.Building != null && tile.Building != building && !tile.Building.Tags.Contains(Building.Tag.Does_Not_Disrupt_Hunting)) {
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
        prototypes.First(x => x.Internal_Name == "hunting_lodge").Tags.Add(Building.Tag.Does_Not_Disrupt_Hunting);

        prototypes.Add(new Building("Trapper", "trapper", Building.UI_Category.Forestry, "trapper", Building.BuildingSize.s2x2, 100, new Dictionary<Resource, int>() {
            { Resource.Wood, 60 }, { Resource.Stone, 5 }, { Resource.Tools, 10 }
        }, 125, new List<Resource>(), 0, 0.0f, 65, new Dictionary<Resource, float>() { { Resource.Wood, 0.01f } }, 1.00f, 0.0f, 0, new Dictionary<Building.Resident, int>() { { Building.Resident.Peasant, 5 } }, 5, true, false, true, 9.0f, 0, Reserve_Tiles(Tile.Work_Type.Trap),
        delegate (Building building, float delta_time) {
            if (!building.Is_Operational) {
                return;
            }
            float furs = 0.0f;
            foreach (Tile tile in building.Get_Tiles_In_Circle(building.Range)) {
                if (tile.Building != null && tile.Building != building && !tile.Building.Tags.Contains(Building.Tag.Does_Not_Disrupt_Hunting)) {
                    if (tile.Building.Is_Road && tile.Building.Size == Building.BuildingSize.s1x1) {
                        furs -= 0.05f;
                    } else {
                        furs -= 1.0f;
                    }
                } else if (tile.Building == null && tile.Can_Work(building, Tile.Work_Type.Trap)) {
                    if (tile.Internal_Name == "forest") {
                        furs += 1.00f;
                    } else if (tile.Internal_Name == "sparse_forest") {
                        furs += 0.85f;
                    } else if (tile.Internal_Name == "grass") {
                        furs += 0.10f;
                    } else if (tile.Internal_Name == "fertile_ground") {
                        furs += 0.125f;
                    } else if (tile.Internal_Name.StartsWith("hill_")) {
                        furs += 0.25f;
                    }
                }
            }
            float multiplier = 0.015f;
            furs = furs * multiplier;
            float game = furs * 0.25f;
            building.Produce(Resource.Furs, furs, delta_time);
            building.Produce(Resource.Game, game, delta_time);
        }, unreserve_tiles, Highlight_Tiles(Tile.Work_Type.Trap), new List<Resource>(), new List<Resource>() { Resource.Game, Resource.Furs }, 0.0f, 0.0f));
        prototypes.First(x => x.Internal_Name == "trapper").Tags.Add(Building.Tag.Does_Not_Disrupt_Hunting);

        prototypes.Add(new Building("Quarry", "quarry", Building.UI_Category.Industry, "quarry", Building.BuildingSize.s3x3, 350, new Dictionary<Resource, int>() {
            { Resource.Wood, 65 }, { Resource.Lumber, 80 }, { Resource.Tools, 45 }
        }, 175, new List<Resource>(), 0, 0.0f, 225, new Dictionary<Resource, float>() { { Resource.Wood, 0.10f } }, 1.75f, 0.0f, 0, new Dictionary<Building.Resident, int>() { { Building.Resident.Peasant, 20 } }, 20, true, false, true, 0.0f, 0, null, delegate (Building building, float delta_time) {
            if (!building.Is_Operational) {
                return;
            }
            float base_stone = 10.0f;
            float stone_tiles = 0.0f;
            float marble = 0.0f;
            float marble_multiplier = 1.0f;
            float total_tiles = building.Tiles.Count;
            foreach(Tile tile in building.Tiles) {
                if (tile.Minerals.ContainsKey(Mineral.Marble)) {
                    marble += tile.Minerals[Mineral.Marble];
                } else {
                    stone_tiles += 1.0f;
                }
            }
            building.Produce(Resource.Stone, base_stone * (stone_tiles / total_tiles), delta_time);
            building.Produce(Resource.Marble, marble * marble_multiplier, delta_time);
        }, null, null, new List<Resource>(), new List<Resource>() { Resource.Stone, Resource.Marble }, -0.5f, 7.0f));

        prototypes.Add(new Building("Decorative Tree", "decorative_tree", Building.UI_Category.Services, "decorative_tree", Building.BuildingSize.s1x1, 50, new Dictionary<Resource, int>() {
            { Resource.Stone, 5 }, { Resource.Tools, 1 }
        }, 25, new List<Resource>(), 0, 0.0f, 50, new Dictionary<Resource, float>(), 0.02f, 0.0f, 0, new Dictionary<Building.Resident, int>(), 0, false, false, false, 0.0f, 0, null, null, null, null,
        new List<Resource>(), new List<Resource>(), 0.5f, 3.0f));
        prototypes.First(x => x.Internal_Name == "decorative_tree").Sprites.Add(new SpriteData("decorative_tree_1"));
        prototypes.First(x => x.Internal_Name == "decorative_tree").Sprites.Add(new SpriteData("decorative_tree_2"));
        prototypes.First(x => x.Internal_Name == "decorative_tree").Sprites.Add(new SpriteData("decorative_tree_3"));
        prototypes.First(x => x.Internal_Name == "decorative_tree").Tags.Add(Building.Tag.Random_Sprite);
        prototypes.First(x => x.Internal_Name == "decorative_tree").Tags.Add(Building.Tag.No_Notification_On_Build);

        prototypes.Add(new Building("Fountain Plaza", "fountain_plaza", Building.UI_Category.Services, "fountain_plaza", Building.BuildingSize.s1x1, 75, new Dictionary<Resource, int>() {
            { Resource.Stone, 15 }, { Resource.Marble, 10 }, { Resource.Mechanisms, 1 }, { Resource.Tools, 5 }
        }, 300, new List<Resource>(), 0, 0.0f, 150, new Dictionary<Resource, float>() { { Resource.Stone, 0.01f }, { Resource.Marble, 0.01f } }, 0.05f, 0.0f, 0, new Dictionary<Building.Resident, int>(), 0, false, true, false, 0.0f, 0, null, null, null, null,
        new List<Resource>(), new List<Resource>(), 0.75f, 5.0f));
        prototypes.First(x => x.Internal_Name == "fountain_plaza").Sprite.Animation_Frame_Time = 0.25f;
        prototypes.First(x => x.Internal_Name == "fountain_plaza").Sprite.Animation_Sprites = new List<string>() { "fountain_plaza_1", "fountain_plaza_2", "fountain_plaza_3", "fountain_plaza_4" };

        prototypes.Add(new Building("Mine", "mine", Building.UI_Category.Industry, "mine", Building.BuildingSize.s2x2, 200, new Dictionary<Resource, int>() {
            { Resource.Wood, 90 }, { Resource.Lumber, 90 }, { Resource.Stone, 15 }, { Resource.Tools, 40 }
        }, 250, new List<Resource>(), 0, 0.0f, 250, new Dictionary<Resource, float>() { { Resource.Wood, 0.10f } }, 2.00f, 0.0f, 0, new Dictionary<Building.Resident, int>() { { Building.Resident.Peasant, 15 } }, 15, true, false, true, 5.0f, 0, Reserve_Tiles(Tile.Work_Type.Mine), delegate (Building building, float delta_time) {
            if (!building.Is_Operational) {
                return;
            }
            float iron_ore = 0.0f;
            float coal = 0.0f;
            float salt = 0.0f;
            float copper = 0.0f;
            float tin = 0.0f;
            float silver = 0.0f;
            float gold = 0.0f;
            float gems = 0.0f;
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
                    if (tile.Minerals.ContainsKey(Mineral.Copper)) {
                        copper += tile.Minerals[Mineral.Copper];
                    }
                    if (tile.Minerals.ContainsKey(Mineral.Tin)) {
                        tin += tile.Minerals[Mineral.Tin];
                    }
                    if (tile.Minerals.ContainsKey(Mineral.Silver)) {
                        silver += tile.Minerals[Mineral.Silver];
                    }
                    if (tile.Minerals.ContainsKey(Mineral.Gold)) {
                        gold += tile.Minerals[Mineral.Gold];
                    }
                    if (tile.Minerals.ContainsKey(Mineral.Gems)) {
                        gems += tile.Minerals[Mineral.Gems];
                    }
                }
            }
            float multiplier = 0.20f;
            building.Produce(Resource.Iron_Ore, iron_ore * multiplier, delta_time);
            building.Produce(Resource.Coal, coal * multiplier, delta_time);
            building.Produce(Resource.Salt, salt * multiplier, delta_time);
            building.Produce(Resource.Copper_Ore, copper * multiplier, delta_time);
            building.Produce(Resource.Tin_Ore, tin * multiplier, delta_time);
            building.Produce(Resource.Silver_Ore, silver * multiplier, delta_time);
            building.Produce(Resource.Gold_Ore, gold * multiplier, delta_time);
            building.Produce(Resource.Gems, gems * multiplier, delta_time);
        }, unreserve_tiles, Highlight_Tiles(Tile.Work_Type.Mine), new List<Resource>(), new List<Resource>() { Resource.Iron_Ore, Resource.Coal, Resource.Salt, Resource.Copper_Ore, Resource.Tin_Ore, Resource.Silver_Ore, Resource.Gold_Ore, Resource.Gems }, -0.5f, 5.0f));

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
            List<Resource> production_types = new List<Resource>() { Resource.Charcoal, Resource.Potash };
            Resource selected_fuel = fuel_types[building.Special_Settings.First(x => x.Name == "fuel").Dropdown_Selection];
            Resource selected_production = production_types[building.Special_Settings.First(x => x.Name == "production").Dropdown_Selection];
            foreach (Resource fuel_type in fuel_types) {
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
            building.Process(new Dictionary<Resource, float>() { { Resource.Wood, 5.0f }, { selected_fuel, fuel_usage } }, new Dictionary<Resource, float>() { { selected_production, 5.0f } }, delta_time);
        }, null, null, new List<Resource>() { Resource.Wood, Resource.Charcoal, Resource.Coal, Resource.Firewood }, new List<Resource>() { Resource.Charcoal, Resource.Potash }, -1.50f, 6.0f));
        prototypes.First(x => x.Internal_Name == "charcoal_burner").Special_Settings.Add(new SpecialSetting("fuel", "Fuel", SpecialSetting.SettingType.Dropdown, 0, false, new List<string>() { Resource.Firewood.UI_Name + " (2.5/day)", Resource.Charcoal.UI_Name + " (1.25/day)", Resource.Coal.UI_Name +" (1.25/day)" }, 0));
        prototypes.First(x => x.Internal_Name == "charcoal_burner").Special_Settings.Add(new SpecialSetting("production", "Production", SpecialSetting.SettingType.Dropdown, 0, false, new List<string>() { Resource.Charcoal.UI_Name + " (5/day)", Resource.Potash.UI_Name + " (5/day)" }, 0));

        prototypes.Add(new Building("Foundry", "foundry", Building.UI_Category.Industry, "foundry", Building.BuildingSize.s2x2, 225, new Dictionary<Resource, int>() {
            { Resource.Lumber, 75 }, { Resource.Stone, 130 }, { Resource.Tools, 20 }
        }, 160, new List<Resource>(), 0, 50.0f, 205, new Dictionary<Resource, float>() { { Resource.Stone, 0.05f }, { Resource.Lumber, 0.01f } }, 1.00f, 0.0f, 0, new Dictionary<Building.Resident, int>() {
        { Building.Resident.Peasant, 10 } }, 10, true, false, true, 0.0f, 7, null, delegate (Building building, float delta_time) {
            List<Resource> fuel_types = new List<Resource>() { Resource.Firewood, Resource.Charcoal, Resource.Coal };
            Resource selected_fuel = fuel_types[building.Special_Settings.First(x => x.Name == "fuel").Dropdown_Selection];
            List<Resource> ore_types = new List<Resource>() { Resource.Iron_Ore, Resource.Copper_Ore, Resource.Tin_Ore, Resource.Silver_Ore, Resource.Gold_Ore };
            Resource selected_ore = ore_types[building.Special_Settings.First(x => x.Name == "production").Dropdown_Selection];
            List<Resource> bar_types = new List<Resource>() { Resource.Iron_Bars, Resource.Copper_Bars, Resource.Tin_Bars, Resource.Silver_Bars, Resource.Gold_Bars };
            Resource selected_bar = bar_types[building.Special_Settings.First(x => x.Name == "production").Dropdown_Selection];
            foreach (Resource fuel_type in fuel_types) {
                if (fuel_type != selected_fuel && building.Consumes.Contains(fuel_type)) {
                    building.Consumes.Remove(fuel_type);
                }
            }
            if (!building.Consumes.Contains(selected_fuel)) {
                building.Consumes.Add(selected_fuel);
            }
            foreach (Resource ore in ore_types) {
                if (ore != selected_ore && building.Consumes.Contains(ore)) {
                    building.Consumes.Remove(ore);
                } else if(ore == selected_ore && !building.Consumes.Contains(ore)) {
                    building.Consumes.Add(ore);
                }
            }
            if (!building.Is_Operational) {
                return;
            }
            float fuel_usage = selected_fuel == Resource.Firewood ? 10.0f : 5.0f;
            building.Process(new Dictionary<Resource, float>() { { selected_ore, 20.0f }, { selected_fuel, fuel_usage } }, new Dictionary<Resource, float>() { { selected_bar, 10.0f } }, delta_time);
        }, null, null, new List<Resource>() { Resource.Iron_Ore, Resource.Copper_Ore, Resource.Tin_Ore, Resource.Charcoal, Resource.Coal, Resource.Firewood, Resource.Silver_Ore, Resource.Gold_Ore },
        new List<Resource>() { Resource.Iron_Bars, Resource.Copper_Bars, Resource.Tin_Bars, Resource.Silver_Bars, Resource.Gold_Bars }, -1.25f, 6.0f));
        prototypes.First(x => x.Internal_Name == "foundry").Special_Settings.Add(new SpecialSetting("fuel", "Fuel", SpecialSetting.SettingType.Dropdown, 0, false, new List<string>() {
            Resource.Firewood.UI_Name + " (10/day)",
            Resource.Charcoal.UI_Name + " (5/day)",
            Resource.Coal.UI_Name + " (5/day)" },
        0));
        prototypes.First(x => x.Internal_Name == "foundry").Special_Settings.Add(new SpecialSetting("production", "Production", SpecialSetting.SettingType.Dropdown, 0, false, new List<string>() {
            Resource.Iron_Bars.UI_Name + " (10/day)",
            Resource.Copper_Bars.UI_Name + " (10/day)",
            Resource.Tin_Bars.UI_Name + " (10/day)",
            Resource.Silver_Bars.UI_Name + " (10/day)",
            Resource.Gold_Bars.UI_Name + " (10/day)"
        }, 0));
        prototypes.First(x => x.Internal_Name == "foundry").Sprites.Add(new SpriteData("foundry_1"));

        prototypes.Add(new Building("Blast Furnace", "blast_furnace", Building.UI_Category.Industry, "blast_furnace", Building.BuildingSize.s3x3, 300, new Dictionary<Resource, int>() {
            { Resource.Lumber, 75 }, { Resource.Stone, 200 }, { Resource.Bricks, 50 }, { Resource.Mechanisms, 10 }, { Resource.Tools, 40 }
        }, 275, new List<Resource>(), 0, 50.0f, 335, new Dictionary<Resource, float>() { { Resource.Stone, 0.10f }, { Resource.Lumber, 0.01f }, { Resource.Bricks, 0.01f } }, 3.00f, 0.0f, 0, new Dictionary<Building.Resident, int>() {
        { Building.Resident.Peasant, 15 }, { Building.Resident.Citizen, 5 } }, 15, true, false, true, 0.0f, 7, null, delegate (Building building, float delta_time) {
            List<Resource> fuel_types = new List<Resource>() { Resource.Firewood, Resource.Charcoal, Resource.Coal };
            Resource selected_fuel = fuel_types[building.Special_Settings.First(x => x.Name == "fuel").Dropdown_Selection];
            List<Resource> carbon_types = new List<Resource>() { Resource.Charcoal, Resource.Coal };
            Resource selected_carbon = carbon_types[building.Special_Settings.First(x => x.Name == "carbon").Dropdown_Selection];
            int production = building.Special_Settings.First(x => x.Name == "production").Dropdown_Selection;
            Dictionary<Resource, float> inputs = new Dictionary<Resource, float>();
            Dictionary<Resource, float> outputs = new Dictionary<Resource, float>();

            inputs.Add(selected_fuel, selected_fuel == Resource.Firewood ? 12.5f : 6.25f);
            switch (production) {
                case 0:
                    inputs.Add(Resource.Iron_Ore, 30.0f);
                    outputs.Add(Resource.Iron_Bars, 15.0f);
                    break;
                case 1:
                    inputs.Add(Resource.Copper_Ore, 30.0f);
                    outputs.Add(Resource.Copper_Bars, 15.0f);
                    break;
                case 2:
                    inputs.Add(Resource.Tin_Ore, 30.0f);
                    outputs.Add(Resource.Tin_Bars, 15.0f);
                    break;
                case 3:
                    inputs.Add(Resource.Copper_Bars, 10.0f);
                    inputs.Add(Resource.Tin_Bars, 2.0f);
                    outputs.Add(Resource.Bronze_Bars, 10.0f);
                    break;
                case 4:
                    inputs.Add(Resource.Tin_Bars, 10.0f);
                    inputs.Add(Resource.Copper_Bars, 2.0f);
                    outputs.Add(Resource.Pewter_Bars, 10.0f);
                    break;
                case 5:
                    inputs.Add(Resource.Iron_Ore, 30.0f);
                    if (inputs.ContainsKey(selected_carbon)) {
                        inputs[selected_carbon] += 15.0f;
                    } else {
                        inputs.Add(selected_carbon, 15.0f);
                    }
                    outputs.Add(Resource.Pig_Iron, 15.0f);
                    break;
                case 6:
                    inputs.Add(Resource.Iron_Bars, 10.0f);
                    inputs.Add(Resource.Pig_Iron, 10.0f);
                    if (inputs.ContainsKey(selected_carbon)) {
                        inputs[selected_carbon] += 10.0f;
                    } else {
                        inputs.Add(selected_carbon, 10.0f);
                    }
                    outputs.Add(Resource.Steel_Bars, 10.0f);
                    break;
                case 7:
                    inputs.Add(Resource.Silver_Ore, 30.0f);
                    outputs.Add(Resource.Silver_Bars, 15.0f);
                    break;
                case 8:
                    inputs.Add(Resource.Gold_Ore, 30.0f);
                    outputs.Add(Resource.Gold_Bars, 15.0f);
                    break;
            }
            building.Update_Consumes_Produces(inputs, outputs);
            if (!building.Is_Operational) {
                return;
            }
            building.Process(inputs, outputs, delta_time);
        }, null, null, new List<Resource>() { Resource.Iron_Ore, Resource.Copper_Ore, Resource.Tin_Ore, Resource.Charcoal, Resource.Coal, Resource.Firewood, Resource.Iron_Bars, Resource.Pig_Iron, Resource.Silver_Ore, Resource.Gold_Ore },
        new List<Resource>() { Resource.Iron_Bars, Resource.Copper_Bars, Resource.Tin_Bars, Resource.Pig_Iron, Resource.Steel_Bars, Resource.Silver_Bars, Resource.Gold_Bars }, -1.25f, 7.0f));
        prototypes.First(x => x.Internal_Name == "blast_furnace").Special_Settings.Add(new SpecialSetting("fuel", "Fuel", SpecialSetting.SettingType.Dropdown, 0, false, new List<string>() {
            Resource.Firewood.UI_Name + " (12.5/day)",
            Resource.Charcoal.UI_Name + " (6.25/day)",
            Resource.Coal.UI_Name + " (6.25/day)"
        }, 0));
        prototypes.First(x => x.Internal_Name == "blast_furnace").Special_Settings.Add(new SpecialSetting("carbon", "Carbon", SpecialSetting.SettingType.Dropdown, 0, false, new List<string>() {
            Resource.Charcoal.UI_Name,
            Resource.Coal.UI_Name
        }, 0));
        prototypes.First(x => x.Internal_Name == "blast_furnace").Special_Settings.Add(new SpecialSetting("production", "Production", SpecialSetting.SettingType.Dropdown, 0, false, new List<string>() {
            Resource.Iron_Bars.UI_Name + " (15/day)",
            Resource.Copper_Bars.UI_Name + " (15/day)",
            Resource.Tin_Bars.UI_Name + " (15/day)",
            Resource.Bronze_Bars.UI_Name + " (10/day)",
            Resource.Pewter_Bars.UI_Name + " (10/day)",
            Resource.Pig_Iron.UI_Name + " (15/day)",
            Resource.Steel_Bars.UI_Name + " (10/day)",
            Resource.Silver_Bars.UI_Name + " (15/day)",
            Resource.Gold_Bars.UI_Name + " (15/day)"
        }, 0));

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
            int production = building.Special_Settings.First(x => x.Name == "production").Dropdown_Selection;
            Dictionary<Resource, float> inputs = new Dictionary<Resource, float>();
            Dictionary<Resource, float> outputs = new Dictionary<Resource, float>();

            inputs.Add(selected_fuel, selected_fuel == Resource.Firewood ? 2.5f : 1.25f);
            switch (production) {
                case 0:
                    inputs.Add(Resource.Iron_Bars, 5.0f);
                    inputs.Add(Resource.Lumber, 1.0f);
                    outputs.Add(Resource.Tools, 5.0f);
                    break;
                case 1:
                    inputs.Add(Resource.Iron_Bars, 5.0f);
                    outputs.Add(Resource.Mechanisms, 2.5f);
                    break;
                case 2:
                    inputs.Add(Resource.Copper_Bars, 7.5f);
                    outputs.Add(Resource.Mechanisms, 2.5f);
                    break;
            }
            building.Consumes.Clear();
            building.Produces.Clear();
            foreach (KeyValuePair<Resource, float> pair in inputs) {
                building.Consumes.Add(pair.Key);
            }
            foreach (KeyValuePair<Resource, float> pair in outputs) {
                building.Produces.Add(pair.Key);
            }
            if (!building.Is_Operational) {
                return;
            }
            building.Process(inputs, outputs, delta_time);
        }, null, null, new List<Resource>() { Resource.Iron_Bars, Resource.Copper_Bars, Resource.Lumber, Resource.Charcoal, Resource.Coal, Resource.Firewood }, new List<Resource>() { Resource.Tools, Resource.Mechanisms }, -1.00f, 4.0f));
        prototypes.First(x => x.Internal_Name == "smithy").Special_Settings.Add(new SpecialSetting("production", "Production", SpecialSetting.SettingType.Dropdown, 0.0f, false, new List<string>() { Resource.Tools.UI_Name + " (5/day)", Resource.Mechanisms.UI_Name + " (i) (2.5/day)", Resource.Mechanisms.UI_Name + " (c) (2.5/day)" }, 0));
        prototypes.First(x => x.Internal_Name == "smithy").Special_Settings.Add(new SpecialSetting("fuel", "Fuel", SpecialSetting.SettingType.Dropdown, 0, false, new List<string>() { Resource.Firewood.UI_Name + " (2.5/day)", Resource.Charcoal.UI_Name + " (1.25/day)", Resource.Coal.UI_Name + " (1.25/day)" }, 0));

        prototypes.Add(new Building("Metallurgist", "metallurgist", Building.UI_Category.Industry, "metallurgist", Building.BuildingSize.s2x2, 225, new Dictionary<Resource, int>() {
            { Resource.Lumber, 70 }, { Resource.Bricks, 120 }, { Resource.Stone, 35 }, { Resource.Tools, 25 }, { Resource.Mechanisms, 5 }
        }, 240, new List<Resource>(), 0, 50.0f, 230, new Dictionary<Resource, float>() { { Resource.Bricks, 0.05f }, { Resource.Lumber, 0.01f } }, 3.50f, 0.0f, 0, new Dictionary<Building.Resident, int>() {
        { Building.Resident.Citizen, 10 } }, 10, true, false, true, 0.0f, 7, null, delegate (Building building, float delta_time) {
            List<Resource> fuel_types = new List<Resource>() { Resource.Firewood, Resource.Charcoal, Resource.Coal };
            Resource selected_fuel = fuel_types[building.Special_Settings.First(x => x.Name == "fuel").Dropdown_Selection];
            int production = building.Special_Settings.First(x => x.Name == "production").Dropdown_Selection;
            Dictionary<Resource, float> inputs = new Dictionary<Resource, float>();
            Dictionary<Resource, float> outputs = new Dictionary<Resource, float>();

            inputs.Add(selected_fuel, selected_fuel == Resource.Firewood ? 5.0f : 2.5f);
            switch (production) {
                case 0:
                    inputs.Add(Resource.Pewter_Bars, 10.0f);
                    outputs.Add(Resource.Pewterware, 2.5f);
                    break;
                case 1:
                    inputs.Add(Resource.Iron_Bars, 10.0f);
                    outputs.Add(Resource.Mechanisms, 5.0f);
                    break;
                case 2:
                    inputs.Add(Resource.Bronze_Bars, 10.0f);
                    outputs.Add(Resource.Mechanisms, 5.0f);
                    break;
                case 3:
                    inputs.Add(Resource.Copper_Bars, 15.0f);
                    outputs.Add(Resource.Mechanisms, 5.0f);
                    break;
            }
            building.Consumes.Clear();
            building.Produces.Clear();
            foreach (KeyValuePair<Resource, float> pair in inputs) {
                building.Consumes.Add(pair.Key);
            }
            foreach (KeyValuePair<Resource, float> pair in outputs) {
                building.Produces.Add(pair.Key);
            }
            if (!building.Is_Operational) {
                return;
            }
            building.Process(inputs, outputs, delta_time);
        }, null, null, new List<Resource>() { Resource.Iron_Bars, Resource.Copper_Bars, Resource.Bronze_Bars, Resource.Pewter_Bars, Resource.Charcoal, Resource.Coal, Resource.Firewood }, new List<Resource>() { Resource.Mechanisms, Resource.Pewterware }, -1.10f, 5.0f));
        prototypes.First(x => x.Internal_Name == "metallurgist").Sprites.Add(new SpriteData("metallurgist_1"));
        prototypes.First(x => x.Internal_Name == "metallurgist").Special_Settings.Add(new SpecialSetting("fuel", "Fuel", SpecialSetting.SettingType.Dropdown, 0, false, new List<string>() { Resource.Firewood.UI_Name + " (5/day)", Resource.Charcoal.UI_Name + " (2.5/day)", Resource.Coal.UI_Name + " (2.5/day)" }, 0));
        prototypes.First(x => x.Internal_Name == "metallurgist").Special_Settings.Add(new SpecialSetting("production", "Production", SpecialSetting.SettingType.Dropdown, 0, false, new List<string>() { Resource.Pewterware.UI_Name + " (2.5/day)", Resource.Mechanisms.UI_Name + " (i) (5/day)", Resource.Mechanisms.UI_Name + " (b) (5/day)", Resource.Mechanisms.UI_Name + " (c) (5/day)" }, 0));

        prototypes.Add(new Building("Blacksmith", "blacksmith", Building.UI_Category.Industry, "blacksmith", Building.BuildingSize.s2x2, 225, new Dictionary<Resource, int>() {
            { Resource.Lumber, 85 }, { Resource.Bricks, 140 }, { Resource.Stone, 40 }, { Resource.Tools, 30 }
        }, 255, new List<Resource>(), 0, 50.0f, 265, new Dictionary<Resource, float>() { { Resource.Bricks, 0.05f }, { Resource.Lumber, 0.01f } }, 2.75f, 0.0f, 0, new Dictionary<Building.Resident, int>() {
        { Building.Resident.Citizen, 10 } }, 10, true, false, true, 0.0f, 7, null, delegate (Building building, float delta_time) {
            List<Resource> fuel_types = new List<Resource>() { Resource.Firewood, Resource.Charcoal, Resource.Coal };
            Resource selected_fuel = fuel_types[building.Special_Settings.First(x => x.Name == "fuel").Dropdown_Selection];
            int production = building.Special_Settings.First(x => x.Name == "production").Dropdown_Selection;
            Dictionary<Resource, float> inputs = new Dictionary<Resource, float>();
            Dictionary<Resource, float> outputs = new Dictionary<Resource, float>();

            inputs.Add(selected_fuel, selected_fuel == Resource.Firewood ? 5.0f : 2.5f);
            switch (production) {
                case 0:
                    inputs.Add(Resource.Steel_Bars, 10.0f);
                    inputs.Add(Resource.Lumber, 2.0f);
                    outputs.Add(Resource.Tools, 40.0f);
                    break;
                case 1:
                    inputs.Add(Resource.Iron_Bars, 10.0f);
                    inputs.Add(Resource.Lumber, 2.0f);
                    outputs.Add(Resource.Tools, 10.0f);
                    break;
                case 2:
                    inputs.Add(Resource.Steel_Bars, 10.0f);
                    outputs.Add(Resource.Mechanisms, 20.0f);
                    break;
                case 3:
                    inputs.Add(Resource.Iron_Bars, 10.0f);
                    outputs.Add(Resource.Mechanisms, 5.0f);
                    break;
            }
            building.Update_Consumes_Produces(inputs, outputs);
            if (!building.Is_Operational) {
                return;
            }
            building.Process(inputs, outputs, delta_time);
        }, null, null, new List<Resource>() { Resource.Iron_Bars, Resource.Steel_Bars, Resource.Lumber, Resource.Charcoal, Resource.Coal, Resource.Firewood }, new List<Resource>() { Resource.Tools, Resource.Mechanisms }, -1.00f, 4.0f));
        prototypes.First(x => x.Internal_Name == "blacksmith").Special_Settings.Add(new SpecialSetting("fuel", "Fuel", SpecialSetting.SettingType.Dropdown, 0, false, new List<string>() {
            Resource.Firewood.UI_Name + " (5/day)",
            Resource.Charcoal.UI_Name + " (2.5/day)",
            Resource.Coal.UI_Name + " (2.5/day)"
        }, 0));
        prototypes.First(x => x.Internal_Name == "blacksmith").Special_Settings.Add(new SpecialSetting("production", "Production", SpecialSetting.SettingType.Dropdown, 0, false, new List<string>() {
            Resource.Tools.UI_Name + " (s) (40/day)",
            Resource.Tools.UI_Name + " (i) (10/day)",
            Resource.Mechanisms.UI_Name + " (s) (20/day)",
            Resource.Mechanisms.UI_Name + " (i) (5/day)"
        }, 0));

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
        prototypes.First(x => x.Internal_Name == "potato_field").Tags.Add(Building.Tag.No_Notification_On_Build);

        prototypes.Add(new Building("Brewery", "brewery", Building.UI_Category.Agriculture, "brewery", Building.BuildingSize.s2x2, 90, new Dictionary<Resource, int>() {
            { Resource.Lumber, 120 }, { Resource.Stone, 15 }, { Resource.Tools, 15 }
        }, 110, new List<Resource>(), 0, 50.0f, 135, new Dictionary<Resource, float>() { { Resource.Lumber, 0.05f } }, 1.00f, 0.0f, 0, new Dictionary<Building.Resident, int>() {
        { Building.Resident.Peasant, 5 }, { Building.Resident.Citizen, 5 } }, 5, true, false, true, 0.0f, 7, null, delegate (Building building, float delta_time) {
            int production = building.Special_Settings.First(x => x.Name == "production").Dropdown_Selection;
            if(production == 0) {
                if (!building.Consumes.Contains(Resource.Potatoes)) {
                    building.Consumes.Add(Resource.Potatoes);
                }
                if (building.Consumes.Contains(Resource.Wheat)) {
                    building.Consumes.Remove(Resource.Wheat);
                }
            } else {
                if (building.Consumes.Contains(Resource.Potatoes)) {
                    building.Consumes.Remove(Resource.Potatoes);
                }
                if (!building.Consumes.Contains(Resource.Wheat)) {
                    building.Consumes.Add(Resource.Wheat);
                }
            }
            if (!building.Is_Operational) {
                return;
            }
            if (production == 0) {
                building.Process(new Dictionary<Resource, float>() { { Resource.Potatoes, 5.0f }, { Resource.Barrels, 2.5f } }, new Dictionary<Resource, float>() { { Resource.Ale, 5.0f } }, delta_time);
            } else {
                building.Process(new Dictionary<Resource, float>() { { Resource.Wheat, 5.0f }, { Resource.Barrels, 2.5f } }, new Dictionary<Resource, float>() { { Resource.Beer, 5.0f } }, delta_time);
            }
        }, null, null, new List<Resource>() { Resource.Potatoes, Resource.Wheat, Resource.Barrels }, new List<Resource>() { Resource.Ale, Resource.Beer }, 0.0f, 0.0f));
        prototypes.First(x => x.Internal_Name == "brewery").Special_Settings.Add(new SpecialSetting("production", "Production", SpecialSetting.SettingType.Dropdown, 0.0f, false, new List<string>() {
            Resource.Ale.UI_Name + " from " + Resource.Potatoes.UI_Name.ToLower(), Resource.Beer.UI_Name + " from " + Resource.Wheat.UI_Name.ToLower() }, 0));

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
            bool alcohol_selected = false;
            foreach(Resource resource in tavern.Consumes) {
                if (resource.Tags.Contains(Resource.ResourceTag.Alcohol)) {
                    alcohol_selected = true;
                    break;
                }
            }
            if (!alcohol_selected) {
                tavern.Show_Alert("alert_no_resources");
                return;
            }
            if (!tavern.Is_Operational || tavern.Efficency == 0.0f) {
                return;
            }
            float income = 0.0f;
            List<Residence> residences = new List<Residence>();
            float alcohol_needed = 0.0f;
            foreach (Building building in tavern.Get_Connected_Buildings(tavern.Road_Range).Select(x => x.Key).ToArray()) {
                if (!(building is Residence)) {
                    continue;
                }
                Residence residence = building as Residence;
                alcohol_needed += residence.Service_Needed(Residence.ServiceType.Tavern) * Residence.RESOURCES_FOR_FULL_SERVICE;
                residences.Add(residence);
            }
            if (residences.Count == 0 || alcohol_needed == 0.0f) {
                return;
            }
            
            float fuel = tavern.Input_Storage[selected_fuel];
            float fuel_used_per_day = selected_fuel == Resource.Firewood ? 0.5f : 0.25f;
            tavern.Input_Storage[selected_fuel] = Mathf.Max(0.0f, tavern.Input_Storage[selected_fuel] - Building.Calculate_Actual_Amount(fuel_used_per_day, delta_time));
            tavern.Update_Delta(selected_fuel, -fuel_used_per_day);
            fuel = tavern.Input_Storage[selected_fuel];

            if (alcohol_needed > 0.0f) {
                float food_needed = alcohol_needed * 0.1f;
                float total_food = 0.0f;
                float total_alcohol = 0.0f;
                Dictionary<Resource, float> alcohol_ratios = new Dictionary<Resource, float>();
                foreach (KeyValuePair<Resource, float> pair in tavern.Input_Storage) {
                    if (pair.Key.Is_Food) {
                        total_food += pair.Value;
                    } else if (pair.Key.Tags.Contains(Resource.ResourceTag.Alcohol)) {
                        total_alcohol += pair.Value;
                    }
                }
                if (total_alcohol > 0.0f && fuel > 0.0f) {
                    foreach (KeyValuePair<Resource, float> pair in tavern.Input_Storage) {
                        if (pair.Key.Tags.Contains(Resource.ResourceTag.Alcohol)) {
                            alcohol_ratios.Add(pair.Key, pair.Value / total_alcohol);
                        }
                    }
                    float alcohol_ratio = Math.Min(1.0f, total_alcohol / alcohol_needed);
                    float food_ratio = Math.Min(1.0f, total_food / food_needed);
                    float food_used = 0.0f;
                    float alcohol_used = 0.0f;
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
                        float disparity = unique_food_count != 0.0f ? (1.0f / unique_food_count) - min_food_ratio : 1.0f;
                        food_quality = Math.Max(unique_food_count * (1.0f - disparity), 0.0f);
                        food_quality = (Mathf.Pow(food_quality, 0.5f) + ((food_quality - 1.0f) * 0.1f)) / 4.0f;
                        food_quality *= ((tavern.Efficency + 2.0f) / 3.0f);
                        food_quality *= 1.5f;
                        food_quality = Mathf.Clamp01(food_quality);
                        if (!(has_meat && has_vegetables)) {
                            food_quality *= 0.5f;
                        }
                    }
                    foreach (Residence residence in residences) {
                        float alcohol_for_residence = (residence.Service_Needed(Residence.ServiceType.Tavern) * Residence.RESOURCES_FOR_FULL_SERVICE) * alcohol_ratio;
                        alcohol_used += alcohol_for_residence;
                        food_used += ((alcohol_for_residence * 0.1f) * food_ratio);
                        residence.Serve(Residence.ServiceType.Tavern, residence.Service_Needed(Residence.ServiceType.Tavern) * alcohol_ratio, tavern.Efficency * ((food_quality + 1.0f) / 2.0f));
                    }

                    foreach(KeyValuePair<Resource, float> ratio in alcohol_ratios) {
                        float type_used = alcohol_used * ratio.Value;
                        tavern.Input_Storage[ratio.Key] -= type_used;
                        income += ratio.Key.Value * type_used;
                        tavern.Update_Delta(ratio.Key, (-type_used / delta_time) * TimeManager.Instance.Days_To_Seconds(1.0f, 1.0f), false);
                        tavern.Check_Input_Storage(ratio.Key);
                    }

                    if (food_ratios != null) {
                        foreach (KeyValuePair<Resource, float> pair in food_ratios) {
                            tavern.Input_Storage[pair.Key] -= pair.Value * food_used;
                            tavern.Update_Delta(pair.Key, (-(pair.Value * food_used) / delta_time) * TimeManager.Instance.Days_To_Seconds(1.0f, 1.0f), false);
                            income += pair.Key.Value * (pair.Value * food_used);
                            tavern.Check_Input_Storage(pair.Key);
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
        }, null, null, new List<Resource>() { Resource.Berries, Resource.Roots, Resource.Mushrooms, Resource.Firewood, Resource.Charcoal, Resource.Coal, Resource.Game, Resource.Bread, Resource.Potatoes, Resource.Ale, Resource.Mutton, Resource.Corn, Resource.Fish, Resource.Bananas, Resource.Oranges, Resource.Grapes, Resource.Beer, Resource.Rum, Resource.Wine, Resource.Salted_Fish, Resource.Salted_Meat, Resource.Lobsters }, new List<Resource>(), 0.05f, 3.0f));
        prototypes.First(x => x.Internal_Name == "tavern").Special_Settings.Add(new SpecialSetting("fuel", "Fuel", SpecialSetting.SettingType.Dropdown, 0, false, new List<string>() { Resource.Firewood.UI_Name + " (0.5/day)", Resource.Charcoal.UI_Name +  " (0.25/day)", Resource.Coal.UI_Name + " (0.25/day)" }, 0));
        foreach (Resource resource in Resource.All.OrderByDescending(x => x.Tags.Contains(Resource.ResourceTag.Alcohol) ? 1.0f : 0.0f).ToArray()) {
            if (resource.Is_Food || resource.Tags.Contains(Resource.ResourceTag.Alcohol)) {
                prototypes.First(x => x.Internal_Name == "tavern").Special_Settings.Add(new SpecialSetting(resource.ToString().ToLower(), string.Format("Serve {0}", resource.UI_Name.ToLower()), SpecialSetting.SettingType.Toggle, 0.0f, (resource.Tags.Contains(Resource.ResourceTag.Alcohol) || (resource.Is_Food && resource.Food_Type != Resource.FoodType.Delicacy))));
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
            { Resource.Wood, 110 }, { Resource.Lumber, 110 }, { Resource.Stone, 20 }, { Resource.Tools, 15 }
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
        prototypes.First(x => x.Internal_Name == "townhouse").Tags.Add(Building.Tag.Random_Sprite);

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
                    Residence residence = building as Residence;
                    if(residence.Taxed_By != -1 && residence.Taxed_By != office.Id) {
                        continue;
                    }
                    residence.Serve(Residence.ServiceType.Taxes, 1.0f, tax_rate_multiplier);
                    residence.Taxed_By = office.Id;
                    foreach(Building.Resident resident in Enum.GetValues(typeof(Building.Resident))) {
                        income += residence.Current_Residents[resident] * base_tax_income[resident] * tax_rate_multiplier;
                    }
                }
            }
            income *= office.Efficency;
            if (income != 0.0f) {
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
            float fuel_amount = selected_fuel == Resource.Firewood ? 5.0f : 2.5f;
            switch (production_selection) {
                case 0:
                    inputs.Add(Resource.Lumber, 5.0f);
                    outputs.Add(Resource.Barrels, 10.0f);
                    break;
                case 1:
                    inputs.Add(Resource.Lumber, 10.0f);
                    inputs.Add(Resource.Leather, 1.0f);
                    outputs.Add(Resource.Furniture, 2.0f);
                    break;
                case 2:
                    inputs.Add(Resource.Iron_Bars, 10.0f);
                    inputs.Add(selected_fuel, fuel_amount);
                    outputs.Add(Resource.Mechanisms, 5.0f);
                    break;
                case 3:
                    inputs.Add(Resource.Copper_Bars, 15.0f);
                    inputs.Add(selected_fuel, fuel_amount);
                    outputs.Add(Resource.Mechanisms, 5.0f);
                    break;
                case 4:
                    inputs.Add(Resource.Wood, 5.0f);
                    outputs.Add(Resource.Firewood, 5.0f);
                    break;
                case 5:
                    inputs.Add(Resource.Lumber, 5.0f);
                    outputs.Add(Resource.Firewood, 5.0f);
                    break;
                case 6:
                    inputs.Add(Resource.Wood, 5.0f);
                    outputs.Add(Resource.Lumber, 2.5f);
                    break;
            }
            building.Update_Consumes_Produces(inputs, outputs);
            if (!building.Is_Operational) {
                return;
            }
            building.Process(inputs, outputs, delta_time);
        }, null, null, new List<Resource>() { Resource.Wood, Resource.Lumber, Resource.Iron_Bars, Resource.Copper_Bars, Resource.Firewood, Resource.Charcoal, Resource.Coal, Resource.Leather }, new List<Resource>() { Resource.Barrels, Resource.Lumber, Resource.Firewood, Resource.Mechanisms }, -0.10f, 4.0f));
        prototypes.First(x => x.Internal_Name == "workshop").Special_Settings.Add(new SpecialSetting("production", "Production", SpecialSetting.SettingType.Dropdown, 0.0f, false, new List<string>() {
            Resource.Barrels.UI_Name + " (10/day)", Resource.Furniture.UI_Name + " (2/day)", Resource.Mechanisms.UI_Name + " (i) (5/day)", Resource.Mechanisms.UI_Name + " (c) (5/day)", Resource.Firewood.UI_Name + " (w) (5/day)", Resource.Firewood.UI_Name + " (l) (5/day)", Resource.Lumber + " (2.5/day)" }, 0));
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
        prototypes.First(x => x.Internal_Name == "tailors_shop").Special_Settings.Add(new SpecialSetting("production", "Production", SpecialSetting.SettingType.Dropdown, 0, false, new List<string>() { Resource.Simple_Clothes.UI_Name + " (2.5/day)", Resource.Leather_Clothes.UI_Name + " (2.5/day)" }, 0));

        prototypes.Add(new Building("Farmhouse", "farmhouse", Building.UI_Category.Agriculture, "farmhouse", Building.BuildingSize.s3x3, 200, new Dictionary<Resource, int>() {
            { Resource.Lumber, 265 }, { Resource.Stone, 25 }, { Resource.Tools, 20 }
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
        prototypes.First(x => x.Internal_Name == "wheat_field").Tags.Add(Building.Tag.No_Notification_On_Build);

        prototypes.Add(new Building("Corn Field", "corn_field", Building.UI_Category.Agriculture, "corn_field", Building.BuildingSize.s1x1, 50, new Dictionary<Resource, int>() {
            { Resource.Tools, 1 }
        }, 10, new List<Resource>(), 0, 0.0f, 25, new Dictionary<Resource, float>(), 0.0f, 0.0f, 0, new Dictionary<Building.Resident, int>(), 0, false, false, false, 0.0f, 0, null, null, null, null,
        new List<Resource>(), new List<Resource>(), 0.0f, 0.0f));
        prototypes.First(x => x.Internal_Name == "corn_field").Tags.Add(Building.Tag.Does_Not_Block_Wind);
        prototypes.First(x => x.Internal_Name == "corn_field").Tags.Add(Building.Tag.No_Notification_On_Build);

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
            { Resource.Lumber, 50 }, { Resource.Stone, 60 }, { Resource.Bricks, 100 }, { Resource.Tools, 25 }
        }, 225, new List<Resource>(), 0, 50.0f, 230, new Dictionary<Resource, float>() { { Resource.Bricks, 0.05f }, { Resource.Lumber, 0.01f } }, 1.25f, 0.0f, 0, new Dictionary<Building.Resident, int>() {
            { Building.Resident.Citizen, 5 } }, 5, true, false, true, 0.0f, 6, null, delegate (Building building, float delta_time) {
                List<Resource> fuel_types = new List<Resource>() { Resource.Firewood, Resource.Charcoal, Resource.Coal };
                int production = building.Special_Settings.First(x => x.Name == "production").Dropdown_Selection;
                Resource selected_fuel = fuel_types[building.Special_Settings.First(x => x.Name == "fuel").Dropdown_Selection];
                foreach (Resource fuel_type in fuel_types) {
                    if (fuel_type != selected_fuel && building.Consumes.Contains(fuel_type)) {
                        building.Consumes.Remove(fuel_type);
                    }
                }
                if (!building.Consumes.Contains(selected_fuel)) {
                    building.Consumes.Add(selected_fuel);
                }
                if(production == 0 && building.Consumes.Contains(Resource.Salt)) {
                    building.Consumes.Remove(Resource.Salt);
                } else if(production == 1 && !building.Consumes.Contains(Resource.Salt)) {
                    building.Consumes.Add(Resource.Salt);
                }
                if (!building.Is_Operational) {
                    return;
                }
                float fuel_usage = selected_fuel == Resource.Firewood ? 2.5f : 1.25f;
                if (production == 0) {
                    building.Process(new Dictionary<Resource, float>() { { Resource.Flour, 10.0f }, { selected_fuel, fuel_usage } }, new Dictionary<Resource, float>() { { Resource.Bread, 30.0f } }, delta_time);
                } else {
                    building.Process(new Dictionary<Resource, float>() { { Resource.Flour, 10.0f }, { Resource.Salt, 5.0f }, { selected_fuel, fuel_usage } }, new Dictionary<Resource, float>() { { Resource.Pretzels, 5.0f } }, delta_time);
                }
            }, null, null, new List<Resource>() { Resource.Flour, Resource.Firewood, Resource.Charcoal, Resource.Coal, Resource.Salt }, new List<Resource>() { Resource.Bread, Resource.Pretzels }, 0.0f, 0.0f));
        prototypes.First(x => x.Internal_Name == "bakery").Special_Settings.Add(new SpecialSetting("production", "Production", SpecialSetting.SettingType.Dropdown, 0.0f, false, new List<string>() { Resource.Bread.UI_Name + " (30/day)", Resource.Pretzels.UI_Name + " (5/day)" }, 0));
        prototypes.First(x => x.Internal_Name == "bakery").Special_Settings.Add(new SpecialSetting("fuel", "Fuel", SpecialSetting.SettingType.Dropdown, 0, false, new List<string>() { Resource.Firewood.UI_Name + " (2.5/day)", Resource.Charcoal.UI_Name + " (1.25/day)", Resource.Coal.UI_Name + " (1.25/day)" }, 0));

        prototypes.Add(new Building("Trading Post", "trading_post", Building.UI_Category.Admin, "trading_post_1", Building.BuildingSize.s3x3, 190, new Dictionary<Resource, int>() {
             { Resource.Lumber, 250 }, { Resource.Stone, 20 }, { Resource.Bricks, 50 }, { Resource.Tools, 15 }
        }, 450, new List<Resource>(), 0, 0.0f, 320, new Dictionary<Resource, float>() { { Resource.Lumber, 0.05f } }, 3.00f, 0.0f, 0.0f, new Dictionary<Building.Resident, int>() { { Building.Resident.Citizen, 10 }, { Building.Resident.Noble, 5 } }, 10,
        true, false, true, 0.0f, 0, null, delegate(Building building, float delta_time) {
            building.Trade_Route_Settings.Caravan_Cooldown -= TimeManager.Instance.Seconds_To_Days(delta_time, 1.0f);
            bool trade = false;
            if(building.Trade_Route_Settings.Caravan_Cooldown <= 0.0f) {
                building.Trade_Route_Settings.Caravan_Cooldown += TradeRouteSettings.CARAVAN_INTERVAL;
                trade = true;
            }
            building.Consumes.Clear();
            building.Produces.Clear();
            if (building.Trade_Route_Settings.Set) {
                if(building.Trade_Route_Settings.Action == TradeRouteSettings.TradeAction.Buy) {
                    building.Produces.Add(building.Trade_Route_Settings.Resource);
                } else {
                    building.Consumes.Add(building.Trade_Route_Settings.Resource);
                }
            }
            if (!building.Is_Operational) {
                return;
            }
            if (!City.Instance.Has_Outside_Road_Connection()) {
                building.Show_Alert("alert_general");
                return;
            }
            if (building.Trade_Route_Settings.Set && trade) {
                building.Trade_Route_Settings.Validate();
                if (building.Trade_Route_Settings.Set) {
                    float amount = building.Trade_Route_Settings.Effective_Amount;//TODO:? * (building.Trade_Route_Settings.Caravan_Cooldown / TradeRouteSettings.CARAVAN_INTERVAL);
                    if (building.Trade_Route_Settings.Action == TradeRouteSettings.TradeAction.Buy) {
                        if (!building.Output_Storage.ContainsKey(building.Trade_Route_Settings.Resource)) {
                            building.Output_Storage.Add(building.Trade_Route_Settings.Resource, 0.0f);
                        }
                        amount = Math.Min(amount, Building.INPUT_OUTPUT_STORAGE_LIMIT - building.Output_Storage[building.Trade_Route_Settings.Resource]);
                        building.Output_Storage[building.Trade_Route_Settings.Resource] += amount;
                        City.Instance.Take_Cash(amount * building.Trade_Route_Settings.Partner.Get_Export_Price(building.Trade_Route_Settings.Resource));
                        float opinion_boost = (amount * building.Trade_Route_Settings.Resource.Value) / 10000.0f;
                        if(building.Trade_Route_Settings.Partner.Opinion > 0.5f && building.Trade_Route_Settings.Partner.Opinion <= 0.75f) {
                            opinion_boost *= 0.5f;
                        } else if(building.Trade_Route_Settings.Partner.Opinion > 0.75f) {
                            opinion_boost *= 0.1f;
                        }
                        building.Trade_Route_Settings.Partner.Improve_Opinion(opinion_boost);
                    } else {
                        if (!building.Input_Storage.ContainsKey(building.Trade_Route_Settings.Resource)) {
                            building.Input_Storage.Add(building.Trade_Route_Settings.Resource, 0.0f);
                        }
                        amount = Math.Min(amount, building.Input_Storage[building.Trade_Route_Settings.Resource]);
                        building.Input_Storage[building.Trade_Route_Settings.Resource] -= amount;
                        City.Instance.Add_Cash(amount * building.Trade_Route_Settings.Partner.Get_Import_Price(building.Trade_Route_Settings.Resource));
                        float opinion_boost = (amount * building.Trade_Route_Settings.Resource.Value) / 10000.0f;
                        if (building.Trade_Route_Settings.Partner.Opinion > 0.5f && building.Trade_Route_Settings.Partner.Opinion <= 0.75f) {
                            opinion_boost *= 0.5f;
                        } else if (building.Trade_Route_Settings.Partner.Opinion > 0.75f) {
                            opinion_boost *= 0.1f;
                        }
                        building.Trade_Route_Settings.Partner.Improve_Opinion(opinion_boost);
                    }
                }
            }
        }, null, null, new List<Resource>(), new List<Resource>(), 0.0f, 0.0f));
        prototypes.First(x => x.Internal_Name == "trading_post").Tags.Add(Building.Tag.Land_Trade);

        prototypes.Add(new Building("Trade Harbor", "trade_harbor", Building.UI_Category.Admin, "trade_harbor", Building.BuildingSize.s3x3, 175, new Dictionary<Resource, int>() {
             { Resource.Lumber, 225 }, { Resource.Stone, 95 }, { Resource.Wood, 40 }, { Resource.Tools, 15 }, { Resource.Mechanisms, 5 }
        }, 475, new List<Resource>(), 0, 0.0f, 365, new Dictionary<Resource, float>() { { Resource.Lumber, 0.05f }, { Resource.Stone, 0.01f } }, 3.00f, 0.0f, 0.0f, new Dictionary<Building.Resident, int>() { { Building.Resident.Citizen, 10 }, { Building.Resident.Noble, 5 } }, 10,
        true, false, true, 0.0f, 0, On_Harbor_Built,
        delegate (Building building, float delta_time) {
            building.Trade_Route_Settings.Caravan_Cooldown -= TimeManager.Instance.Seconds_To_Days(delta_time, 1.0f);
            bool trade = false;
            if (building.Trade_Route_Settings.Caravan_Cooldown <= 0.0f) {
                building.Trade_Route_Settings.Caravan_Cooldown += TradeRouteSettings.CARAVAN_INTERVAL;
                trade = true;
            }
            building.Consumes.Clear();
            building.Produces.Clear();
            if (building.Trade_Route_Settings.Set) {
                if (building.Trade_Route_Settings.Action == TradeRouteSettings.TradeAction.Buy) {
                    building.Produces.Add(building.Trade_Route_Settings.Resource);
                } else {
                    building.Consumes.Add(building.Trade_Route_Settings.Resource);
                }
            }
            if (!building.Is_Operational) {
                return;
            }
            
            if (!building.Has_Functional_Dock()) {
                building.Show_Alert("alert_general");
                return;
            }

            if (building.Trade_Route_Settings.Set && trade) {
                building.Trade_Route_Settings.Validate();
                if (building.Trade_Route_Settings.Set) {
                    float amount = building.Trade_Route_Settings.Effective_Amount;//TODO:? * (building.Trade_Route_Settings.Caravan_Cooldown / TradeRouteSettings.CARAVAN_INTERVAL);
                    if (building.Trade_Route_Settings.Action == TradeRouteSettings.TradeAction.Buy) {
                        if (!building.Output_Storage.ContainsKey(building.Trade_Route_Settings.Resource)) {
                            building.Output_Storage.Add(building.Trade_Route_Settings.Resource, 0.0f);
                        }
                        amount = Math.Min(amount, Building.INPUT_OUTPUT_STORAGE_LIMIT - building.Output_Storage[building.Trade_Route_Settings.Resource]);
                        building.Output_Storage[building.Trade_Route_Settings.Resource] += amount;
                        City.Instance.Take_Cash(amount * building.Trade_Route_Settings.Partner.Get_Export_Price(building.Trade_Route_Settings.Resource));
                        float opinion_boost = (amount * building.Trade_Route_Settings.Resource.Value) / 10000.0f;
                        if (building.Trade_Route_Settings.Partner.Opinion > 0.5f && building.Trade_Route_Settings.Partner.Opinion <= 0.75f) {
                            opinion_boost *= 0.5f;
                        } else if (building.Trade_Route_Settings.Partner.Opinion > 0.75f) {
                            opinion_boost *= 0.1f;
                        }
                        building.Trade_Route_Settings.Partner.Improve_Opinion(opinion_boost);
                    } else {
                        if (!building.Input_Storage.ContainsKey(building.Trade_Route_Settings.Resource)) {
                            building.Input_Storage.Add(building.Trade_Route_Settings.Resource, 0.0f);
                        }
                        amount = Math.Min(amount, building.Input_Storage[building.Trade_Route_Settings.Resource]);
                        building.Input_Storage[building.Trade_Route_Settings.Resource] -= amount;
                        City.Instance.Add_Cash(amount * building.Trade_Route_Settings.Partner.Get_Import_Price(building.Trade_Route_Settings.Resource));
                        float opinion_boost = (amount * building.Trade_Route_Settings.Resource.Value) / 10000.0f;
                        if (building.Trade_Route_Settings.Partner.Opinion > 0.5f && building.Trade_Route_Settings.Partner.Opinion <= 0.75f) {
                            opinion_boost *= 0.5f;
                        } else if (building.Trade_Route_Settings.Partner.Opinion > 0.75f) {
                            opinion_boost *= 0.1f;
                        }
                        building.Trade_Route_Settings.Partner.Improve_Opinion(opinion_boost);
                    }
                }
            }
        }, On_Harbor_Deconstruct, null, new List<Resource>(), new List<Resource>(), 0.0f, 0.0f));
        prototypes.First(x => x.Internal_Name == "trade_harbor").Tags.Add(Building.Tag.Water_Trade);
        prototypes.First(x => x.Internal_Name == "trade_harbor").On_Build_Check = On_Harbor_Build_Check;
        
        prototypes.Add(new Building("Dock", "dock", Building.UI_Category.Unbuildable, "dock_n", Building.BuildingSize.s1x1, 10, new Dictionary<Resource, int>() {
            { Resource.Lumber, 10 }, { Resource.Wood, 10 }, { Resource.Stone, 1 }
        }, 50, new List<Resource>(), 0, 0.0f, 0, new Dictionary<Resource, float>(), 0.0f, 0.0f, 0, new Dictionary<Building.Resident, int>(), 0, false, false, false, 0.0f, 0, null, null, null, null,
        new List<Resource>(), new List<Resource>(), 0.0f, 0.0f));
        prototypes.First(x => x.Internal_Name == "dock").Sprites.Add(new SpriteData("dock_e"));
        prototypes.First(x => x.Internal_Name == "dock").Sprites.Add(new SpriteData("dock_s"));
        prototypes.First(x => x.Internal_Name == "dock").Sprites.Add(new SpriteData("dock_w"));
        prototypes.First(x => x.Internal_Name == "dock").Tags.Add(Building.Tag.Dock);
        prototypes.First(x => x.Internal_Name == "dock").Tags.Add(Building.Tag.Does_Not_Block_Wind);
        prototypes.First(x => x.Internal_Name == "dock").Tags.Add(Building.Tag.Does_Not_Disrupt_Hunting);
        prototypes.First(x => x.Internal_Name == "dock").Tags.Add(Building.Tag.No_Notification_On_Build);
        prototypes.First(x => x.Internal_Name == "dock").Tags.Add(Building.Tag.Undeletable);

        prototypes.Add(new Building("Embassy", "embassy", Building.UI_Category.Admin, "embassy", Building.BuildingSize.s2x2, 225, new Dictionary<Resource, int>() {
            { Resource.Lumber, 75 }, { Resource.Bricks, 160 }, { Resource.Marble, 75 }, { Resource.Tools, 20 }
        }, 600, new List<Resource>(), 0, 0.0f, 310, new Dictionary<Resource, float>() { { Resource.Bricks, 0.05f }, { Resource.Lumber, 0.01f }, { Resource.Marble, 0.01f } }, 5.00f, 0.0f, 0, new Dictionary<Building.Resident, int>() {
            { Building.Resident.Citizen, 10 }, { Building.Resident.Noble, 20 } }, 20, true, false, true, 0.0f, 0, null, delegate (Building building, float delta_time) {
                if (!building.Is_Operational) {
                    return;
                }
                if (!City.Instance.Has_Outside_Road_Connection()) {
                    building.Show_Alert("alert_general");
                    return;
                }
                int dropdown_index = building.Special_Settings.First(x => x.Name == "target").Dropdown_Selection;
                if(dropdown_index == 0) {
                    return;
                }
                ForeignCity target = Contacts.Instance.Cities.OrderBy(x => x.Id).ToArray()[dropdown_index - 1];
                if(target.Opinion == 1.0f) {
                    building.Show_Alert("alert_general");
                    return;
                }
                float multiplier = 1.0f;
                if(building.Current_Workers[Building.Resident.Noble] == 1) {
                    multiplier += 0.1f;
                } else if(building.Current_Workers[Building.Resident.Noble] > 1) {
                    multiplier += 0.35f;
                }
                if(target.Opinion >= 0.50f) {
                    multiplier -= 0.20f;
                } else if(target.Opinion > 0.35f) {
                    multiplier -= 0.15f;
                } else if(target.Opinion < -0.50f) {
                    multiplier -= 0.25f;
                }
                float opinion_bonus = ((delta_time * building.Efficency) / 1500.0f) * multiplier;
                target.Improve_Opinion(opinion_bonus);
            }, null, null, new List<Resource>(), new List<Resource>(), 0.10f, 5.0f));
        prototypes.First(x => x.Internal_Name == "embassy").Tags.Add(Building.Tag.Unique);
        List<string> embassy_options = new List<string>() { "None" };
        foreach(ForeignCity city in Contacts.Instance.Cities.OrderBy(x => x.Id).ToArray()) {
            embassy_options.Add(city.Name);
        }
        prototypes.First(x => x.Internal_Name == "embassy").Special_Settings.Add(new SpecialSetting("target", "Target", SpecialSetting.SettingType.Dropdown, 0, false, embassy_options, 0));

        prototypes.Add(new Building("General Store", "general_store", Building.UI_Category.Services, "general_store", Building.BuildingSize.s2x2, 125, new Dictionary<Resource, int>() {
         { Resource.Bricks, 100 }, { Resource.Lumber, 45 }, { Resource.Stone, 20 }, { Resource.Tools, 15 } }, 150, new List<Resource>(), 0, 0.0f, 165, new Dictionary<Resource, float>() {
         { Resource.Bricks, 0.025f }, { Resource.Lumber, 0.025f } }, 1.0f, 0.0f, 0, new Dictionary<Building.Resident, int>() { { Building.Resident.Citizen, 10 } }, 10, true, false, true, 0.0f, 8,
         null, prototypes.First(x => x.Internal_Name == "marketplace").On_Update, null, null, prototypes.First(x => x.Internal_Name == "marketplace").Consumes, new List<Resource>(), 0.0f, 0.0f));
        foreach(SpecialSetting setting in prototypes.First(x => x.Internal_Name == "marketplace").Special_Settings) {
            prototypes.First(x => x.Internal_Name == "general_store").Special_Settings.Add(new SpecialSetting(setting));
        }

        prototypes.Add(new Building("Coffeehouse", "coffeehouse", Building.UI_Category.Services, "coffeehouse", Building.BuildingSize.s2x2, 125, new Dictionary<Resource, int>() {
            { Resource.Lumber, 55 }, { Resource.Stone, 45 }, { Resource.Bricks, 100 }, { Resource.Tools, 20 }
        }, 300, new List<Resource>(), 0, 0.0f, 200, new Dictionary<Resource, float>() { { Resource.Bricks, 0.05f }, { Resource.Lumber, 0.01f } }, 1.75f, 0.0f, 0, new Dictionary<Building.Resident, int>() {
            { Building.Resident.Citizen, 10 } }, 10, true, false, true, 0.0f, 8, null, delegate (Building building, float delta_time) {
                foreach (SpecialSetting setting in building.Special_Settings) {
                    if (setting.Type == SpecialSetting.SettingType.Toggle) {
                        Resource resource = Resource.All.FirstOrDefault(x => x.Type.ToString().ToLower() == setting.Name);
                        if (resource != null) {
                            if (setting.Toggle_Value && !building.Consumes.Contains(resource)) {
                                building.Consumes.Add(resource);
                            } else if (!setting.Toggle_Value && building.Consumes.Contains(resource)) {
                                building.Consumes.Remove(resource);
                            }
                        }
                    }
                }
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
                if (!building.Is_Operational || building.Efficency == 0.0f) {
                    return;
                }
                float income = 0.0f;
                List<Residence> residences = new List<Residence>();
                float coffee_needed = 0.0f;
                foreach (Building b in building.Get_Connected_Buildings(building.Road_Range).Select(x => x.Key).ToArray()) {
                    if (!(b is Residence)) {
                        continue;
                    }
                    Residence residence = b as Residence;
                    coffee_needed += residence.Service_Needed(Residence.ServiceType.Coffeehouse) * Residence.RESOURCES_FOR_FULL_SERVICE;
                    residences.Add(residence);
                }
                if (residences.Count == 0 || coffee_needed == 0.0f) {
                    return;
                }

                float fuel = building.Input_Storage[selected_fuel];
                float fuel_used_per_day = selected_fuel == Resource.Firewood ? 0.5f : 0.25f;
                building.Input_Storage[selected_fuel] = Mathf.Max(0.0f, building.Input_Storage[selected_fuel] - Building.Calculate_Actual_Amount(fuel_used_per_day, delta_time));
                building.Update_Delta(selected_fuel, -fuel_used_per_day);
                fuel = building.Input_Storage[selected_fuel];

                float coffee = building.Input_Storage[Resource.Coffee];
                if(coffee == 0.0f || fuel == 0.0f) {
                    building.Show_Alert("alert_no_resources");
                } else {
                    float coffee_ratio = Math.Min(coffee / coffee_needed, 1.0f);
                    float coffee_used = 0.0f;
                    float pastry_per_coffee_ratio = 0.5f;
                    float pastry_needed = coffee_needed * pastry_per_coffee_ratio;
                    float pastry_total = 0.0f;
                    Dictionary<Resource, float> pastry_ratios = new Dictionary<Resource, float>();
                    foreach(KeyValuePair<Resource, float> pair in building.Input_Storage) {
                        if(pair.Key.Tags.Contains(Resource.ResourceTag.Pastry)) {
                            pastry_total += pair.Value;
                        }
                    }
                    float pastry_quality = 0.0f;
                    if (pastry_total != 0.0f) {
                        foreach (KeyValuePair<Resource, float> pair in building.Input_Storage) {
                            if (pair.Key.Tags.Contains(Resource.ResourceTag.Pastry)) {
                                pastry_ratios.Add(pair.Key, pair.Value / pastry_total);
                            }
                        }
                        foreach (KeyValuePair<Resource, float> pair in pastry_ratios) {
                            pastry_quality += Math.Min(((pair.Value * pastry_ratios.Count) * building.Input_Storage[pair.Key]) / pastry_needed, 1.0f);
                        }
                    }
                    float pastry_ratio = Math.Min(pastry_total / pastry_needed, 1.0f);
                    float pasty_used = 0.0f;
                    float max_pastry_bonus = 0.65f;
                    float pastry_bonus_multiplier = Mathf.Clamp01(pastry_quality * 0.5f);

                    foreach (Residence residence in residences) {
                        float coffee_for_residence = (residence.Service_Needed(Residence.ServiceType.Coffeehouse) * Residence.RESOURCES_FOR_FULL_SERVICE) * coffee_ratio;
                        coffee_used += coffee_for_residence;
                        pasty_used += ((coffee_for_residence * pastry_per_coffee_ratio) * pastry_ratio);
                        residence.Serve(Residence.ServiceType.Coffeehouse, residence.Service_Needed(Residence.ServiceType.Coffeehouse) * coffee_ratio, (1.0f - max_pastry_bonus) + (max_pastry_bonus * pastry_bonus_multiplier));
                    }
                    building.Input_Storage[Resource.Coffee] -= coffee_used;
                    building.Check_Input_Storage(Resource.Coffee);
                    income += coffee_used * Resource.Coffee.Value;
                    building.Update_Delta(Resource.Coffee, (-coffee_used / delta_time) * TimeManager.Instance.Days_To_Seconds(1.0f, 1.0f));

                    if (pasty_used != 0.0f) {
                        foreach (KeyValuePair<Resource, float> pair in pastry_ratios) {
                            float pastry_type_used = pasty_used * pair.Value;
                            building.Input_Storage[pair.Key] -= pastry_type_used;
                            building.Check_Input_Storage(pair.Key);
                            income += pastry_type_used * pair.Key.Value;
                            building.Update_Delta(pair.Key, (-pastry_type_used / delta_time) * TimeManager.Instance.Days_To_Seconds(1.0f, 1.0f));
                        }
                    }
                }

                if (income != 0.0f) {
                    building.Per_Day_Cash_Delta += (income / delta_time) * TimeManager.Instance.Days_To_Seconds(1.0f, 1.0f);
                    City.Instance.Add_Cash(income);
                }
            }, null, null, new List<Resource>() { Resource.Firewood, Resource.Charcoal, Resource.Coal, Resource.Coffee, Resource.Pretzels, Resource.Cake }, new List<Resource>(), 0.05f, 3.0f));
        prototypes.First(x => x.Internal_Name == "coffeehouse").Special_Settings.Add(new SpecialSetting("fuel", "Fuel", SpecialSetting.SettingType.Dropdown, 0, false, new List<string>() { Resource.Firewood.UI_Name + " (0.5/day)", Resource.Charcoal.UI_Name + " (0.25/day)", Resource.Coal.UI_Name + " (0.25/day)" }, 0));
        foreach(Resource resource in Resource.All) {
            if (resource.Tags.Contains(Resource.ResourceTag.Pastry)) {
                prototypes.First(x => x.Internal_Name == "coffeehouse").Special_Settings.Add(new SpecialSetting(resource.ToString().ToLower(), string.Format("Serve {0}", resource.UI_Name.ToLower()), SpecialSetting.SettingType.Toggle, 0.0f, true));
            }
        }

        prototypes.Add(new Building("Homeware Shop", "homeware_shop", Building.UI_Category.Services, "homeware_shop", Building.BuildingSize.s2x2, 125, new Dictionary<Resource, int>() {
            { Resource.Bricks, 80 }, { Resource.Lumber, 65 }, { Resource.Stone, 20 }, { Resource.Tools, 15 } }, 160, new List<Resource>(), 0, 0.0f, 165, new Dictionary<Resource, float>() {
            { Resource.Bricks, 0.025f }, { Resource.Lumber, 0.025f } }, 1.25f, 0.0f, 0, new Dictionary<Building.Resident, int>() { { Building.Resident.Citizen, 5 } }, 5, true, false, true, 0.0f, 8, null, delegate (Building shop, float delta_time) {
                shop.Consumes.Clear();
                shop.Consumes.Add(Resource.Furniture);

                Resource tableware_type = null;
                float tableware_quality = 0.0f;
                if(shop.Special_Settings.First(x => x.Name == "tableware").Dropdown_Selection == 0) {
                    tableware_type = Resource.Pewterware;
                    tableware_quality = 1.0f;
                    shop.Consumes.Add(Resource.Pewterware);
                } else {
                    tableware_type = Resource.Glassware;
                    tableware_quality = 0.5f;
                    shop.Consumes.Add(Resource.Glassware);
                }
                
                if (!shop.Is_Operational) {
                    return;
                }

                float tableware_needed = 0.0f;
                float furniture_needed = 0.0f;
                List<Residence> residences = new List<Residence>();
                foreach (Building building in shop.Get_Connected_Buildings(shop.Road_Range).Select(x => x.Key).ToArray()) {
                    if (!(building is Residence)) {
                        continue;
                    }
                    Residence residence = building as Residence;
                    tableware_needed += residence.Service_Needed(Residence.ServiceType.Tableware) * Residence.RESOURCES_FOR_FULL_SERVICE;
                    furniture_needed += residence.Service_Needed(Residence.ServiceType.Furniture) * Residence.RESOURCES_FOR_FULL_SERVICE;
                    residences.Add(residence);
                }
                float total_tableware = shop.Input_Storage[tableware_type];
                float total_furniture = shop.Input_Storage[Resource.Furniture];
                float income = 0.0f;
                float efficency_multiplier = (shop.Efficency + 1.0f) / 2.0f;
                if(tableware_needed != 0.0f && total_tableware != 0.0f) {
                    float tableware_ratio = Math.Min(1.0f, total_tableware / tableware_needed);
                    float tableware_used = 0.0f;
                    foreach (Residence residence in residences) {
                        float tableware_for_residence = (residence.Service_Needed(Residence.ServiceType.Tableware) * Residence.RESOURCES_FOR_FULL_SERVICE) * tableware_ratio;
                        tableware_used += tableware_for_residence;
                        residence.Serve(Residence.ServiceType.Tableware, residence.Service_Needed(Residence.ServiceType.Tableware) * tableware_ratio, tableware_quality * efficency_multiplier);
                    }
                    shop.Input_Storage[tableware_type] -= tableware_used;
                    shop.Check_Input_Storage(tableware_type);
                    income += tableware_used * tableware_type.Value;
                    shop.Update_Delta(tableware_type, (-tableware_used / delta_time) * TimeManager.Instance.Days_To_Seconds(1.0f, 1.0f));
                }
                if (furniture_needed != 0.0f && total_furniture != 0.0f) {
                    float furniture_ratio = Math.Min(1.0f, total_furniture / furniture_needed);
                    float furniture_used = 0.0f;
                    foreach (Residence residence in residences) {
                        float furniture_for_residence = (residence.Service_Needed(Residence.ServiceType.Furniture) * Residence.RESOURCES_FOR_FULL_SERVICE) * furniture_ratio;
                        furniture_used += furniture_for_residence;
                        residence.Serve(Residence.ServiceType.Furniture, residence.Service_Needed(Residence.ServiceType.Furniture) * furniture_ratio, 1.0f * efficency_multiplier);
                    }
                    shop.Input_Storage[Resource.Furniture] -= furniture_used;
                    shop.Check_Input_Storage(Resource.Furniture);
                    income += furniture_used * Resource.Furniture.Value;
                    shop.Update_Delta(Resource.Furniture, (-furniture_used / delta_time) * TimeManager.Instance.Days_To_Seconds(1.0f, 1.0f));
                }
                if (income != 0.0f) {
                    shop.Per_Day_Cash_Delta += (income / delta_time) * TimeManager.Instance.Days_To_Seconds(1.0f, 1.0f);
                    City.Instance.Add_Cash(income);
                }
        }, null, null, new List<Resource>() { Resource.Pewterware, Resource.Glassware, Resource.Furniture }, new List<Resource>(), 0.0f, 0.0f));

        prototypes.First(x => x.Internal_Name == "homeware_shop").Special_Settings.Add(new SpecialSetting("tableware", "Tableware", SpecialSetting.SettingType.Dropdown, 0, false, new List<string>() {
            Resource.Pewterware.UI_Name + " (100%Q)",
            Resource.Glassware.UI_Name + " (50%Q)"
        }, 0));

        prototypes.Add(new Building("Salting House", "salting_house", Building.UI_Category.Agriculture, "salting_house", Building.BuildingSize.s2x2, 100, new Dictionary<Resource, int>() {
            { Resource.Lumber, 145 }, { Resource.Stone, 20 }, { Resource.Tools, 15 }
        }, 165, new List<Resource>(), 0, 50.0f, 165, new Dictionary<Resource, float>() { { Resource.Lumber, 0.05f } }, 1.00f, 0.0f, 0, new Dictionary<Building.Resident, int>() {
        { Building.Resident.Peasant, 5 }, { Building.Resident.Citizen, 5 } }, 5, true, false, true, 0.0f, 6, null, delegate (Building building, float delta_time) {
            int production_selection = building.Special_Settings.First(x => x.Name == "production").Dropdown_Selection;
            Dictionary<Resource, float> inputs = new Dictionary<Resource, float>();
            Dictionary<Resource, float> outputs = new Dictionary<Resource, float>();
            inputs.Add(Resource.Salt, 5.0f);
            inputs.Add(Resource.Barrels, 2.5f);
            switch (production_selection) {
                case 0:
                    inputs.Add(Resource.Fish, 5.0f);
                    outputs.Add(Resource.Salted_Fish, 5.0f);
                    break;
                case 1:
                    inputs.Add(Resource.Game, 5.0f);
                    outputs.Add(Resource.Salted_Meat, 5.0f);
                    break;
                case 2:
                    inputs.Add(Resource.Mutton, 5.0f);
                    outputs.Add(Resource.Salted_Meat, 5.0f);
                    break;
            }
            building.Update_Consumes_Produces(inputs, outputs);
            if (!building.Is_Operational) {
                return;
            }
            building.Process(inputs, outputs, delta_time);
        }, null, null, new List<Resource>() { Resource.Salt, Resource.Barrels, Resource.Fish, Resource.Game, Resource.Mutton }, new List<Resource>() { Resource.Salted_Fish, Resource.Salted_Meat }, 0.0f, 0.0f));
        prototypes.First(x => x.Internal_Name == "salting_house").Special_Settings.Add(new SpecialSetting("production", "Production", SpecialSetting.SettingType.Dropdown, 0, false, new List<string>() {
            Resource.Salted_Fish.UI_Name + " (5/day)", string.Format("{0} ({1}) (5/day)", Resource.Salted_Meat.UI_Name, Resource.Game.UI_Name), string.Format("{0} ({1}) (5/day)", Resource.Salted_Meat.UI_Name, Resource.Mutton.UI_Name) }, 0));

        prototypes.Add(new Residence("Manor", "manor", Building.UI_Category.Housing, "manor", Building.BuildingSize.s3x3, 150, new Dictionary<Resource, int>() {
            { Resource.Lumber, 290 }, { Resource.Marble, 50 }, { Resource.Stone, 25 }, { Resource.Tools, 15 }
        }, 300, new List<Resource>(), 0, 365, new Dictionary<Resource, float>() { { Resource.Lumber, 0.05f }, { Resource.Marble, 0.01f } }, 1.00f, 0.0f, 0.0f, 0.0f, new Dictionary<Building.Resident, int>() { { Building.Resident.Noble, 5 } }, 0.0f, null, null, null, null, new List<Resource>(), new List<Resource>(), 0.025f, 5.0f));

        prototypes.Add(new Building("Luxury Goods Market", "luxury_goods_market", Building.UI_Category.Services, "luxury_goods_market", Building.BuildingSize.s3x3, 150, new Dictionary<Resource, int>() {
            { Resource.Lumber, 20 }, { Resource.Stone, 45 }, { Resource.Marble, 45 }, { Resource.Tools, 10 }
        }, 250, new List<Resource>(), 0, 0.0f, 110, new Dictionary<Resource, float>() { { Resource.Stone, 0.02f }, { Resource.Marble, 0.02f } }, 3.00f, 0.0f, 0.0f,
        new Dictionary<Building.Resident, int>() { { Building.Resident.Citizen, 10 }, { Building.Resident.Noble, 5 } }, 10, true, true, true, 0.0f, 10, null, delegate(Building market, float delta_time) {
            market.Consumes.Clear();
            foreach (SpecialSetting setting in market.Special_Settings) {
                if (setting.Toggle_Value) {
                    market.Consumes.Add(Resource.All.First(x => x.ToString().ToLower() == setting.Name));
                }
            }
            if (!market.Is_Operational) {
                return;
            }
            float wine_needed = 0.0f;
            float delicacies_needed = 0.0f;
            float jewelry_needed = 0.0f;
            float clothes_needed = 0.0f;
            List<Residence> residences = new List<Residence>();
            foreach (Building building in market.Get_Connected_Buildings(market.Road_Range).Select(x => x.Key).ToArray()) {
                if (!(building is Residence)) {
                    continue;
                }
                Residence residence = building as Residence;
                wine_needed += residence.Service_Needed(Residence.ServiceType.Wine) * Residence.RESOURCES_FOR_FULL_SERVICE;
                delicacies_needed += residence.Service_Needed(Residence.ServiceType.Delicacies) * Residence.RESOURCES_FOR_FULL_SERVICE;
                jewelry_needed += residence.Service_Needed(Residence.ServiceType.Jewelry) * Residence.RESOURCES_FOR_FULL_SERVICE;
                clothes_needed += residence.Service_Needed(Residence.ServiceType.Fine_Clothes) * Residence.RESOURCES_FOR_FULL_SERVICE;
                residences.Add(residence);
            }

            float income = 0.0f;
            float total_wine = market.Input_Storage[Resource.Wine];
            float efficency_multiplier = (market.Efficency + 1.0f) / 2.0f;
            if (wine_needed != 0.0f && total_wine != 0.0f) {
                float wine_ratio = Math.Min(1.0f, total_wine / wine_needed);
                float wine_used = 0.0f;
                foreach (Residence residence in residences) {
                    float wine_for_residence = (residence.Service_Needed(Residence.ServiceType.Wine) * Residence.RESOURCES_FOR_FULL_SERVICE) * wine_ratio;
                    wine_used += wine_for_residence;
                    residence.Serve(Residence.ServiceType.Wine, residence.Service_Needed(Residence.ServiceType.Wine) * wine_ratio, 1.0f * efficency_multiplier);
                }
                market.Input_Storage[Resource.Wine] -= wine_used;
                market.Check_Input_Storage(Resource.Wine);
                income += wine_used * Resource.Wine.Value;
                market.Update_Delta(Resource.Wine, (-wine_used / delta_time) * TimeManager.Instance.Days_To_Seconds(1.0f, 1.0f));
            }

            float total_jewelry = market.Input_Storage[Resource.Simple_Jewelry] + market.Input_Storage[Resource.Fine_Jewelry] + market.Input_Storage[Resource.Opulent_Jewelry];
            if (jewelry_needed != 0.0f && total_jewelry != 0.0f) {
                float jewelry_ratio = Math.Min(total_jewelry / jewelry_needed, 1.0f);
                float normal_ratio = market.Input_Storage[Resource.Simple_Jewelry] / total_jewelry;
                float fine_ratio = market.Input_Storage[Resource.Fine_Jewelry] / total_jewelry;
                float opulent_ratio = market.Input_Storage[Resource.Opulent_Jewelry] / total_jewelry;
                float jewelry_quality = (normal_ratio * (1.0f / 3.0f)) + (fine_ratio * (2.0f / 3.0f)) + (opulent_ratio * 1.0f);
                float jewelry_used = 0.0f;
                foreach (Residence residence in residences) {
                    float jewelry_for_residence = (residence.Service_Needed(Residence.ServiceType.Jewelry) * Residence.RESOURCES_FOR_FULL_SERVICE) * jewelry_ratio;
                    jewelry_used += jewelry_for_residence;
                    residence.Serve(Residence.ServiceType.Jewelry, residence.Service_Needed(Residence.ServiceType.Jewelry) * jewelry_ratio, jewelry_quality * efficency_multiplier);
                }
                float normal_sold = normal_ratio * jewelry_used;
                market.Input_Storage[Resource.Simple_Jewelry] -= normal_sold;
                income += normal_sold * Resource.Simple_Jewelry.Value;
                market.Update_Delta(Resource.Simple_Jewelry, (-normal_sold / delta_time) * TimeManager.Instance.Days_To_Seconds(1.0f, 1.0f));
                float fine_sold = fine_ratio * jewelry_used;
                market.Input_Storage[Resource.Fine_Jewelry] -= fine_sold;
                income += fine_sold * Resource.Fine_Jewelry.Value;
                market.Update_Delta(Resource.Fine_Jewelry, (-fine_sold / delta_time) * TimeManager.Instance.Days_To_Seconds(1.0f, 1.0f));
                float opulent_sold = opulent_ratio * jewelry_used;
                market.Input_Storage[Resource.Opulent_Jewelry] -= opulent_sold;
                income += opulent_sold * Resource.Opulent_Jewelry.Value;
                market.Update_Delta(Resource.Opulent_Jewelry, (-opulent_sold / delta_time) * TimeManager.Instance.Days_To_Seconds(1.0f, 1.0f));
                market.Check_Input_Storage(Resource.Simple_Jewelry);
                market.Check_Input_Storage(Resource.Fine_Jewelry);
                market.Check_Input_Storage(Resource.Opulent_Jewelry);
            }

            float total_delicacies = market.Input_Storage[Resource.Pretzels] + market.Input_Storage[Resource.Cake];
            if (delicacies_needed != 0.0f && total_delicacies != 0.0f) {
                float delicacies_ratio = Math.Min(total_delicacies / delicacies_needed, 1.0f);
                float prezel_ratio = market.Input_Storage[Resource.Pretzels] / total_delicacies;
                float cake_ratio = market.Input_Storage[Resource.Cake] / total_delicacies;
                float delicacy_quality = (prezel_ratio * 0.5f) + (cake_ratio * 1.0f);
                float delicacies_used = 0.0f;
                foreach (Residence residence in residences) {
                    float delicacies_for_residence = (residence.Service_Needed(Residence.ServiceType.Delicacies) * Residence.RESOURCES_FOR_FULL_SERVICE) * delicacies_ratio;
                    delicacies_used += delicacies_for_residence;
                    residence.Serve(Residence.ServiceType.Delicacies, residence.Service_Needed(Residence.ServiceType.Delicacies) * delicacies_ratio, delicacy_quality * efficency_multiplier);
                }
                float prezels_sold = prezel_ratio * delicacies_used;
                market.Input_Storage[Resource.Pretzels] -= prezels_sold;
                income += prezels_sold * Resource.Pretzels.Value;
                market.Update_Delta(Resource.Pretzels, (-prezels_sold / delta_time) * TimeManager.Instance.Days_To_Seconds(1.0f, 1.0f));
                float cake_sold = cake_ratio * delicacies_used;
                market.Input_Storage[Resource.Cake] -= cake_sold;
                income += cake_sold * Resource.Cake.Value;
                market.Update_Delta(Resource.Cake, (-cake_sold / delta_time) * TimeManager.Instance.Days_To_Seconds(1.0f, 1.0f));
                market.Check_Input_Storage(Resource.Pretzels);
                market.Check_Input_Storage(Resource.Cake);
            }

            float total_clothes = market.Input_Storage[Resource.Fine_Clothes] + market.Input_Storage[Resource.Luxury_Clothes];
            if (clothes_needed != 0.0f && total_clothes != 0.0f) {
                float clothing_ratio = Math.Min(total_clothes / clothes_needed, 1.0f);
                float fine_ratio = market.Input_Storage[Resource.Fine_Clothes] / total_clothes;
                float luxury_ratio = market.Input_Storage[Resource.Luxury_Clothes] / total_clothes;
                float clothing_quality = (fine_ratio * 0.5f) + (luxury_ratio * 1.0f);
                float clothing_used = 0.0f;
                foreach (Residence residence in residences) {
                    float clothes_for_residence = (residence.Service_Needed(Residence.ServiceType.Fine_Clothes) * Residence.RESOURCES_FOR_FULL_SERVICE) * clothing_ratio;
                    clothing_used += clothes_for_residence;
                    residence.Serve(Residence.ServiceType.Fine_Clothes, residence.Service_Needed(Residence.ServiceType.Fine_Clothes) * clothing_ratio, clothing_quality * efficency_multiplier);
                }
                float fine_sold = fine_ratio * clothing_used;
                market.Input_Storage[Resource.Fine_Clothes] -= fine_sold;
                income += fine_sold * Resource.Fine_Clothes.Value;
                market.Update_Delta(Resource.Fine_Clothes, (-fine_sold / delta_time) * TimeManager.Instance.Days_To_Seconds(1.0f, 1.0f));
                float luxury_sold = luxury_ratio * clothing_used;
                market.Input_Storage[Resource.Luxury_Clothes] -= luxury_sold;
                income += luxury_sold * Resource.Luxury_Clothes.Value;
                market.Update_Delta(Resource.Luxury_Clothes, (-luxury_sold / delta_time) * TimeManager.Instance.Days_To_Seconds(1.0f, 1.0f));
                market.Check_Input_Storage(Resource.Fine_Clothes);
                market.Check_Input_Storage(Resource.Luxury_Clothes);
            }

            if (income != 0.0f) {
                market.Per_Day_Cash_Delta += (income / delta_time) * TimeManager.Instance.Days_To_Seconds(1.0f, 1.0f);
                City.Instance.Add_Cash(income);
            }
        }, null, null, new List<Resource>(), new List<Resource>(), 0.05f, 7.0f));
        foreach(Resource resource in new List<Resource>() { Resource.Wine, Resource.Pretzels, Resource.Cake, Resource.Simple_Jewelry, Resource.Fine_Jewelry, Resource.Opulent_Jewelry, Resource.Fine_Clothes, Resource.Luxury_Clothes }) {
            prototypes.First(x => x.Internal_Name == "luxury_goods_market").Consumes.Add(resource);
            prototypes.First(x => x.Internal_Name == "luxury_goods_market").Special_Settings.Add(new SpecialSetting(resource.ToString().ToLower(), resource.UI_Name, SpecialSetting.SettingType.Toggle, 0.0f, true));
        }

        prototypes.Add(new Building("Clothier's Shop", "clothiers_shop", Building.UI_Category.Textile, "clothiers_shop", Building.BuildingSize.s2x2, 150, new Dictionary<Resource, int>() {
            { Resource.Lumber, 100 }, { Resource.Stone, 20 }, { Resource.Bricks, 80 }, { Resource.Tools, 25 }
        }, 250, new List<Resource>(), 0, 50.0f, 200, new Dictionary<Resource, float>() { { Resource.Lumber, 0.025f }, { Resource.Bricks, 0.025f } }, 2.85f, 0.0f, 0, new Dictionary<Building.Resident, int>() {
        { Building.Resident.Citizen, 10 } }, 10, true, false, true, 0.0f, 6, null, delegate (Building building, float delta_time) {
            int production_selection = building.Special_Settings.First(x => x.Name == "production").Dropdown_Selection;
            Dictionary<Resource, float> inputs = new Dictionary<Resource, float>();
            Dictionary<Resource, float> outputs = new Dictionary<Resource, float>();
            switch (production_selection) {
                case 0:
                    outputs.Add(Resource.Simple_Clothes, 5.0f);
                    inputs.Add(Resource.Cloth, 5.0f);
                    inputs.Add(Resource.Thread, 3.0f);
                    break;
                case 1:
                    outputs.Add(Resource.Leather_Clothes, 5.0f);
                    inputs.Add(Resource.Cloth, 5.0f);
                    inputs.Add(Resource.Thread, 3.0f);
                    inputs.Add(Resource.Leather, 5.0f);
                    break;
                case 2:
                    outputs.Add(Resource.Fine_Clothes, 2.5f);
                    inputs.Add(Resource.Cloth, 2.5f);
                    inputs.Add(Resource.Thread, 1.75f);
                    inputs.Add(Resource.Leather, 2.5f);
                    inputs.Add(Resource.Furs, 1.5f);
                    break;
                case 3:
                    outputs.Add(Resource.Luxury_Clothes, 2.5f);
                    inputs.Add(Resource.Cloth, 2.0f);
                    inputs.Add(Resource.Thread, 1.75f);
                    inputs.Add(Resource.Leather, 2.0f);
                    inputs.Add(Resource.Furs, 1.5f);
                    inputs.Add(Resource.Silk, 1.5f);
                    break;
            }
            building.Update_Consumes_Produces(inputs, outputs);
            if (!building.Is_Operational) {
                return;
            }
            building.Process(inputs, outputs, delta_time);
        }, null, null, new List<Resource>() { Resource.Thread, Resource.Cloth, Resource.Leather, Resource.Furs, Resource.Silk }, new List<Resource>() { Resource.Simple_Clothes, Resource.Leather_Clothes, Resource.Fine_Clothes, Resource.Luxury_Clothes }, 0.0f, 0.0f));
        prototypes.First(x => x.Internal_Name == "clothiers_shop").Special_Settings.Add(new SpecialSetting("production", "Production", SpecialSetting.SettingType.Dropdown, 0, false, new List<string>() {
            Resource.Simple_Clothes.UI_Name + " (5/day)",
            Resource.Leather_Clothes.UI_Name + " (5/day)",
            Resource.Fine_Clothes.UI_Name + " (2.5/day)",
            Resource.Luxury_Clothes.UI_Name + " (2.5/day)"
        }, 0));

        prototypes.Add(new Building("Vineyard", "vineyard", Building.UI_Category.Agriculture, "vineyard", Building.BuildingSize.s3x3, 150, new Dictionary<Resource, int>() {
            { Resource.Lumber, 75 }, { Resource.Stone, 10 }, { Resource.Bricks, 90 }, { Resource.Tools, 20 }
        }, 200, new List<Resource>(), 0, 0.0f, 175, new Dictionary<Resource, float>() { { Resource.Lumber, 0.02f }, { Resource.Bricks, 0.02f } }, 1.10f, 0.0f, 0, new Dictionary<Building.Resident, int>() { { Building.Resident.Peasant, 10 }, { Building.Resident.Citizen, 5 } }, 10, true, false, true, 3.75f, 0, Reserve_Tiles(Tile.Work_Type.Farm),
        delegate (Building building, float delta_time) {
            if (!building.Is_Operational) {
                return;
            }
            float grapes = 0.0f;
            foreach (Tile tile in building.Get_Tiles_In_Circle(building.Range)) {
                if (tile.Building != null && tile.Building.Internal_Name == "grape_trellis" && tile.Can_Work(building, Tile.Work_Type.Farm)) {
                    if (tile.Internal_Name == "grass") {
                        grapes += 0.10f;
                    } else if (tile.Internal_Name == "fertile_ground") {
                        grapes += 0.15f;
                    }
                }
            }
            float multiplier = 0.735294f;
            building.Produce(Resource.Grapes, grapes * multiplier, delta_time);
        }, unreserve_tiles, Highlight_Tiles(Tile.Work_Type.Farm), new List<Resource>(), new List<Resource>() { Resource.Grapes }, 0.0f, 0.0f));

        prototypes.Add(new Building("Grape Trellis", "grape_trellis", Building.UI_Category.Agriculture, "grape_trellis", Building.BuildingSize.s1x1, 50, new Dictionary<Resource, int>() {
            { Resource.Wood, 1 }, { Resource.Tools, 1 }
        }, 15, new List<Resource>(), 0, 0.0f, 20, new Dictionary<Resource, float>(), 0.0f, 0.0f, 0, new Dictionary<Building.Resident, int>(), 0, false, false, false, 0.0f, 0, null, null, null, null,
        new List<Resource>(), new List<Resource>(), 0.05f, 3.0f));
        prototypes.First(x => x.Internal_Name == "grape_trellis").Tags.Add(Building.Tag.Does_Not_Block_Wind);
        prototypes.First(x => x.Internal_Name == "grape_trellis").Tags.Add(Building.Tag.No_Notification_On_Build);

        prototypes.Add(new Building("Winery", "winery", Building.UI_Category.Agriculture, "winery", Building.BuildingSize.s2x2, 150, new Dictionary<Resource, int>() {
            { Resource.Lumber, 75 }, { Resource.Stone, 20 }, { Resource.Bricks, 95 }, { Resource.Tools, 20 }
        }, 250, new List<Resource>(), 0, 50.0f, 190, new Dictionary<Resource, float>() { { Resource.Lumber, 0.025f }, { Resource.Bricks, 0.025f } }, 1.75f, 0.0f, 0, new Dictionary<Building.Resident, int>() {
            { Building.Resident.Peasant, 10 }, { Building.Resident.Citizen, 5 } }, 10, true, false, true, 0.0f, 6, null, delegate (Building building, float delta_time) {
                if (!building.Is_Operational) {
                    return;
                }
                building.Process(new Dictionary<Resource, float>() { { Resource.Grapes, 5.0f }, { Resource.Glassware, 2.5f } }, new Dictionary<Resource, float>() { { Resource.Wine, 5.0f } }, delta_time);
            }, null, null, new List<Resource>() { Resource.Glassware, Resource.Grapes }, new List<Resource>() { Resource.Wine }, 0.0f, 0.0f));

        prototypes.Add(new Building("Jeweler's Shop", "jeweler", Building.UI_Category.Industry, "jeweler", Building.BuildingSize.s2x2, 150, new Dictionary<Resource, int>() {
            { Resource.Lumber, 50 }, { Resource.Stone, 25 }, { Resource.Bricks, 120 }, { Resource.Tools, 30 }
        }, 350, new List<Resource>(), 0, 50.0f, 195, new Dictionary<Resource, float>() { { Resource.Bricks, 0.05f }, { Resource.Lumber, 0.01f } }, 4.00f, 0.0f, 0, new Dictionary<Building.Resident, int>() {
        { Building.Resident.Citizen, 5 } }, 5, true, false, true, 0.0f, 6, null, delegate (Building building, float delta_time) {
            List<Resource> fuel_types = new List<Resource>() { Resource.Firewood, Resource.Charcoal, Resource.Coal };
            Resource selected_fuel = fuel_types[building.Special_Settings.First(x => x.Name == "fuel").Dropdown_Selection];
            int production = building.Special_Settings.First(x => x.Name == "production").Dropdown_Selection;
            Dictionary<Resource, float> inputs = new Dictionary<Resource, float>();
            Dictionary<Resource, float> outputs = new Dictionary<Resource, float>();

            inputs.Add(selected_fuel, selected_fuel == Resource.Firewood ? 1.0f : 0.5f);
            switch (production) {
                case 0:
                    inputs.Add(Resource.Silver_Bars, 1.0f);
                    outputs.Add(Resource.Simple_Jewelry, 1.0f);
                    break;
                case 1:
                    inputs.Add(Resource.Gold_Bars, 1.0f);
                    outputs.Add(Resource.Simple_Jewelry, 1.5f);
                    break;
                case 2:
                    inputs.Add(Resource.Silver_Bars, 1.0f);
                    inputs.Add(Resource.Gems, 0.5f);
                    outputs.Add(Resource.Fine_Jewelry, 1.0f);
                    break;
                case 3:
                    inputs.Add(Resource.Gold_Bars, 1.0f);
                    inputs.Add(Resource.Gems, 0.5f);
                    outputs.Add(Resource.Opulent_Jewelry, 1.0f);
                    break;
            }
            building.Update_Consumes_Produces(inputs, outputs);
            if (!building.Is_Operational) {
                return;
            }
            building.Process(inputs, outputs, delta_time);
        }, null, null, new List<Resource>() { Resource.Silver_Bars, Resource.Gold_Bars, Resource.Gems, Resource.Charcoal, Resource.Coal, Resource.Firewood }, new List<Resource>() { Resource.Simple_Jewelry, Resource.Fine_Jewelry, Resource.Opulent_Jewelry }, 0.05f, 3.0f));
        prototypes.First(x => x.Internal_Name == "jeweler").Special_Settings.Add(new SpecialSetting("fuel", "Fuel", SpecialSetting.SettingType.Dropdown, 0, false, new List<string>() {
            Resource.Firewood.UI_Name + " (1.0/day)",
            Resource.Charcoal.UI_Name + " (0.5/day)",
            Resource.Coal.UI_Name + " (0.5/day)"
        }, 0));
        prototypes.First(x => x.Internal_Name == "jeweler").Special_Settings.Add(new SpecialSetting("production", "Production", SpecialSetting.SettingType.Dropdown, 0, false, new List<string>() {
            Resource.Simple_Jewelry.UI_Name + " (s) (40/day)",
            Resource.Simple_Jewelry.UI_Name + " (g) (10/day)",
            Resource.Fine_Jewelry.UI_Name + " (20/day)",
            Resource.Opulent_Jewelry.UI_Name + " (5/day)"
        }, 0));

        prototypes.Add(new Building("Shipyard", "shipyard", Building.UI_Category.Coastal, "shipyard", Building.BuildingSize.s3x3, 100, new Dictionary<Resource, int>() {
            { Resource.Wood, 20 }, { Resource.Lumber, 50 }, { Resource.Stone, 25 }, { Resource.Tools, 25 }, { Resource.Mechanisms, 5 }
        }, 200, new List<Resource>(), 0, 0.0f, 100, new Dictionary<Resource, float>() { { Resource.Lumber, 0.01f } }, 3.00f, 0.0f, 0, new Dictionary<Building.Resident, int>() { { Building.Resident.Peasant, 10 }, { Building.Resident.Citizen, 20 } }, 20, true, false, true, 0.0f, 7, null,
        delegate (Building building, float delta_time) {
            int max_ships = 5;
            int ship_count = building.Data.ContainsKey("ship_count") ? int.Parse(building.Data["ship_count"]) : 0;
            building.Special_Status_Text_1 = string.Format("Ships: {0} / {1}", ship_count, max_ships);
            building.Special_Status_Text_2 = null;
            bool ship_access = false;
            if (building.Data.ContainsKey("has_ship_access")) {
                building.Data.Remove("has_ship_access");
            }
            foreach (Tile t in Map.Instance.Get_Tiles_Around(building)) {
                if (t.Is_Water && t.Has_Ship_Access && t.Building == null) {
                    ship_access = true;
                    break;
                }
            }
            if (!ship_access) {
                building.Show_Alert("alert_general");
                return;
            }
            building.Data.Add("has_ship_access", true.ToString());
            if (ship_count >= max_ships) {
                return;
            }
            if (!building.Is_Operational) {
                return;
            }
            bool button_was_pressed = building.Special_Settings.First(x => x.Name == "build_ship_button").Button_Was_Pressed;
            float ship_progress = building.Data.ContainsKey("ship_progress") ? float.Parse(building.Data["ship_progress"]) : -1.0f;
            building.Special_Settings.First(x => x.Name == "build_ship_button").Label = string.Format("Build ship ({0})", ship_count);
            if (!button_was_pressed && ship_progress == -1.0f) {
                //Ship is not being built and a new ship is not ordered
                return;
            }
            if (ship_progress >= 1.0f) {
                //Ship ready
                ship_count++;
                if (building.Data.ContainsKey("ship_count")) {
                    building.Data["ship_count"] = ship_count.ToString();
                } else {
                    building.Data.Add("ship_count", ship_count.ToString());
                }
                building.Data["ship_progress"] = (-1.0f).ToString();
                building.Special_Settings.First(x => x.Name == "build_ship_button").Button_Was_Pressed = false;
                building.Output_Storage[Resource.Ship_Parts] = 0.0f;
                return;
            }
            if (button_was_pressed && ship_progress == -1.0f) {
                //Start a new ship
                ship_progress = 0.0f;
            }

            building.Process(new Dictionary<Resource, float>() { { Resource.Lumber, 10.0f }, { Resource.Cloth, 0.05f }, { Resource.Mechanisms, 0.025f }, { Resource.Tools, 0.025f } },
                new Dictionary<Resource, float>() { { Resource.Ship_Parts, 0.05f } }, delta_time);
            ship_progress = Mathf.Clamp01(building.Output_Storage[Resource.Ship_Parts]);
            building.Special_Status_Text_2 = string.Format("Building: {0}%", Helper.Float_To_String(ship_progress * 100.0f, 0));

            building.Special_Settings.First(x => x.Name == "build_ship_button").Label = string.Format("Building ship {0}% ({1})", Helper.Float_To_String(ship_progress * 100.0f, 0), ship_count);

            if (building.Data.ContainsKey("ship_progress")) {
                building.Data["ship_progress"] = ship_progress.ToString();
            } else {
                building.Data.Add("ship_progress", ship_progress.ToString());
            }
        }, null, null, new List<Resource>() { Resource.Lumber, Resource.Cloth, Resource.Tools, Resource.Mechanisms }, new List<Resource>() { Resource.Ship_Parts }, 0.0f, 0.0f));
        prototypes.First(x => x.Internal_Name == "shipyard").On_Build_Check = delegate (Building building, Tile tile, out string message) {
            message = string.Empty;
            bool ship_access = false;
            foreach (Tile t in Map.Instance.Get_Tiles_Around(building)) {
                if (t.Is_Water && t.Has_Ship_Access && t.Building == null) {
                    ship_access = true;
                    break;
                }
            }
            if (!ship_access) {
                message = "Must be placed on a waterfront with ship access.";
                return false;
            }
            return ship_access;
        };
        prototypes.First(x => x.Internal_Name == "shipyard").Special_Settings.Add(new SpecialSetting("build_ship_button", "Build ship", SpecialSetting.SettingType.Button));

        prototypes.Add(new Building("Expedition Harbor", "expedition_harbor", Building.UI_Category.Admin, "expedition_harbor", Building.BuildingSize.s3x3, 175, new Dictionary<Resource, int>() {
             { Resource.Lumber, 235 }, { Resource.Stone, 90 }, { Resource.Wood, 25 }, { Resource.Tools, 15 }, { Resource.Bricks, 10 }, { Resource.Mechanisms, 5 }
        }, 500, new List<Resource>(), 0, 0.0f, 365, new Dictionary<Resource, float>() { { Resource.Lumber, 0.05f }, { Resource.Stone, 0.01f } }, 5.00f, 0.0f, 0.0f, new Dictionary<Building.Resident, int>() { { Building.Resident.Citizen, 15 }, { Building.Resident.Noble, 10 } }, 20,
        false, false, true, 0.0f, 0, On_Harbor_Built,
        delegate (Building building, float delta_time) {
            List<Resource> clear_resources = new List<Resource>();
            foreach(KeyValuePair<Resource, float> pair in building.Output_Storage) {
                if(pair.Value == 0.0f) {
                    clear_resources.Add(pair.Key);
                }
            }
            foreach(Resource resource in clear_resources) {
                building.Output_Storage.Remove(resource);
            }
            if (!building.Is_Operational) {
                return;
            }
            if (!building.Has_Functional_Dock()) {
                building.Show_Alert("alert_general");
                return;
            }
        }, On_Harbor_Deconstruct, null, new List<Resource>(), new List<Resource>(), 0.0f, 0.0f));
        prototypes.First(x => x.Internal_Name == "expedition_harbor").On_Build_Check = On_Harbor_Ship_Build_Check;
        prototypes.First(x => x.Internal_Name == "expedition_harbor").On_Building_Start = On_Harbor_Ship_Building_Start;
        prototypes.First(x => x.Internal_Name == "expedition_harbor").Tags.Add(Building.Tag.Creates_Expeditions);

        prototypes.Add(new Building("Theatre", "theatre", Building.UI_Category.Services, "theatre", Building.BuildingSize.s3x3, 300, new Dictionary<Resource, int>() {
            { Resource.Lumber, 250 }, { Resource.Stone, 275 }, { Resource.Marble, 25 }, { Resource.Tools, 30 }
        }, 650, new List<Resource>(), 0, 0.0f, 550, new Dictionary<Resource, float>() { { Resource.Stone, 0.05f }, { Resource.Lumber, 0.05f } }, 5.00f, 0.0f, 0, new Dictionary<Building.Resident, int>() {
        { Building.Resident.Citizen, 15 }, { Building.Resident.Noble, 10 } }, 20, true, false, true, 0.0f, 14, null, delegate (Building theatre, float delta_time) {
            if (!theatre.Is_Operational) {
                return;
            }
            foreach (Building building in theatre.Get_Connected_Buildings(theatre.Road_Range).Select(x => x.Key).ToArray()) {
                if (building is Residence) {
                    (building as Residence).Serve(Residence.ServiceType.Theatre, 1.0f, theatre.Efficency);
                }
            }
        }, null, null, new List<Resource>(), new List<Resource>(), 0.01f, 5.0f));

        prototypes.Add(new Building("School House", "school_house", Building.UI_Category.Services, "school_house", Building.BuildingSize.s2x2, 125, new Dictionary<Resource, int>() {
            { Resource.Lumber, 45 }, { Resource.Bricks, 90 }, { Resource.Stone, 20 }, { Resource.Tools, 15 }
        }, 200, new List<Resource>(), 0, 0.0f, 155, new Dictionary<Resource, float>() { { Resource.Bricks, 0.02f }, { Resource.Lumber, 0.01f } }, 2.00f, 0.0f, 0, new Dictionary<Building.Resident, int>() {
        { Building.Resident.Citizen, 5 } }, 5, true, false, true, 0.0f, 5, null, delegate (Building school, float delta_time) {
            if (!school.Is_Operational) {
                return;
            }
            foreach (Building building in school.Get_Connected_Buildings(school.Road_Range).Select(x => x.Key).ToArray()) {
                if (building is Residence) {
                    (building as Residence).Serve(Residence.ServiceType.Education, 1.0f, school.Efficency);
                }
            }
        }, null, null, new List<Resource>(), new List<Resource>(), 0.0f, 0.0f));
        prototypes.First(x => x.Internal_Name == "school_house").Sprites.Add(new SpriteData("school_house_1"));

        prototypes.Add(new Building("Shore House", "shore_house", Building.UI_Category.Coastal, "shore_house", Building.BuildingSize.s2x2, 90, new Dictionary<Resource, int>() {
            { Resource.Wood, 35 }, { Resource.Lumber, 25 }, { Resource.Stone, 10 }, { Resource.Tools, 10 }
        }, 75, new List<Resource>(), 0, 0.0f, 70, new Dictionary<Resource, float>() { { Resource.Wood, 0.01f } }, 0.50f, 0.0f, 0, new Dictionary<Building.Resident, int>() { { Building.Resident.Peasant, 10 }, { Building.Resident.Citizen, 5 } },
        10, true, false, true, 1.10f, 0, null,
        delegate (Building building, float delta_time) {
            if (!building.Is_Operational) {
                return;
            }
            float tiles = 0.0f;
            foreach (Tile t in Map.Instance.Get_Tiles_Around(building)) {
                if (t.Is_Water) {
                    tiles++;
                }
            }
            building.Produce(Resource.Sand, 20.0f * (Mathf.Clamp(tiles, 0.0f, 2.0f) * 0.5f), delta_time);
        }, null,
        delegate (Building building) {
            List<Tile> worked_tiles = new List<Tile>();
            foreach (Tile t in Map.Instance.Get_Tiles_Around(building)) {
                if (t.Is_Water) {
                    worked_tiles.Add(t);
                }
            }
            return worked_tiles;
        },
        new List<Resource>(), new List<Resource>() { Resource.Sand }, 0.0f, 0.0f));
        prototypes.First(x => x.Internal_Name == "shore_house").On_Build_Check = Waterfront_Check;

        prototypes.Add(new Building("Fishing Harbor", "fishing_harbor", Building.UI_Category.Coastal, "fishing_harbor", Building.BuildingSize.s3x3, 175, new Dictionary<Resource, int>() {
             { Resource.Lumber, 240 }, { Resource.Stone, 95 }, { Resource.Wood, 20 }, { Resource.Tools, 20 }
        }, 250, new List<Resource>(), 0, 0.0f, 355, new Dictionary<Resource, float>() { { Resource.Lumber, 0.05f }, { Resource.Stone, 0.01f } }, 4.50f, 0.0f, 0.0f, new Dictionary<Building.Resident, int>() { { Building.Resident.Peasant, 15 }, { Building.Resident.Citizen, 20 } }, 20,
        true, false, true, 0.0f, 0, On_Harbor_Built,
        delegate (Building building, float delta_time) {
            if (!building.Is_Operational) {
                return;
            }
            if (!building.Has_Functional_Dock()) {
                building.Show_Alert("alert_general");
                return;
            }
            building.Produce(Resource.Fish, 12.0f, delta_time);
            building.Produce(Resource.Lobsters, 4.0f, delta_time);
        }, On_Harbor_Deconstruct, null, new List<Resource>(), new List<Resource>() { Resource.Fish, Resource.Lobsters }, 0.0f, 0.0f));
        prototypes.First(x => x.Internal_Name == "fishing_harbor").On_Build_Check = On_Harbor_Ship_Build_Check;
        prototypes.First(x => x.Internal_Name == "fishing_harbor").On_Building_Start = On_Harbor_Ship_Building_Start;

        prototypes.Add(new Building("Glassworks", "glassworks", Building.UI_Category.Industry, "glassworks", Building.BuildingSize.s2x2, 225, new Dictionary<Resource, int>() {
            { Resource.Lumber, 70 }, { Resource.Bricks, 130 }, { Resource.Stone, 35 }, { Resource.Tools, 35 }
        }, 235, new List<Resource>(), 0, 50.0f, 235, new Dictionary<Resource, float>() { { Resource.Bricks, 0.05f }, { Resource.Lumber, 0.01f } }, 2.00f, 0.0f, 0, new Dictionary<Building.Resident, int>() {
        { Building.Resident.Citizen, 10 } }, 10, true, false, true, 0.0f, 7, null, delegate (Building building, float delta_time) {
            List<Resource> fuel_types = new List<Resource>() { Resource.Firewood, Resource.Charcoal, Resource.Coal };
            Resource selected_fuel = fuel_types[building.Special_Settings.First(x => x.Name == "fuel").Dropdown_Selection];
            int production = building.Special_Settings.First(x => x.Name == "production").Dropdown_Selection;
            Dictionary<Resource, float> inputs = new Dictionary<Resource, float>();
            Dictionary<Resource, float> outputs = new Dictionary<Resource, float>();

            inputs.Add(selected_fuel, selected_fuel == Resource.Firewood ? 5.0f : 2.5f);
            switch (production) {
                case 0:
                    inputs.Add(Resource.Sand, 20.0f);
                    inputs.Add(Resource.Potash, 5.0f);
                    outputs.Add(Resource.Glass, 10.0f);
                    break;
                case 1:
                    inputs.Add(Resource.Glass, 10.0f);
                    outputs.Add(Resource.Glassware, 2.5f);
                    break;
            }
            building.Consumes.Clear();
            building.Produces.Clear();
            foreach (KeyValuePair<Resource, float> pair in inputs) {
                building.Consumes.Add(pair.Key);
            }
            foreach (KeyValuePair<Resource, float> pair in outputs) {
                building.Produces.Add(pair.Key);
            }
            if (!building.Is_Operational) {
                return;
            }
            building.Process(inputs, outputs, delta_time);
        }, null, null, new List<Resource>() { Resource.Charcoal, Resource.Coal, Resource.Firewood, Resource.Sand, Resource.Potash }, new List<Resource>() { Resource.Glass, Resource.Glassware }, -1.25f, 5.0f));
        prototypes.First(x => x.Internal_Name == "glassworks").Special_Settings.Add(new SpecialSetting("fuel", "Fuel", SpecialSetting.SettingType.Dropdown, 0, false, new List<string>() { Resource.Firewood.UI_Name + " (5/day)", Resource.Charcoal.UI_Name + " (2.5/day)", Resource.Coal.UI_Name + " (2.5/day)" }, 0));
        prototypes.First(x => x.Internal_Name == "glassworks").Special_Settings.Add(new SpecialSetting("production", "Production", SpecialSetting.SettingType.Dropdown, 0, false, new List<string>() { Resource.Glass.UI_Name + " (10/day)", Resource.Glassware.UI_Name + " (2.5/day)"}, 0));
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
