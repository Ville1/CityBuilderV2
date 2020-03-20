using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class BuildingPrototypes {
    private static BuildingPrototypes instance;

    private List<Building> prototypes;

    private BuildingPrototypes()
    {
        prototypes = new List<Building>();

        prototypes.Add(new Building("Townhall", Building.TOWN_HALL_INTERNAL_NAME, Building.UI_Category.Admin, "town_hall", Building.BuildingSize.s2x2, 1000, new Dictionary<Resource, int>(), 0, new List<Resource>() { Resource.Lumber, Resource.Stone, Resource.Tools, Resource.Wood },
            2000, 15.0f, 0, new Dictionary<Resource, float>(), 0.0f, 3.0f, 50.0f, new Dictionary<Building.Resident, int>(), 0, false, false, false, 0.0f, 25, null, null, null, null, new List<Resource>(), new List<Resource>()));

        prototypes.Add(new Residence("Cabin", "hut", Building.UI_Category.Housing, "hut", Building.BuildingSize.s2x2, 100, new Dictionary<Resource, int>() {
            { Resource.Wood, 100 }, { Resource.Stone, 15 }, { Resource.Tools, 10 }
        }, 100, new List<Resource>(), 0, 115, new Dictionary<Resource, float>() { { Resource.Wood, 0.05f } }, 0.0f, 0.0f, 0.0f, new Dictionary<Building.Resident, int>() { { Building.Resident.Peasant, 10 } }, null, null, null, null, new List<Resource>(), new List<Resource>()));

        prototypes.Add(new Building("Wood Cutters Lodge", "wood_cutters_lodge", Building.UI_Category.Forestry, "wood_cutters_lodge", Building.BuildingSize.s2x2, 100, new Dictionary<Resource, int>() {
            { Resource.Wood, 75 }, { Resource.Stone, 5 }, { Resource.Tools, 15 }
        }, 90, new List<Resource>(), 0, 0.0f, 85, new Dictionary<Resource, float>(), 0.75f, 0.0f, 0, new Dictionary<Building.Resident, int>() { { Building.Resident.Peasant, 10 } }, 10, true, false, true, 4.0f, 0, delegate(Building building) {
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
            wood /= 4.0f;
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
        }, 225, new List<Resource>() { Resource.Lumber, Resource.Stone, Resource.Tools, Resource.Wood, Resource.Hide, Resource.Leather },
        2000, 65.0f, 250, new Dictionary<Resource, float>() { { Resource.Lumber, 0.05f } }, 1.0f, 0.0f, 0.0f, new Dictionary<Building.Resident, int>() { { Building.Resident.Peasant, 10 } }, 10, false, false, true, 0.0f, 15, null, null, null, null, new List<Resource>(), new List<Resource>()));

        prototypes.Add(new Building("Cellar", "cellar", Building.UI_Category.Infrastructure, "cellar", Building.BuildingSize.s1x1, 100, new Dictionary<Resource, int>() {
            { Resource.Wood, 15 }, { Resource.Stone, 50 }, { Resource.Tools, 10 }, { Resource.Lumber, 50 }
        }, 100, new List<Resource>() { Resource.Roots, Resource.Berries, Resource.Mushrooms, Resource.Herbs, Resource.Game, Resource.Potatoes, Resource.Bread },
        1000, 50.0f, 110, new Dictionary<Resource, float>() { { Resource.Wood, 0.05f } }, 0.5f, 0.0f, 0.0f, new Dictionary<Building.Resident, int>() { { Building.Resident.Peasant, 5 } }, 5, false, false, true, 0.0f, 12, null, null, null, null, new List<Resource>(), new List<Resource>()));

        prototypes.Add(new Building("Wood Stockpile", "wood_stockpile", Building.UI_Category.Infrastructure, "wood_stockpile", Building.BuildingSize.s2x2, 100, new Dictionary<Resource, int>() {
            { Resource.Wood, 25 }, { Resource.Stone, 5 }, { Resource.Tools, 5 }
        }, 100, new List<Resource>() { Resource.Wood, Resource.Lumber, Resource.Firewood },
        1000, 45.0f, 50, new Dictionary<Resource, float>() { { Resource.Wood, 0.01f } }, 0.25f, 0.0f, 0.0f, new Dictionary<Building.Resident, int>() { { Building.Resident.Peasant, 5 } }, 5, false, false, true, 0.0f, 12, null, null, null, null, new List<Resource>(), new List<Resource>()));

        prototypes.Add(new Building("Gatherers Lodge", "gatherers_lodge", Building.UI_Category.Forestry, "gatherers_lodge", Building.BuildingSize.s2x2, 100, new Dictionary<Resource, int>() {
            { Resource.Wood, 85 }, { Resource.Stone, 10 }, { Resource.Tools, 10 }
        }, 100, new List<Resource>(), 0, 0.0f, 95, new Dictionary<Resource, float>() { { Resource.Wood, 0.05f } }, 0.75f, 0.0f, 0, new Dictionary<Building.Resident, int>() { { Building.Resident.Peasant, 5 } }, 5, true, false, true, 5.0f, 0, delegate (Building building) {
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
            float multiplier = 0.5f;
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
            foreach(Building building in market.Get_Connected_Buildings(market.Road_Range).Select(x => x.Key).ToArray()) {
                if(!(building is Residence)) {
                    continue;
                }
                Residence residence = building as Residence;
                food_needed += residence.Service_Needed(Residence.ServiceType.Food) * resources_for_full_service;
                fuel_needed += residence.Service_Needed(Residence.ServiceType.Fuel) * resources_for_full_service;
                herbs_needed += residence.Service_Needed(Residence.ServiceType.Herbs) * resources_for_full_service;
                residences.Add(residence);
            }
            if(residences.Count == 0 || (food_needed == 0.0f && fuel_needed == 0.0f && herbs_needed == 0.0f)) {
                return;
            }

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

            if (income != 0.0f) {
                market.Per_Day_Cash_Delta += (income / delta_time) * TimeManager.Instance.Days_To_Seconds(1.0f, 1.0f);
                City.Instance.Add_Cash(income);
            }//                                  v unnecessary list v special settings adds and removes stuff from consumption list
        }, null, null, new List<Resource>() { Resource.Berries, Resource.Roots, Resource.Mushrooms, Resource.Herbs, Resource.Firewood, Resource.Charcoal, Resource.Game, Resource.Bread, Resource.Potatoes }, new List<Resource>()));
        Resource prefered_fuel = Resource.All.Where(x => x.Is_Fuel).OrderByDescending(x => x.Value / x.Fuel_Value).FirstOrDefault();
        foreach(Resource resource in Resource.All) {
            if (resource.Is_Food) {
                prototypes.First(x => x.Internal_Name == "marketplace").Special_Settings.Add(new SpecialSetting(resource.ToString().ToLower(), resource.UI_Name, SpecialSetting.SettingType.Toggle, 0.0f, true));
            } else if (resource.Is_Fuel) {
                prototypes.First(x => x.Internal_Name == "marketplace").Special_Settings.Add(new SpecialSetting(resource.ToString().ToLower(), resource.UI_Name, SpecialSetting.SettingType.Toggle, 0.0f, resource == prefered_fuel));
            }
        }

        prototypes.Add(new Building("Hunting Lodge", "hunting_lodge", Building.UI_Category.Forestry, "hunting_lodge", Building.BuildingSize.s2x2, 100, new Dictionary<Resource, int>() {
            { Resource.Wood, 85 }, { Resource.Stone, 10 }, { Resource.Tools, 10 }
        }, 110, new List<Resource>(), 0, 0.0f, 95, new Dictionary<Resource, float>() { { Resource.Wood, 0.05f } }, 0.75f, 0.0f, 0, new Dictionary<Building.Resident, int>() { { Building.Resident.Peasant, 5 } }, 5, true, false, true, 9.0f, 0, delegate (Building building) {
            foreach (Tile tile in building.Get_Tiles_In_Circle(building.Range)) {
                tile.Worked_By.Add(building);
            }
        }, delegate (Building building, float delta_time) {
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
                } else if (tile.Building == null && tile.Worked_By.FirstOrDefault(x => x.Internal_Name == building.Internal_Name) == building) {
                    if (tile.Internal_Name == "forest") {
                        game += 1.00f;
                    } else if (tile.Internal_Name == "sparse_forest") {
                        game += 0.75f;
                    } else if (tile.Internal_Name == "grass") {
                        game += 0.10f;
                    } else if (tile.Internal_Name == "fertile_ground") {
                        game += 0.125f;
                    }
                }
            }
            float multiplier = 0.5f;
            game = Mathf.Max((game * 0.1f) * multiplier, 0.0f);
            float hide = game * 0.25f;
            building.Produce(Resource.Game, game, delta_time);
            building.Produce(Resource.Hide, hide, delta_time);
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
        }, new List<Resource>(), new List<Resource>() { Resource.Game, Resource.Hide }));

        prototypes.Add(new Building("Quarry", "quarry", Building.UI_Category.Industry, "quarry", Building.BuildingSize.s3x3, 350, new Dictionary<Resource, int>() {
            { Resource.Wood, 65 }, { Resource.Lumber, 80 }, { Resource.Tools, 45 }
        }, 175, new List<Resource>(), 0, 0.0f, 225, new Dictionary<Resource, float>() { { Resource.Wood, 0.10f } }, 1.75f, 0.0f, 0, new Dictionary<Building.Resident, int>() { { Building.Resident.Peasant, 20 } }, 20, true, false, true, 0.0f, 0, null, delegate (Building building, float delta_time) {
            if (!building.Is_Operational) {
                return;
            }
            building.Produce(Resource.Stone, 10.0f, delta_time);
        }, null, null, new List<Resource>(), new List<Resource>() { Resource.Stone }));
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
