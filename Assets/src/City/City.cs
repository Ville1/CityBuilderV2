using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class City {
    public static readonly float GRACE_TIME = 180;//180;
    public static readonly bool PAUSED_BUILDINGS_KEEP_WORKERS = false;

    private static City instance;

    public string Name { get; private set; }
    public bool Has_Town_Hall { get; private set; }
    public List<Building> Buildings { get; private set; }
    public float Cash { get; private set; }
    public Dictionary<Resource, float> Resource_Totals { get; private set; }
    public bool Grace_Time { get { return grace_time_remaining > 0.0f; } }
    public float Grace_Time_Remaining { get { return grace_time_remaining; } }
    public Dictionary<Building.Resident, float> Unemployment { get; private set; }
    public Dictionary<Building.Resident, float> Happiness { get; private set; }

    public float Cash_Delta { get; private set; }
    public Dictionary<Resource, float> Resource_Max_Storage { get; private set; }
    public Dictionary<Resource, float> Resource_Delta { get; private set; }
    public float Food_Current { get; private set; }
    public float Food_Max { get; private set; }
    public float Food_Produced { get; private set; }
    public float Food_Consumed { get; private set; }
    public float Food_Delta { get; private set; }

    private float grace_time_remaining;
    private List<Building> removed_buildings;

    private City()
    {
        Resource_Totals = new Dictionary<Resource, float>();
        Resource_Max_Storage = new Dictionary<Resource, float>();
        Resource_Delta = new Dictionary<Resource, float>();
        foreach (Resource resource in Resource.All) {
            Resource_Totals.Add(resource, 0.0f);
            Resource_Max_Storage.Add(resource, 0.0f);
            Resource_Delta.Add(resource, 0.0f);
        }
        Unemployment = new Dictionary<Building.Resident, float>();
        Happiness = new Dictionary<Building.Resident, float>();
        foreach(Building.Resident resident in Enum.GetValues(typeof(Building.Resident))) {
            Unemployment.Add(resident, 0.0f);
            Happiness.Add(resident, 0.0f);
        }
        Cash_Delta = 0.0f;
    }

    public void Start_New(string name)
    {
        Has_Town_Hall = false;
        Buildings = new List<Building>();
        Name = name;
        grace_time_remaining = GRACE_TIME;
        removed_buildings = new List<Building>();
        foreach (Building.Resident resident in Enum.GetValues(typeof(Building.Resident))) {
            Unemployment[resident] = 0.0f;
            Happiness[resident] = 0.0f;
        }
    }

    public void Load(CitySaveData data)
    {
        Start_New(data.Name);
        Cash = data.Cash;
        Has_Town_Hall = data.Buildings.FirstOrDefault(x => x.Internal_Name == Building.TOWN_HALL_INTERNAL_NAME) != null;
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
            grace_time_remaining = Math.Max(grace_time_remaining - TimeManager.Instance.Seconds_To_Days(delta_time), 0.0f);
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
        
        //Update statistics
        foreach(Resource resource in Resource.All) {
            Resource_Totals[resource] = 0.0f;
            Resource_Max_Storage[resource] = 0.0f;
            Resource_Delta[resource] = 0.0f;
        }
        Cash_Delta = 0.0f;
        Food_Current = 0.0f;
        Food_Max = 0.0f;
        Food_Produced = 0.0f;
        Food_Consumed = 0.0f;
        Food_Delta = 0.0f;

        Dictionary<Building.Resident, int> current_population = new Dictionary<Building.Resident, int>();
        Dictionary<Building.Resident, int> max_population = new Dictionary<Building.Resident, int>();
        Dictionary<Building.Resident, float> happiness = new Dictionary<Building.Resident, float>();
        Dictionary<Building.Resident, int> workers_required = new Dictionary<Building.Resident, int>();
        foreach (Building.Resident resident in Enum.GetValues(typeof(Building.Resident))) {
            current_population.Add(resident, 0);
            max_population.Add(resident, 0);
            happiness.Add(resident, 0.0f);
            workers_required.Add(resident, 0);
        }
        foreach (Building building in Buildings) {
            foreach(KeyValuePair<Resource, float> pair in building.Storage) {
                Resource_Totals[pair.Key] += pair.Value;
                if (pair.Key.Is_Food) {
                    Food_Current += pair.Value;
                }
            }
            foreach (KeyValuePair<Resource, float> pair in building.Input_Storage) {
                Resource_Totals[pair.Key] += pair.Value;
                if (pair.Key.Is_Food) {
                    Food_Current += pair.Value;
                }
            }
            foreach (KeyValuePair<Resource, float> pair in building.Output_Storage) {
                Resource_Totals[pair.Key] += pair.Value;
                if (pair.Key.Is_Food) {
                    Food_Current += pair.Value;
                }
            }
            foreach (KeyValuePair<Resource, float> pair in building.Per_Day_Resource_Delta) {
                Resource_Delta[pair.Key] += pair.Value;
            }
            foreach (KeyValuePair<Resource, float> pair in building.Total_Max_Storage) {
                Resource_Max_Storage[pair.Key] += pair.Value;
                if (pair.Key.Is_Food) {
                    Food_Max += pair.Value;
                }
            }
            Cash_Delta += building.Per_Day_Cash_Delta;
            Food_Produced += building.Food_Production_Per_Day;
            Food_Delta += building.Food_Production_Per_Day;
            if (building is Residence) {
                Food_Consumed += (building as Residence).Food_Consumed;
                Food_Delta -= (building as Residence).Food_Consumed;
            }
            if (building.Is_Complete) {
                if (building is Residence && building.Is_Operational) {
                    Residence residence = building as Residence;
                    foreach (Building.Resident resident in Enum.GetValues(typeof(Building.Resident))) {
                        current_population[resident] += residence.Current_Residents[resident];
                        max_population[resident] += residence.Resident_Space[resident];
                        happiness[resident] += residence.Happiness[resident] * residence.Current_Residents[resident];
                    }
                }
                if (building.Requires_Workers && building.Is_Complete && (building.Is_Connected || !building.Requires_Connection) && (!building.Is_Paused || PAUSED_BUILDINGS_KEEP_WORKERS)) {
                    foreach (Building.Resident resident in Enum.GetValues(typeof(Building.Resident))) {
                        workers_required[resident] += building.Worker_Settings[resident];
                    }
                }
            }
        }

        //Allocate workers
        Dictionary<Building.Resident, float> worker_ratios = new Dictionary<Building.Resident, float>();
        Dictionary<Building.Resident, int> available_workers = new Dictionary<Building.Resident, int>();
        Dictionary<Building.Resident, int> workers_allocated = new Dictionary<Building.Resident, int>();
        foreach (Building.Resident resident in Enum.GetValues(typeof(Building.Resident))) {
            worker_ratios.Add(resident, workers_required[resident] == 0 ? 0.0f : current_population[resident] / (float)workers_required[resident]);
            available_workers.Add(resident, current_population[resident]);
            workers_allocated.Add(resident, 0);
        }
        foreach (Building building in Buildings) {
            if (!building.Requires_Workers || !(building.Is_Complete && (building.Is_Connected || !building.Requires_Connection) && (!building.Is_Paused || PAUSED_BUILDINGS_KEEP_WORKERS))) {
                continue;
            }
            foreach (Building.Resident resident in Enum.GetValues(typeof(Building.Resident))) {
                building.Current_Workers[resident] = 0;
            }
            if (building.Is_Complete && (building.Is_Connected || !building.Requires_Connection) && (!building.Is_Paused || PAUSED_BUILDINGS_KEEP_WORKERS)) {
                foreach (Building.Resident resident in Enum.GetValues(typeof(Building.Resident))) {
                    int workers = Mathf.RoundToInt(building.Worker_Settings[resident] * Mathf.Clamp(worker_ratios[resident], 0.0f, 1.0f));
                    if(workers > available_workers[resident]) {
                        workers = available_workers[resident];
                    }
                    building.Current_Workers[resident] = workers;
                    available_workers[resident] -= workers;
                    workers_allocated[resident] += workers;
                    if (available_workers[resident] < 0) {
                        CustomLogger.Instance.Error("More workers allocated, than there is available workers");
                    }
                }
            }
        }

        foreach (Building building in Buildings) {
            if (!building.Requires_Workers || !(building.Is_Complete && (building.Is_Connected || !building.Requires_Connection) && (!building.Is_Paused || PAUSED_BUILDINGS_KEEP_WORKERS))) {
                continue;
            }
            foreach (Building.Resident resident in Enum.GetValues(typeof(Building.Resident))) {
                if (available_workers[resident] > 0 && worker_ratios[resident] < 1.0f && building.Current_Workers[resident] < building.Worker_Settings[resident]) {
                    building.Current_Workers[resident]++;
                    available_workers[resident]--;
                    workers_allocated[resident]++;
                }
            }
        }

        foreach (Building.Resident resident in Enum.GetValues(typeof(Building.Resident))) {
            if(available_workers[resident] > 0 && workers_allocated[resident] < workers_required[resident]) {
                CustomLogger.Instance.Error("Worker allocation mismatch, not enough allocated");
            }
            if(workers_allocated[resident] > workers_required[resident]) {
                CustomLogger.Instance.Error("Worker allocation mismatch, too many allocated");
            }
            if (workers_allocated[resident] > current_population[resident]) {
                CustomLogger.Instance.Error("Worker allocation mismatch, allocated > population");
            }
        }

        //GUI
        int peasant_current = current_population[Building.Resident.Peasant];
        int peasant_max = max_population[Building.Resident.Peasant];
        float peasant_happiness = peasant_current > 0 ? happiness[Building.Resident.Peasant] / peasant_current : 0.0f;
        Happiness[Building.Resident.Peasant] = peasant_happiness;
        int peasant_employment = workers_allocated[Building.Resident.Peasant] - workers_required[Building.Resident.Peasant] + available_workers[Building.Resident.Peasant];
        float peasant_employment_relative = peasant_current == 0 ? 0.0f : peasant_employment / (float)peasant_current;
        Unemployment[Building.Resident.Peasant] = peasant_employment_relative > 0.0f ? peasant_employment_relative : 0.0f;

        int citizen_current = current_population[Building.Resident.Citizen];
        int citizen_max = max_population[Building.Resident.Citizen];
        float citizen_happiness = citizen_current > 0 ? happiness[Building.Resident.Citizen] / citizen_current : 0.0f;
        Happiness[Building.Resident.Citizen] = peasant_happiness;
        int citizen_employment = workers_allocated[Building.Resident.Citizen] - workers_required[Building.Resident.Citizen] + available_workers[Building.Resident.Citizen];
        float citizen_employment_relative = citizen_current == 0 ? 0.0f : citizen_employment / (float)citizen_current;
        Unemployment[Building.Resident.Citizen] = citizen_employment_relative > 0.0f ? citizen_employment_relative : 0.0f;

        int noble_current = current_population[Building.Resident.Noble];
        int noble_max = max_population[Building.Resident.Noble];
        float noble_happiness = noble_current > 0 ? happiness[Building.Resident.Noble] / noble_current : 0.0f;
        Happiness[Building.Resident.Noble] = peasant_happiness;
        int noble_employment = workers_allocated[Building.Resident.Noble] - workers_required[Building.Resident.Noble] + available_workers[Building.Resident.Noble];
        float noble_employment_relative = noble_current == 0 ? 0.0f : noble_employment / (float)noble_current;
        Unemployment[Building.Resident.Citizen] = noble_employment_relative > 0.0f ? noble_employment_relative : 0.0f;
        TopGUIManager.Instance.Update_City_Info(Name, Cash, Cash_Delta, Mathf.RoundToInt(Resource_Totals[Resource.Wood]), Mathf.RoundToInt(Resource_Totals[Resource.Lumber]), Mathf.RoundToInt(Resource_Totals[Resource.Stone]),
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

    public void Take_Cash(float amount)
    {
        if (amount <= 0.0f) {
            CustomLogger.Instance.Warning(string.Format("amount = {0}", amount));
            return;
        }
        Cash -= amount;
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
        if ((prototype.Permitted_Terrain.Count == 0 && !tile.Buildable) || (prototype.Permitted_Terrain.Count > 0 && !prototype.Permitted_Terrain.Contains(tile.Internal_Name))) {
            message = "Invalid terrain";
            return false;
        }
        foreach(Tile t in Map.Instance.Get_Tiles(tile.Coordinates, prototype.Width, prototype.Height)) {
            if ((prototype.Permitted_Terrain.Count == 0 && !t.Buildable) || (prototype.Permitted_Terrain.Count > 0 && !prototype.Permitted_Terrain.Contains(t.Internal_Name))) {
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
            return;
        }
        Cash -= prototype.Cash_Cost;
        foreach(KeyValuePair<Resource, int> pair in prototype.Cost) {
            if(Take_From_Storage(pair.Key, pair.Value) != pair.Value) {
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

    public CitySaveData Save_Data()
    {
        return new CitySaveData() {
            Name = Name,
            Cash = Cash,
            Buildings = new List<BuildingSaveData>()
        };
    }

    public void Delete()
    {
        if (Buildings != null) {
            foreach (Building building in Buildings) {
                GameObject.Destroy(building.GameObject);
            }
        }
    }

    public float Add_To_Storage(Resource resouce, float amount)
    {
        float amount_added = 0.0f;
        foreach (Building building in Buildings) {
            if (!building.Allowed_Resources.Contains(resouce)) {
                continue;
            }
            amount_added += building.Store_Resources(resouce, amount - amount_added);
        }
        return amount_added;
    }

    public float Take_From_Storage(Resource resouce, float amount)
    {
        float amount_taken = 0.0f;
        foreach(Building building in Buildings) {
            float from_building = building.Take_Resources(resouce, amount - amount_taken);
            amount_taken += from_building;
            if(amount_taken == amount) {
                break;
            }
        }
        return amount_taken;
    }
}
