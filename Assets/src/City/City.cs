using System;
using System.Collections.Generic;
using UnityEngine;

public enum Resource { Wood, Stone, Lumber, Tools }

public class City {
    public static readonly float GRACE_TIME = 600;
    public static readonly bool PAUSED_BUILDINGS_KEEP_WORKERS = false;

    private static City instance;

    public string Name { get; private set; }
    public bool Has_Town_Hall { get; private set; }
    public List<Building> Buildings { get; private set; }
    public float Cash { get; private set; }
    public Dictionary<Resource, float> Resource_Totals { get; private set; }
    public bool Grace_Time { get { return grace_time_remaining > 0.0f; } }

    private float grace_time_remaining;
    private List<Building> removed_buildings;

    private City()
    {
        Resource_Totals = new Dictionary<Resource, float>();
        foreach (Resource resource in Enum.GetValues(typeof(Resource))) {
            Resource_Totals.Add(resource, 0.0f);
        }
    }

    public void Start_New()
    {
        Has_Town_Hall = false;
        Buildings = new List<Building>();
        Name = "PLACEHOLDER";
        grace_time_remaining = GRACE_TIME;
        removed_buildings = new List<Building>();
    }
    
    public static City Instance
    {
        get {
            if(instance == null) {
                instance = new City();
            }
            return instance;
        }
    }

    public void Update(float delta_time)
    {
        if(grace_time_remaining > 0.0f) {
            grace_time_remaining = Mathf.Clamp(grace_time_remaining - (delta_time * TimeManager.Instance.Multiplier), 0.0f, GRACE_TIME);
        }
        //TODO: Add stopwatch?
        foreach(Building building in Buildings) {
            if (building is Residence) {
                (building as Residence).Update(delta_time);
            } else {
                building.Update(delta_time);
            }
        }
        foreach(Building building in removed_buildings) {
            Buildings.Remove(building);
        }
        removed_buildings.Clear();
        
        foreach(Resource resource in Enum.GetValues(typeof(Resource))) {
            Resource_Totals[resource] = 0.0f;
        }
        Dictionary<Building.Resident, int> current_population = new Dictionary<Building.Resident, int>();
        Dictionary<Building.Resident, int> max_population = new Dictionary<Building.Resident, int>();
        Dictionary<Building.Resident, float> happiness = new Dictionary<Building.Resident, float>();
        foreach (Building.Resident resident in Enum.GetValues(typeof(Building.Resident))) {
            current_population.Add(resident, 0);
            max_population.Add(resident, 0);
            happiness.Add(resident, 0.0f);
        }
        foreach (Building building in Buildings) {
            foreach(KeyValuePair<Resource, float> pair in building.Storage) {
                Resource_Totals[pair.Key] = Resource_Totals[pair.Key] + pair.Value;
            }
            if (building is Residence && building.Is_Built) {
                Residence residence = building as Residence;
                foreach (Building.Resident resident in Enum.GetValues(typeof(Building.Resident))) {
                    current_population[resident] += residence.Current_Residents[resident];
                    max_population[resident] += residence.Resident_Space[resident];
                    happiness[resident] += residence.Happiness[resident] * residence.Current_Residents[resident];
                }
            }
        }
        int peasant_current = current_population[Building.Resident.Peasant];
        int peasant_max = max_population[Building.Resident.Peasant];
        float peasant_happiness = peasant_current > 0 ? happiness[Building.Resident.Peasant] / peasant_current : 0.0f;
        float peasant_employment_relative = 0.0f;
        int peasant_employment = 0;

        int citizen_current = current_population[Building.Resident.Citizen];
        int citizen_max = max_population[Building.Resident.Citizen];
        float citizen_happiness = citizen_current > 0 ? happiness[Building.Resident.Citizen] / citizen_current : 0.0f;
        float citizen_employment_relative = 0.0f;
        int citizen_employment = 0;

        int noble_current = current_population[Building.Resident.Noble];
        int noble_max = max_population[Building.Resident.Noble];
        float noble_happiness = noble_current > 0 ? happiness[Building.Resident.Noble] / noble_current : 0.0f;
        float noble_employment_relative = 0.0f;
        int noble_employment = 0;
        TopGUIManager.Instance.Update_City_Info(Name, Cash, 0.0f, Mathf.RoundToInt(Resource_Totals[Resource.Wood]), Mathf.RoundToInt(Resource_Totals[Resource.Lumber]), Mathf.RoundToInt(Resource_Totals[Resource.Stone]),
            Mathf.RoundToInt(Resource_Totals[Resource.Tools]), peasant_current, peasant_max, peasant_happiness, peasant_employment_relative, peasant_employment, citizen_current, citizen_max, citizen_happiness,
            citizen_employment_relative, citizen_employment, noble_current, noble_max, noble_happiness, noble_employment_relative, noble_employment);
    }

