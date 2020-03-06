using System;
using System.Collections.Generic;
using System.Diagnostics;

public enum Resource { Wood, Stone, Lumber, Tools }

public class City {
    private static City instance;

    public string Name { get; private set; }
    public bool Has_Town_Hall { get; private set; }
    public List<Building> Buildings { get; private set; }
    public float Cash { get; private set; }
    public Dictionary<Resource, int> Resource_Totals { get; private set; }

    private City()
    {
        Resource_Totals = new Dictionary<Resource, int>();
        foreach (Resource resource in Enum.GetValues(typeof(Resource))) {
            Resource_Totals.Add(resource, 0);
        }
    }

    public void Start_New()
    {
        Has_Town_Hall = false;
        Buildings = new List<Building>();
        Name = "PLACEHOLDER";
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
        //TODO: Add stopwatch?
        foreach(Building building in Buildings) {
            building.Update(delta_time);
        }
        
        foreach(Resource resource in Enum.GetValues(typeof(Resource))) {
            Resource_Totals[resource] = 0;
        }
        foreach(Building building in Buildings) {
            foreach(KeyValuePair<Resource, int> pair in building.Storage) {
                Resource_Totals[pair.Key] = Resource_Totals[pair.Key] + pair.Value;
            }
        }
        TopGUIManager.Instance.Update_City_Info(Name, Cash, 0.0f, Resource_Totals[Resource.Wood], Resource_Totals[Resource.Lumber], Resource_Totals[Resource.Stone], Resource_Totals[Resource.Tools]);
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
        if (prototype.Is_Town_Hall) {
            Has_Town_Hall = true;
            Building town_hall = new Building(prototype, tile, false);
            Cash = 5000.0f;
            town_hall.Store_Resources(Resource.Wood, 750);
            town_hall.Store_Resources(Resource.Stone, 750);
            town_hall.Store_Resources(Resource.Tools, 500);

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
        Buildings.Add(new Building(prototype, tile, false));
    }

    private bool Take_From_Storage(Resource resouce, int amount)
    {
        int amount_taken = 0;
        foreach(Building building in Buildings) {
            int from_building = building.Take_Resources(resouce, amount - amount_taken);
            amount_taken += from_building;
            if(amount_taken == amount) {
                break;
            }
        }
        return amount_taken == amount;
    }
}
