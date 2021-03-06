﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ForeignCity {
    private static long current_id;
    public enum TradeRouteType { Land, Water, Both }
    public enum CityType { Farming_Town, Rural_Town, Industrial_City, Forest_Village, Remote_Village, Maritime_Town, Mining_Town, Large_City, Colony }

    public static readonly List<Resource> IMPORTANT_EXPORTS = new List<Resource>() { Resource.Coffee, Resource.Silk };
    public static readonly float EXPORT_BASE_PRICE_MULTIPLIER = 2.0f;
    public static readonly float EXPORT_CHEAP_PRICE_MULTIPLIER = 1.25f;
    public static readonly float EXPORT_EXPENSIVE_PRICE_MULTIPLIER = 5.0f;
    public static readonly float IMPORT_BASE_PRICE_MULTIPLIER = 0.50f;
    public static readonly float IMPORT_PREFERRED_PRICE_MULTIPLIER = 1.00f;
    public static readonly float IMPORT_DISLIKED_PRICE_MULTIPLIER = 0.10f;
    public static readonly float OPINION_MAX_RESTING_POINT = 0.50f;
    public static readonly float OPINION_PASSIVE_DELTA_PER_DAY = 0.00025f;
    public static readonly float OPINION_IMPROVED_TIMER_DAYS = 30.0f;
    public static readonly float COLONY_DISCOUNT = 0.25f;

    public long Id { get; private set; }
    public string Name { get; private set; }
    public float Opinion { get; private set; }
    public float Opinion_Resting_Point { get; private set; }
    public float Opinion_Improved_Timer { get; private set; }
    public List<Resource> Preferred_Imports { get; private set; }
    public List<Resource> Disliked_Imports { get; private set; }
    public List<Resource> Unaccepted_Imports { get; private set; }
    public List<Resource> Exports { get; private set; }
    public List<Resource> Cheap_Exports { get; private set; }
    public List<Resource> Expensive_Exports { get; private set; }
    public TradeRouteType Trade_Route_Type { get; private set; }
    public CityType City_Type { get; private set; }

    private List<Resource.ResourceTag> possible_exports;
    private List<Resource.ResourceTag> impossible_exports;

    public ForeignCity()
    {
        Id = current_id;
        current_id++;
        Name = NameManager.Instance.Get_Name(NameManager.NameType.City, false);
        if(RNG.Instance.Next(0, 100) < 33) {
            //Hostile
            Opinion = -1.0f + RNG.Instance.Next_F();
        } else if(RNG.Instance.Next(0, 100) < 10) {
            //Positive
            Opinion = 0.25f + (RNG.Instance.Next_F() * (OPINION_MAX_RESTING_POINT - 0.25f));
        } else {
            //Neutral
            Opinion = RNG.Instance.Next_F() * 0.25f;
        }
        Opinion_Resting_Point = -1.0f;
        Update_Opinion_Resting_Point();
        City_Type = RNG.Instance.Item(Enum.GetValues(typeof(CityType)).Cast<CityType>().Where(x => x != CityType.Colony).ToList());
        Dictionary<TradeRouteType, int> trade_route_chances = new Dictionary<TradeRouteType, int>() { { TradeRouteType.Land, 40 }, { TradeRouteType.Water, 40 }, { TradeRouteType.Both, 20 } };

        Preferred_Imports = new List<Resource>();
        Disliked_Imports = new List<Resource>();
        Unaccepted_Imports = new List<Resource>();
        Exports = new List<Resource>();
        Cheap_Exports = new List<Resource>();
        Expensive_Exports = new List<Resource>();
        possible_exports = new List<Resource.ResourceTag>();
        impossible_exports = new List<Resource.ResourceTag>();

        switch (City_Type) {
            case CityType.Farming_Town:
                Initialize_Exports_And_Imports(
                    new List<Resource.ResourceTag>() { Resource.ResourceTag.Agricultural, Resource.ResourceTag.Livestock, Resource.ResourceTag.Crop },
                    new List<Resource.ResourceTag>() { Resource.ResourceTag.Clothing },
                    new List<Resource.ResourceTag>() { Resource.ResourceTag.Opulent, Resource.ResourceTag.Exotic },
                    new List<Resource.ResourceTag>() { Resource.ResourceTag.Hunting, Resource.ResourceTag.Coastal },
                    new List<Resource.ResourceTag>() { Resource.ResourceTag.Archaic, Resource.ResourceTag.Opulent, Resource.ResourceTag.Foraging, Resource.ResourceTag.Food },
                    new List<Resource.ResourceTag>() { Resource.ResourceTag.Opulent });
                trade_route_chances[TradeRouteType.Land] = 70;
                trade_route_chances[TradeRouteType.Water] = 15;
                trade_route_chances[TradeRouteType.Both] = 15;
                break;
            case CityType.Rural_Town:
                Initialize_Exports_And_Imports(
                    new List<Resource.ResourceTag>() { Resource.ResourceTag.Agricultural, Resource.ResourceTag.Forestry, Resource.ResourceTag.Crop },
                    new List<Resource.ResourceTag>() { Resource.ResourceTag.Foraging, Resource.ResourceTag.Clothing },
                    new List<Resource.ResourceTag>() { Resource.ResourceTag.Opulent, Resource.ResourceTag.Fine, Resource.ResourceTag.Exotic },
                    new List<Resource.ResourceTag>() { Resource.ResourceTag.Construction, Resource.ResourceTag.Coastal },
                    new List<Resource.ResourceTag>() { Resource.ResourceTag.Agricultural, Resource.ResourceTag.Livestock, Resource.ResourceTag.Archaic, Resource.ResourceTag.Opulent, Resource.ResourceTag.Foraging },
                    new List<Resource.ResourceTag>() { Resource.ResourceTag.Opulent });
                trade_route_chances[TradeRouteType.Land] = 80;
                trade_route_chances[TradeRouteType.Water] = 10;
                trade_route_chances[TradeRouteType.Both] = 10;
                break;
            case CityType.Remote_Village:
                Initialize_Exports_And_Imports(
                    new List<Resource.ResourceTag>() { Resource.ResourceTag.Archaic },
                    new List<Resource.ResourceTag>() { Resource.ResourceTag.Foraging, Resource.ResourceTag.Hunting, Resource.ResourceTag.Crop },
                    new List<Resource.ResourceTag>() { Resource.ResourceTag.Opulent, Resource.ResourceTag.Fine, Resource.ResourceTag.Exotic },
                    new List<Resource.ResourceTag>() { Resource.ResourceTag.Construction, Resource.ResourceTag.Clothing, Resource.ResourceTag.Food },
                    new List<Resource.ResourceTag>() { Resource.ResourceTag.Archaic, Resource.ResourceTag.Foraging, Resource.ResourceTag.Hunting, Resource.ResourceTag.Exotic },
                    new List<Resource.ResourceTag>() { Resource.ResourceTag.Opulent });
                trade_route_chances[TradeRouteType.Land] = 90;
                trade_route_chances[TradeRouteType.Water] = 5;
                trade_route_chances[TradeRouteType.Both] = 5;
                break;
            case CityType.Maritime_Town:
                Initialize_Exports_And_Imports(
                    new List<Resource.ResourceTag>() { Resource.ResourceTag.Coastal },
                    new List<Resource.ResourceTag>() { Resource.ResourceTag.Industrial, Resource.ResourceTag.Exotic, Resource.ResourceTag.Jewelry },
                    new List<Resource.ResourceTag>() { Resource.ResourceTag.Mining },
                    new List<Resource.ResourceTag>() { Resource.ResourceTag.Forestry, Resource.ResourceTag.Foraging },
                    new List<Resource.ResourceTag>() { Resource.ResourceTag.Coastal },
                    new List<Resource.ResourceTag>() { });
                trade_route_chances[TradeRouteType.Land] = 0;
                trade_route_chances[TradeRouteType.Water] = 90;
                trade_route_chances[TradeRouteType.Both] = 10;
                break;
            case CityType.Forest_Village:
                Initialize_Exports_And_Imports(
                    new List<Resource.ResourceTag>() { Resource.ResourceTag.Forestry },
                    new List<Resource.ResourceTag>() { Resource.ResourceTag.Foraging, Resource.ResourceTag.Hunting },
                    new List<Resource.ResourceTag>() { Resource.ResourceTag.Opulent, Resource.ResourceTag.Exotic },
                    new List<Resource.ResourceTag>() { },
                    new List<Resource.ResourceTag>() { Resource.ResourceTag.Forestry, Resource.ResourceTag.Foraging, Resource.ResourceTag.Hunting, Resource.ResourceTag.Opulent },
                    new List<Resource.ResourceTag>() { Resource.ResourceTag.Opulent });
                trade_route_chances[TradeRouteType.Land] = 90;
                trade_route_chances[TradeRouteType.Water] = 5;
                trade_route_chances[TradeRouteType.Both] = 5;
                break;
            case CityType.Industrial_City:
                Initialize_Exports_And_Imports(
                    new List<Resource.ResourceTag>() { Resource.ResourceTag.Industrial },
                    new List<Resource.ResourceTag>() { Resource.ResourceTag.Construction },
                    new List<Resource.ResourceTag>() { Resource.ResourceTag.Exotic },
                    new List<Resource.ResourceTag>() { Resource.ResourceTag.Mining, Resource.ResourceTag.Industrial, Resource.ResourceTag.Food },
                    new List<Resource.ResourceTag>() { },
                    new List<Resource.ResourceTag>() { });
                trade_route_chances[TradeRouteType.Land] = 45;
                trade_route_chances[TradeRouteType.Water] = 30;
                trade_route_chances[TradeRouteType.Both] = 25;
                break;
            case CityType.Mining_Town:
                Initialize_Exports_And_Imports(
                    new List<Resource.ResourceTag>() { Resource.ResourceTag.Mining },
                    new List<Resource.ResourceTag>() { Resource.ResourceTag.Industrial, Resource.ResourceTag.Construction },
                    new List<Resource.ResourceTag>() { Resource.ResourceTag.Fine, Resource.ResourceTag.Exotic, Resource.ResourceTag.Opulent },
                    new List<Resource.ResourceTag>() { Resource.ResourceTag.Food, Resource.ResourceTag.Forestry, Resource.ResourceTag.Construction },
                    new List<Resource.ResourceTag>() { Resource.ResourceTag.Mining, Resource.ResourceTag.Opulent},
                    new List<Resource.ResourceTag>() { });
                trade_route_chances[TradeRouteType.Land] = 90;
                trade_route_chances[TradeRouteType.Water] = 5;
                trade_route_chances[TradeRouteType.Both] = 5;
                break;
            case CityType.Large_City:
                Initialize_Exports_And_Imports(
                    new List<Resource.ResourceTag>() { Resource.ResourceTag.Opulent, Resource.ResourceTag.Industrial, Resource.ResourceTag.Agricultural, Resource.ResourceTag.Clothing },
                    new List<Resource.ResourceTag>() { Resource.ResourceTag.Coastal, Resource.ResourceTag.Fine },
                    new List<Resource.ResourceTag>() { Resource.ResourceTag.Archaic, Resource.ResourceTag.Exotic, Resource.ResourceTag.Crop, Resource.ResourceTag.Livestock },
                    new List<Resource.ResourceTag>() { Resource.ResourceTag.Food, Resource.ResourceTag.Industrial, Resource.ResourceTag.Opulent, Resource.ResourceTag.Exotic },
                    new List<Resource.ResourceTag>() { Resource.ResourceTag.Archaic, Resource.ResourceTag.Foraging },
                    new List<Resource.ResourceTag>() { });
                trade_route_chances[TradeRouteType.Land] = 45;
                trade_route_chances[TradeRouteType.Water] = 45;
                trade_route_chances[TradeRouteType.Both] = 10;
                break;
            default:
                CustomLogger.Instance.Error(string.Format("Unimplemented city type: {0}", City_Type.ToString()));
                break;
        }
        int check_total = 0;
        int random = RNG.Instance.Next(0, 100);
        foreach(TradeRouteType type in Enum.GetValues(typeof(TradeRouteType))) {
            check_total += trade_route_chances[type];
        }
        if(random <= trade_route_chances[TradeRouteType.Land]) {
            Trade_Route_Type = TradeRouteType.Land;
        } else if(random > trade_route_chances[TradeRouteType.Land] && random <= trade_route_chances[TradeRouteType.Land] + trade_route_chances[TradeRouteType.Water]) {
            Trade_Route_Type = TradeRouteType.Water;
        } else {
            Trade_Route_Type = TradeRouteType.Both;
        }

        if(check_total != 100) {
            CustomLogger.Instance.Error(string.Format("Chance total is not 100 for type: {0}", City_Type.ToString()));
        }
    }

    public ForeignCity(ColonyLocation colony_data)
    {
        Id = current_id;
        current_id++;
        Name = colony_data.Name;
        Opinion = 1.0f;
        Opinion_Resting_Point = -1.0f;
        Update_Opinion_Resting_Point();
        City_Type = CityType.Colony;
        Trade_Route_Type = colony_data.Trade_Route_Type;

        Preferred_Imports = colony_data.Preferred_Imports;
        Disliked_Imports = colony_data.Disliked_Imports;
        Unaccepted_Imports = colony_data.Unaccepted_Imports;
        Exports = colony_data.Exports;
        Cheap_Exports = colony_data.Cheap_Exports;
        Expensive_Exports = colony_data.Expensive_Exports;

    }

    public ForeignCity(ForeignCitySaveData data)
    {
        Id = data.Id;
        if(Id >= current_id) {
            current_id = Id + 1;
        }
        Name = data.Name;
        Opinion = data.Opinion;
        Opinion_Resting_Point = data.Opinion_Resting_Point;
        City_Type = (CityType)data.City_Type;
        Trade_Route_Type = (TradeRouteType)data.Trade_Route_Type;
        Preferred_Imports = Make_Resource_List(data.Preferred_Imports);
        Disliked_Imports = Make_Resource_List(data.Disliked_Imports);
        Unaccepted_Imports = Make_Resource_List(data.Unaccepted_Imports);
        Exports = Make_Resource_List(data.Exports);
        Cheap_Exports = Make_Resource_List(data.Cheap_Exports);
        Expensive_Exports = Make_Resource_List(data.Expensive_Exports);
    }

    public float? Discount
    {
        get {
            float? base_discount = Opinion < 0.0f ? (float?)null : (Opinion > 0.5f ? (Opinion - 0.5f) / 2.0f : 0.0f);
            if(City_Type != CityType.Colony) {
                return base_discount;
            }
            float discount = base_discount.HasValue ? base_discount.Value * 0.4f : 0.0f;
            return discount + (COLONY_DISCOUNT * City.Instance.Colony_Effectiveness);
        }
    }

    public void Update(float delta_time)
    {
        float delta_days = TimeManager.Instance.Seconds_To_Days(delta_time);
        if (Opinion_Improved_Timer > 0.0f) {
            Opinion_Improved_Timer -= delta_days;
            return;
        }
        if(Opinion > Opinion_Resting_Point) {
            Opinion = Mathf.Clamp(Opinion - (OPINION_PASSIVE_DELTA_PER_DAY * delta_days), Opinion_Resting_Point, 1.0f);
        } else if(Opinion < Opinion_Resting_Point) {
            Opinion = Mathf.Clamp(Opinion + (OPINION_PASSIVE_DELTA_PER_DAY * delta_days), -1.0f, Opinion_Resting_Point);
        }
    }

    public float Get_Export_Price(Resource resource)
    {
        float multiplier = 1.0f;
        if (Exports.Contains(resource)) {
            multiplier = EXPORT_BASE_PRICE_MULTIPLIER;
        } else if (Expensive_Exports.Contains(resource)) {
            multiplier = EXPORT_EXPENSIVE_PRICE_MULTIPLIER;
        } else if (Cheap_Exports.Contains(resource)) {
            multiplier = EXPORT_CHEAP_PRICE_MULTIPLIER;
        } else {
            return 0.0f;
        }
        return Discount.HasValue ? resource.Value * (multiplier - Discount.Value) : resource.Value * multiplier;
    }

    public float Get_Import_Price(Resource resource)
    {
        float multiplier = IMPORT_BASE_PRICE_MULTIPLIER;
        if (Unaccepted_Imports.Contains(resource)) {
            return 0.0f;
        }
        if (Preferred_Imports.Contains(resource)) {
            multiplier = IMPORT_PREFERRED_PRICE_MULTIPLIER;
        } else if (Disliked_Imports.Contains(resource)) {
            multiplier = IMPORT_DISLIKED_PRICE_MULTIPLIER;
        }
        return resource.Value * multiplier;
    }

    public bool Insert_Export(Resource resource)
    {
        if(Cheap_Exports.Contains(resource) || Exports.Contains(resource) || Expensive_Exports.Contains(resource)) {
            return false;
        }
        bool possible = false;
        foreach(Resource.ResourceTag tag in resource.Tags) {
            if (impossible_exports.Contains(tag)) {
                return false;
            }
            if (possible_exports.Contains(tag)) {
                possible = true;
            }
        }
        if (!possible) {
            return false;
        }
        int random = RNG.Instance.Next(0, 100);
        if(random < 35) {
            Cheap_Exports.Add(resource);
        } else if(random >= 35 && random < 65) {
            Exports.Add(resource);
        } else {
            Expensive_Exports.Add(resource);
        }
        return true;
    }

    public void Improve_Opinion(float amount)
    {
        if(amount <= 0.0f) {
            CustomLogger.Instance.Warning(string.Format("amount = {0}", amount));
            return;
        }
        Opinion = Mathf.Clamp(Opinion + amount, -1.0f, 1.0f);
        Opinion_Improved_Timer = OPINION_IMPROVED_TIMER_DAYS;
        Update_Opinion_Resting_Point();
    }

    public override string ToString()
    {
        return string.Format("{0} #{1}", Name, Id);
    }

    public static void Reset_Current_Id()
    {
        current_id = 0;
    }

    private void Update_Opinion_Resting_Point()
    {
        if(Opinion > Opinion_Resting_Point) {
            Opinion_Resting_Point = Opinion;
            if(Opinion_Resting_Point > OPINION_MAX_RESTING_POINT) {
                Opinion_Resting_Point = OPINION_MAX_RESTING_POINT;
            }
        }
    }

    private List<Resource> Make_Resource_List(List<int> types)
    {
        List<Resource> list = new List<Resource>();
        foreach(int type in types) {
            Resource resource = Resource.All.FirstOrDefault(x => (int)x.Type == type);
            if(resource == null) {
                CustomLogger.Instance.Error(string.Format("Invalid resource type: {0}", type));
            } else {
                list.Add(resource);
            }
        }
        return list;
    }

    private void Initialize_Exports_And_Imports(List<Resource.ResourceTag> main_exports, List<Resource.ResourceTag> rare_exports, List<Resource.ResourceTag> never_export, List<Resource.ResourceTag> main_imports,
        List<Resource.ResourceTag> disliked_imports, List<Resource.ResourceTag> unacceped_imports)
    {
        foreach(Resource.ResourceTag tag in main_exports) {
            possible_exports.Add(tag);
        }
        foreach (Resource.ResourceTag tag in rare_exports) {
            possible_exports.Add(tag);
        }
        foreach (Resource.ResourceTag tag in never_export) {
            impossible_exports.Add(tag);
        }

        Initialize_Exports(main_exports, rare_exports, never_export);
        if(Exports.Count + Cheap_Exports.Count + Expensive_Exports.Count <= 1) {
            Initialize_Exports(main_exports, rare_exports, never_export);
        }
        foreach (Resource resource in Resource.All) {
            if(resource.Rarity == Resource.ResourceRarity.Non_Tradeable) {
                continue;
            }
            bool is_main_import = false;
            bool is_disliked_import = false;
            bool is_unacceped_import = false;
            foreach (Resource.ResourceTag tag in resource.Tags) {
                if (unacceped_imports.Contains(tag)) {
                    is_unacceped_import = true;
                }
                if (disliked_imports.Contains(tag)) {
                    is_disliked_import = true;
                }
                if (main_imports.Contains(tag)) {
                    is_main_import = true;
                }
            }
            if (is_disliked_import) {
                is_main_import = false;
            }
            if (is_unacceped_import) {
                is_main_import = false;
                is_disliked_import = false;
            }
            if (is_unacceped_import) {
                //TODO: Allways unaccept?
                if(RNG.Instance.Next(0, 100) <= 97) {
                    Unaccepted_Imports.Add(resource);
                } else {
                    Disliked_Imports.Add(resource);
                }
            } else if (is_main_import) {
                if (RNG.Instance.Next(0, 100) <= 50) {
                    Preferred_Imports.Add(resource);
                }
            } else if (is_disliked_import) {
                if (RNG.Instance.Next(0, 100) <= 65) {
                    Disliked_Imports.Add(resource);
                }
            }
        }

        foreach(Resource export in Exports) {
            if (Preferred_Imports.Contains(export)) {
                Preferred_Imports.Remove(export);
            }
        }
        foreach (Resource export in Expensive_Exports) {
            if (Preferred_Imports.Contains(export)) {
                Preferred_Imports.Remove(export);
            }
        }
        foreach (Resource export in Cheap_Exports) {
            if (Preferred_Imports.Contains(export)) {
                Preferred_Imports.Remove(export);
            }
        }
    }

    private void Initialize_Exports(List<Resource.ResourceTag> main_exports, List<Resource.ResourceTag> rare_exports, List<Resource.ResourceTag> never_export)
    {
        Dictionary<Resource.ResourceRarity, int> base_export_chance = new Dictionary<Resource.ResourceRarity, int>() {
            { Resource.ResourceRarity.Common, 50 },
            { Resource.ResourceRarity.Uncommon, 35 },
            { Resource.ResourceRarity.Rare, 10 },
            { Resource.ResourceRarity.Very_Rare, 5 }
        };
        foreach (Resource resource in Resource.All) {
            if(Exports.Contains(resource) || Cheap_Exports.Contains(resource) || Expensive_Exports.Contains(resource) || resource.Rarity == Resource.ResourceRarity.Non_Tradeable) {
                continue;
            }
            int export_chance = base_export_chance[resource.Rarity];
            bool is_main_export = false;
            bool is_rare_export = false;
            bool is_never_export = false;
            foreach (Resource.ResourceTag tag in resource.Tags) {
                if (never_export.Contains(tag)) {
                    is_never_export = true;
                }
                if (main_exports.Contains(tag)) {
                    is_main_export = true;
                }
                if (rare_exports.Contains(tag)) {
                    is_rare_export = true;
                }
            }
            if (is_never_export) {
                is_main_export = false;
                is_rare_export = false;
            }
            if (is_main_export) {
                is_rare_export = false;
            }
            if (is_main_export || is_rare_export) {
                if (is_rare_export) {
                    export_chance /= 5;
                }
                if (RNG.Instance.Next(0, 100) <= export_chance) {
                    if (is_rare_export) {
                        if (RNG.Instance.Next(0, 100) <= 75) {
                            Expensive_Exports.Add(resource);
                        } else {
                            Exports.Add(resource);
                        }
                    } else {
                        int random = RNG.Instance.Next(0, 100);
                        if (random <= 25) {
                            Expensive_Exports.Add(resource);
                        } else if (random >= 75) {
                            Cheap_Exports.Add(resource);
                        } else {
                            Exports.Add(resource);
                        }
                    }
                }
            }
        }
    }
}