    public void Add_Cash(float amount)
    {
        if(amount <= 0.0f) {
            CustomLogger.Instance.Warning(string.Format("amount = {0}", amount));
            return;
        }
        Cash += amount;
    }

    public bool Can_Build(Building prototype, out string message)
    {
        message = string.Empty;
        if (!Has_Town_Hall && !prototype.Is_Town_Hall) {
            message = "Start by placing the town hall";
            return false;
        }
        if (Has_Town_Hall && prototype.Is_Town_Hall) {
            message = "Only one town hall may be placed";
            return false;
        }
        return true;
    }

    public bool Can_Build(Building prototype, Tile tile, out string message)
    {
        message = string.Empty;
        if(!Can_Build(prototype, out message)) {
            return false;
        }
        if (!tile.Buildable) {
            message = "Invalid terrain";
            return false;
        }
        foreach(Tile t in Map.Instance.Get_Tiles(tile.Coordinates, prototype.Width, prototype.Height)) {
            if (!t.Buildable) {
                message = "Invalid terrain";
                return false;
            }
            if (t.Building != null) {
                message = "Obstructed";
                return false;
            }
        }
        if (tile.Building != null) {
            message = "Obstructed";
            return false;
        }
        if(prototype.Cash_Cost > Cash) {
            message = string.Format("Not enough cash");
            return false;
        }
        foreach(KeyValuePair<Resource, int> pair in prototype.Cost) {
            if(Resource_Totals[pair.Key] < pair.Value) {
                message = string.Format("Not enough {0}", pair.Key.ToString().ToLower());
                return false;
            }
        }
        return true;
    }

    public void Build(Building prototype, Tile tile)
    {
        string message;
        if(!Can_Build(prototype, out message)) {
            return;
        }
        List<Tile> tiles = Map.Instance.Get_Tiles(tile.Coordinates, prototype.Width, prototype.Height);
        if (prototype.Is_Town_Hall) {
            Has_Town_Hall = true;
            Building town_hall = new Building(prototype, tile, tiles, false);
            Cash = 5000.0f;
            town_hall.Store_Resources(Resource.Wood, 750.0f);
            town_hall.Store_Resources(Resource.Stone, 750.0f);
            town_hall.Store_Resources(Resource.Tools, 500.0f);

            Buildings.Add(town_hall);
            TopGUIManager.Instance.Active = true;
            return;
        }
        Cash -= prototype.Cash_Cost;
        foreach(KeyValuePair<Resource, int> pair in prototype.Cost) {
            if(!Take_From_Storage(pair.Key, pair.Value)) {
                CustomLogger.Instance.Error(string.Format("Failed to take {0} {1}", pair.Value, pair.Key));
            }
        }
        if(BuildingPrototypes.Instance.Is_Residence(prototype.Internal_Name)) {
            Buildings.Add(new Residence(BuildingPrototypes.Instance.Get_Residence(prototype.Internal_Name) as Residence, tile, tiles, false));
        } else {
            Buildings.Add(new Building(prototype, tile, tiles, false));
        }
    }

    public void Remove_Building(Building building)
    {
        if (!removed_buildings.Contains(building)) {
            removed_buildings.Add(building);
        }
    }

    public void Delete()
    {
        if (Buildings != null) {
            foreach (Building building in Buildings) {
                GameObject.Destroy(building.GameObject);
            }
        }
    }

    private bool Take_From_Storage(Resource resouce, float amount)
    {
        float amount_taken = 0.0f;
        foreach(Building building in Buildings) {
            float from_building = building.Take_Resources(resouce, amount - amount_taken);
            amount_taken += from_building;
            if(amount_taken == amount) {
                break;
            }
        }
        return amount_taken == amount;
    }
}
