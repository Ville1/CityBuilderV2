using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

public class City {
    public static readonly float GRACE_TIME = 120;
    public static readonly bool PAUSED_BUILDINGS_KEEP_WORKERS = false;
    public static readonly int MAX_COLONY_LOCATIONS = 10;

    private static City instance;

    public string Name { get; private set; }
    public bool Has_Town_Hall { get; private set; }
    public List<Building> Buildings { get; private set; }
    public float Cash { get; private set; }
    public Dictionary<Resource, float> Resource_Totals { get; private set; }
    public Dictionary<Resource, float> Usable_Resource_Totals { get; private set; }
    public bool Grace_Time { get { return grace_time_remaining > 0.0f; } }
    public float Grace_Time_Remaining { get { return grace_time_remaining; } set { grace_time_remaining = value; } }
    public Dictionary<Building.Resident, float> Unemployment { get; private set; }
    public Dictionary<Building.Resident, float> Happiness { get; private set; }
    public Dictionary<Building.Resident, float> Education { get; private set; }

    public float Cash_Delta { get; private set; }
    public Dictionary<Resource, float> Resource_Max_Storage { get; private set; }
    public Dictionary<Resource, float> Resource_Delta { get; private set; }
    public float Food_Current { get; private set; }
    public float Food_Max { get; private set; }
    public float Food_Produced { get; private set; }
    public float Food_Consumed { get; private set; }
    public float Food_Delta { get; private set; }
    public List<Expedition> Expeditions { get; private set; }
    public List<Entity> Walkers { get; private set; }
    public List<ColonyLocation> Colony_Locations { get; private set; }
    public bool Spawn_Walkers { get; set; }

    public bool Ignore_Citizen_Needs { get; set; }
    public bool Ignore_All_Needs { get; set; }

    private float grace_time_remaining;
    private List<Building> removed_buildings;
    private List<Building> added_buildings;
    private List<Expedition> removed_expeditions;
    private List<Expedition> added_expeditions;
    private Dictionary<Building, Entity> ships;
    private float ship_spawn_cooldown;
    private int noble_count;

    private City()
    {
        Resource_Totals = new Dictionary<Resource, float>();
        Usable_Resource_Totals = new Dictionary<Resource, float>();
        Resource_Max_Storage = new Dictionary<Resource, float>();
        Resource_Delta = new Dictionary<Resource, float>();
        foreach (Resource resource in Resource.All) {
            Resource_Totals.Add(resource, 0.0f);
            Usable_Resource_Totals.Add(resource, 0.0f);
            Resource_Max_Storage.Add(resource, 0.0f);
            Resource_Delta.Add(resource, 0.0f);
        }
        Unemployment = new Dictionary<Building.Resident, float>();
        Happiness = new Dictionary<Building.Resident, float>();
        Education = new Dictionary<Building.Resident, float>();
        foreach(Building.Resident resident in Enum.GetValues(typeof(Building.Resident))) {
            Unemployment.Add(resident, 0.0f);
            Happiness.Add(resident, 0.0f);
            Education.Add(resident, 0.0f);
        }
        Cash_Delta = 0.0f;
        Expeditions = new List<Expedition>();
        Walkers = new List<Entity>();
        ships = new Dictionary<Building, Entity>();
        ship_spawn_cooldown = 0.0f;
        Colony_Locations = new List<ColonyLocation>();
        noble_count = 0;
        Spawn_Walkers = true;
    }

    public void Start_New(string name)
    {
        Has_Town_Hall = false;
        Buildings = new List<Building>();
        Name = name;
        grace_time_remaining = GRACE_TIME;
        removed_buildings = new List<Building>();
        added_buildings = new List<Building>();
        removed_expeditions = new List<Expedition>();
        added_expeditions = new List<Expedition>();
        Walkers.Clear();
        ships.Clear();
        ship_spawn_cooldown = 0.0f;
        Colony_Locations.Clear();
        noble_count = 0;
        foreach (Building.Resident resident in Enum.GetValues(typeof(Building.Resident))) {
            Unemployment[resident] = 0.0f;
            Happiness[resident] = 0.0f;
            Education[resident] = 0.0f;
        }
    }

    public void Load(CitySaveData data)
    {
        Start_New(data.Name);
        Cash = data.Cash;
        Has_Town_Hall = data.Buildings.FirstOrDefault(x => x.Internal_Name == Building.TOWN_HALL_INTERNAL_NAME) != null;
        Contacts.Instance.Load(data.Contacts);
        Expeditions = new List<Expedition>();
        foreach(ExpeditionSaveData expedition in data.Expeditions) {
            Expeditions.Add(new Expedition((Expedition.ExpeditionGoal)expedition.Goal, (Expedition.ExpeditionLenght)expedition.Lenght, expedition.Building_Id, expedition.Resource == -1 ? null : Resource.All.First(x => (int)x.Type == expedition.Resource),
                expedition.Time_Remaining, (Expedition.ExpeditionState)expedition.State, expedition.Colony_Data == null ? null : new ColonyLocation(expedition.Colony_Data)));
        }
        foreach(ColonyLocationSaveData colony_location in data.Colony_Locations) {
            Colony_Locations.Add(new ColonyLocation(colony_location));
        }
    }

    public void Finalize_Load(CitySaveData data)
    {
        foreach(Building building in Buildings) {
            building.Finalize_Load(data.Buildings.First(x => x.Id == building.Id));
        }
    }

    public void Update_Grace_Time(float total_days)
    {
        if(total_days >= GRACE_TIME) {
            grace_time_remaining = 0.0f;
        } else {
            grace_time_remaining = GRACE_TIME - total_days;
        }
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
        DiagnosticsManager.Instance.Message(GetType(), "--- UPDATE STARTS ---");
        DiagnosticsManager.Instance.Start(GetType(), "total", "Total");
        DiagnosticsManager.Instance.Start(GetType(), "expeditions", "Expeditions");
        
        if (grace_time_remaining > 0.0f) {
            grace_time_remaining = Math.Max(grace_time_remaining - TimeManager.Instance.Seconds_To_Days(delta_time), 0.0f);
        }
        //TODO: Add stopwatch?
        //Expeditions
        foreach (Expedition expedition in Expeditions) {
            expedition.Update(delta_time);
        }
        DiagnosticsManager.Instance.End(GetType(), "expeditions");
        DiagnosticsManager.Instance.Start(GetType(), "buildings", "Buildings");

        //Buildings
        foreach (Building building in Buildings) {
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
        foreach (Building building in added_buildings) {
            Buildings.Add(building);
        }
        added_buildings.Clear();

        foreach (Expedition expedition in removed_expeditions) {
            Expeditions.Remove(expedition);
        }
        removed_expeditions.Clear();
        foreach (Expedition expedition in added_expeditions) {
            Expeditions.Add(expedition);
        }
        added_expeditions.Clear();
        
        DiagnosticsManager.Instance.End(GetType(), "buildings");
        DiagnosticsManager.Instance.Start(GetType(), "statistics", "Update statistics");

        //Update statistics
        foreach (Resource resource in Resource.All) {
            Resource_Totals[resource] = 0.0f;
            Usable_Resource_Totals[resource] = 0.0f;
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
        Dictionary<Building.Resident, int> available_workers = new Dictionary<Building.Resident, int>();
        Dictionary<Building.Resident, float> total_education = new Dictionary<Building.Resident, float>();
        foreach (Building.Resident resident in Enum.GetValues(typeof(Building.Resident))) {
            current_population.Add(resident, 0);
            max_population.Add(resident, 0);
            happiness.Add(resident, 0.0f);
            workers_required.Add(resident, 0);
            available_workers.Add(resident, 0);
            total_education.Add(resident, 0.0f);
        }
        foreach (Building building in Buildings) {
            foreach(KeyValuePair<Resource, float> pair in building.Storage) {
                Resource_Totals[pair.Key] += pair.Value;
                Usable_Resource_Totals[pair.Key] += pair.Value;
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
                        available_workers[resident] += residence.Available_Work_Force[resident];
                        max_population[resident] += residence.Resident_Space[resident];
                        happiness[resident] += residence.Happiness[resident] * residence.Current_Residents[resident];
                        total_education[resident] += residence.Education(resident) * residence.Current_Residents[resident];
                    }
                }
                if (building.Requires_Workers && building.Is_Complete && (building.Is_Connected || !building.Requires_Connection) && (!building.Is_Paused || PAUSED_BUILDINGS_KEEP_WORKERS)) {
                    foreach (Building.Resident resident in Enum.GetValues(typeof(Building.Resident))) {
                        workers_required[resident] += building.Worker_Settings[resident];
                    }
                }
            }
        }
        DiagnosticsManager.Instance.End(GetType(), "statistics");
        DiagnosticsManager.Instance.Start(GetType(), "workers", "Allocate workers");

        //Allocate workers
        Dictionary<Building.Resident, float> worker_ratios = new Dictionary<Building.Resident, float>();
        Dictionary<Building.Resident, int> workers_allocated = new Dictionary<Building.Resident, int>();
        foreach (Building.Resident resident in Enum.GetValues(typeof(Building.Resident))) {
            worker_ratios.Add(resident, workers_required[resident] == 0 ? 0.0f : current_population[resident] / (float)workers_required[resident]);
            workers_allocated.Add(resident, 0);
        }
        foreach (Building building in Buildings) {
            if (!building.Requires_Workers) {
                continue;
            }
            foreach (Building.Resident resident in Enum.GetValues(typeof(Building.Resident))) {
                building.Current_Workers[resident] = 0;
            }
            if(!(building.Is_Complete && (building.Is_Connected || !building.Requires_Connection) && (!building.Is_Paused || PAUSED_BUILDINGS_KEEP_WORKERS))) {
                continue;
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
        DiagnosticsManager.Instance.End(GetType(), "workers");
        DiagnosticsManager.Instance.Start(GetType(), "walkers", "Spawn walkers");

        //Spawn walkers
        int max = Spawn_Walkers ? Mathf.Min(Mathf.RoundToInt((current_population[Building.Resident.Peasant] + current_population[Building.Resident.Citizen] + current_population[Building.Resident.Noble]) / 30.0f) + 1, 10) : 0;
        if (Walkers.Count < max) {
            List<Building> possible_buildings = Buildings.Where(x => x.Is_Connected && x.Requires_Connection && x.Is_Operational && !x.Is_Road && x.Entities_Spawned.Count == 0).ToList();
            if (possible_buildings.Count > 25) {//TODO
                //TODO: Implement faster pathfinding A* ?
                if (Buildings.Count <= 1) {
                    //Use pathfinding for paths
                    Building spawner = RNG.Instance.Item(possible_buildings);
                    possible_buildings.Remove(spawner);
                    if (possible_buildings.Count >= 1) {
                        Building road = null;
                        foreach (Building b in Map.Instance.Get_Buildings_Around(spawner)) {
                            if (b.Is_Road && b.Is_Built && b.Is_Connected) {
                                road = b;
                                break;
                            }
                        }
                        if (road != null) {
                            Building target_building = RNG.Instance.Item(possible_buildings);//TODO: allow x.Entities_Spawned.Count != 0 in this list
                            Building target_road = null;
                            foreach (Building b in Map.Instance.Get_Buildings_Around(target_building)) {
                                if (b.Is_Road && b.Is_Built && b.Is_Connected) {
                                    target_road = b;
                                    break;
                                }
                            }
                            if (target_road != null) {
                                List<PathfindingNode> path = Pathfinding.Path(Map.Instance.Road_Pathing, road.Tile.PathfindingNode, target_road.Tile.Road_PathfindingNode, false);
                                if (path.Count > 2) {
                                    Entity walker = new Entity(EntityPrototypes.Instance.Get("walker"), road.Tile, spawner);
                                    walker.Set_Path(path, 0.5f);
                                    Walkers.Add(walker);
                                }
                            }
                        }
                    }
                } else {
                    //Use connected buildings for paths
                    Tile spawn = null;
                    Building spawner = null;
                    List<PathfindingNode> path = Entity.Find_Walker_City_Path(out spawn, out spawner);
                    if(path != null) {
                        Entity walker = new Entity(EntityPrototypes.Instance.Get("walker"), spawn, spawner);
                        walker.Set_Path(path, 0.5f);
                        Walkers.Add(walker);
                    }
                }
            }
        }
        DiagnosticsManager.Instance.End(GetType(), "walkers");
        DiagnosticsManager.Instance.Start(GetType(), "ships", "Spawn ships");

        //Spawn ships
        if (Map.Instance.Ship_Spawns.Count > 0) {
            ship_spawn_cooldown -= delta_time * TimeManager.Instance.Multiplier;
            if (ship_spawn_cooldown <= 0.0f) {
                ship_spawn_cooldown += 120.0f * (200.0f / (100.0f + ((0.5f * current_population[Building.Resident.Peasant]) + (1.5f * current_population[Building.Resident.Citizen]) + current_population[Building.Resident.Noble]))) *
                    (RNG.Instance.Next(50, 150) * 0.01f);
                foreach (Building building in Buildings) {
                    if (!building.Data.ContainsKey(Building.DOCK_ID_KEY)) {
                        continue;
                    }
                    Building dock = Buildings.FirstOrDefault(x => x.Id == long.Parse(building.Data[Building.DOCK_ID_KEY]));
                    if (dock == null || ships.ContainsKey(dock) || building.Tags.Contains(Building.Tag.Creates_Expeditions) || !building.Is_Operational) {
                        continue;
                    }
                    Tile spawn = RNG.Instance.Item(Map.Instance.Ship_Spawns);
                    Tile target = null;
                    foreach(Tile t in Map.Instance.Get_Adjanced_Tiles(dock.Tile).Select(x => x.Value).ToArray()) {
                        if(t.Building == null && t.Has_Ship_Access) {
                            target = t;
                            break;
                        }
                    }
                    if(target == null) {
                        break;
                    }
                    List<PathfindingNode> path = Pathfinding.Path(Map.Instance.Ship_Pathing, spawn.Ship_PathfindingNode, target.Ship_PathfindingNode, false);
                    if(path.Count > 2) {
                        List<PathfindingNode> return_path = Pathfinding.Path(Map.Instance.Ship_Pathing, target.Ship_PathfindingNode, spawn.Ship_PathfindingNode, false);
                        path.AddRange(return_path.Where(x => !x.Coordinates.Equals(target.Coordinates)).ToList());
                        Entity ship = new Entity(EntityPrototypes.Instance.Get("ship"), spawn, dock);
                        ship.Set_Path(path, 0.75f);
                        ship.Add_Order(new Entity.PathOrder(target.Ship_PathfindingNode, 20.0f));
                        ships.Add(dock, ship);
                    }
                }
            }
        }
        DiagnosticsManager.Instance.End(GetType(), "ships");
        DiagnosticsManager.Instance.Start(GetType(), "gui", "Update GUI");

        //GUI
        int peasant_current = current_population[Building.Resident.Peasant];
        int peasant_max = max_population[Building.Resident.Peasant];
        float peasant_happiness = peasant_current > 0 ? happiness[Building.Resident.Peasant] / peasant_current : 0.0f;
        Happiness[Building.Resident.Peasant] = peasant_happiness;
        int peasant_employment = workers_allocated[Building.Resident.Peasant] - workers_required[Building.Resident.Peasant] + available_workers[Building.Resident.Peasant];
        float peasant_employment_relative = peasant_current == 0 ? 0.0f : peasant_employment / (float)peasant_current;
        Unemployment[Building.Resident.Peasant] = peasant_employment_relative > 0.0f ? peasant_employment_relative : 0.0f;
        Education[Building.Resident.Peasant] = peasant_current == 0 ? 0.0f : total_education[Building.Resident.Peasant] / peasant_current;

        int citizen_current = current_population[Building.Resident.Citizen];
        int citizen_max = max_population[Building.Resident.Citizen];
        float citizen_happiness = citizen_current > 0 ? happiness[Building.Resident.Citizen] / citizen_current : 0.0f;
        Happiness[Building.Resident.Citizen] = citizen_happiness;
        int citizen_employment = workers_allocated[Building.Resident.Citizen] - workers_required[Building.Resident.Citizen] + available_workers[Building.Resident.Citizen];
        float citizen_employment_relative = citizen_current == 0 ? 0.0f : citizen_employment / (float)citizen_current;
        Unemployment[Building.Resident.Citizen] = citizen_employment_relative > 0.0f ? citizen_employment_relative : 0.0f;
        Education[Building.Resident.Citizen] = citizen_current == 0 ? 0.0f : total_education[Building.Resident.Citizen] / citizen_current;

        int noble_current = current_population[Building.Resident.Noble];
        int noble_max = max_population[Building.Resident.Noble];
        float noble_happiness = noble_current > 0 ? happiness[Building.Resident.Noble] / noble_current : 0.0f;
        Happiness[Building.Resident.Noble] = noble_happiness;
        int noble_employment = workers_allocated[Building.Resident.Noble] - workers_required[Building.Resident.Noble] + available_workers[Building.Resident.Noble];
        float noble_employment_relative = noble_current == 0 ? 0.0f : noble_employment / (float)noble_current;
        Unemployment[Building.Resident.Noble] = noble_employment_relative > 0.0f ? noble_employment_relative : 0.0f;
        Education[Building.Resident.Noble] = noble_current == 0 ? 0.0f : total_education[Building.Resident.Noble] / noble_current;
        noble_count = noble_current;

        TopGUIManager.Instance.Update_City_Info(Name, Cash, Cash_Delta, Mathf.RoundToInt(Usable_Resource_Totals[Resource.Wood]), Mathf.RoundToInt(Usable_Resource_Totals[Resource.Lumber]), Mathf.RoundToInt(Usable_Resource_Totals[Resource.Stone]),
            Mathf.RoundToInt(Usable_Resource_Totals[Resource.Bricks]), Mathf.RoundToInt(Usable_Resource_Totals[Resource.Tools]), Mathf.RoundToInt(Usable_Resource_Totals[Resource.Marble]), Mathf.RoundToInt(Usable_Resource_Totals[Resource.Mechanisms]),
            Mathf.RoundToInt(Usable_Resource_Totals[Resource.Glass]), peasant_current, peasant_max, peasant_happiness, peasant_employment_relative, peasant_employment, citizen_current, citizen_max, citizen_happiness,
            citizen_employment_relative, citizen_employment, noble_current, noble_max, noble_happiness, noble_employment_relative, noble_employment);

        DiagnosticsManager.Instance.End(GetType(), "gui");
        DiagnosticsManager.Instance.End(GetType(), "total");
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
            message = "Only one town hall allowed per city";
            return false;
        }
        if (prototype.Tags.Contains(Building.Tag.Unique)) {
            foreach(Building building in Buildings) {
                if(building.Internal_Name == prototype.Internal_Name) {
                    message = string.Format("Only one {0} allowed per city", prototype.Name.ToLower());
                    return false;
                }
            }
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
        if (!prototype.Tags.Contains(Building.Tag.Bridge)) {
            if (prototype.Cash_Cost > Cash) {
                message = string.Format("Not enough cash, {0} needed", prototype.Cash_Cost);
                return false;
            }
            foreach (KeyValuePair<Resource, int> pair in prototype.Cost) {
                if (Usable_Resource_Totals[pair.Key] < pair.Value) {
                    message = string.Format("Not enough {0}, {1} required", pair.Key.ToString().ToLower(), pair.Value);
                    return false;
                }
            }
        } else {
            List<Tile> bridge_tiles = Bridge_Tiles(tile);
            foreach (Tile t in bridge_tiles) {
                foreach(Tile t2 in Map.Instance.Get_Adjanced_Tiles(t).Select(x => x.Value).ToArray()) {
                    if(t2.Building != null && t2.Building.Tags.Contains(Building.Tag.Bridge)) {
                        message = "Too close to existing bridge";
                        return false;
                    }
                }
            }
            foreach (KeyValuePair<Resource, int> pair in Bridge_Cost(prototype, bridge_tiles)) {
                if (Usable_Resource_Totals[pair.Key] < pair.Value) {
                    message = string.Format("Not enough {0}, {1} required", pair.Key.ToString().ToLower(), pair.Value);
                    return false;
                }
            }
            if (prototype.Cash_Cost * bridge_tiles.Count > Cash) {
                message = string.Format("Not enough cash, {0} needed", prototype.Cash_Cost * bridge_tiles.Count);
                return false;
            }
        }
        if(prototype.On_Build_Check != null) {
            if(!prototype.On_Build_Check(prototype, tile, out message)) {
                return false;
            }
        }
        return true;
    }

    public void Build(Building prototype, Tile tile, bool bridge_part = false)
    {
        string message;
        if(!bridge_part && !Can_Build(prototype, out message)) {
            return;
        }
        List<Tile> tiles = Map.Instance.Get_Tiles(tile.Coordinates, prototype.Width, prototype.Height);
        if (prototype.Is_Town_Hall) {
            Has_Town_Hall = true;
            Building town_hall = new Building(prototype, tile, tiles, false);
            Cash = 12000.0f;
            town_hall.Store_Resources(Resource.Wood, 800.0f);
            town_hall.Store_Resources(Resource.Stone, 600.0f);
            town_hall.Store_Resources(Resource.Tools, 1100.0f);
            Buildings.Add(town_hall);
            return;
        }
        Cash -= prototype.Cash_Cost;
        foreach(KeyValuePair<Resource, int> pair in prototype.Cost) {
            float taken = Take_From_Storage(pair.Key, pair.Value);
            if (taken != pair.Value) {
                CustomLogger.Instance.Error(string.Format("Failed to take {0} {1}, only {2} taken", pair.Value, pair.Key, taken));
            }
        }
        if(prototype.Tags.Contains(Building.Tag.Bridge) && !BuildMenuManager.Instance.Flip_Bridge) {
            Tile west_tile = Map.Instance.Get_Tile_At(tile.Coordinates, Coordinates.Direction.West);
            Tile east_tile = Map.Instance.Get_Tile_At(tile.Coordinates, Coordinates.Direction.East);
            if(west_tile != null && east_tile != null) {
                if(west_tile.Is_Water && !east_tile.Is_Water) {
                    prototype.Switch_Selected_Sprite(3);
                } else if(!west_tile.Is_Water && east_tile.Is_Water) {
                    prototype.Switch_Selected_Sprite(2);
                } else {
                    prototype.Switch_Selected_Sprite(0);
                }
            }
        }
        if(BuildingPrototypes.Instance.Is_Residence(prototype.Internal_Name)) {
            Residence residence = new Residence(BuildingPrototypes.Instance.Get_Residence(prototype.Internal_Name) as Residence, tile, tiles, false);
            residence.Selected_Sprite = prototype.Selected_Sprite;
            Buildings.Add(residence);
        } else {
            Building building = new Building(prototype, tile, tiles, false);
            building.Selected_Sprite = prototype.Selected_Sprite;
            Buildings.Add(building);
        }
        if (!bridge_part) {
            if (prototype.Tags.Contains(Building.Tag.Bridge)) {
                foreach (Tile t in Bridge_Tiles(tile)) {
                    Build(prototype, t, true);
                }
            }
        }
    }

    public void Remove_Building(Building building)
    {
        if (!removed_buildings.Contains(building)) {
            removed_buildings.Add(building);
        }
    }

    public void Add_Building(Building building)
    {
        if (!added_buildings.Contains(building)) {
            added_buildings.Add(building);
        }
    }

    public void Remove_Expedition(Expedition expedition)
    {
        if (!removed_expeditions.Contains(expedition)) {
            removed_expeditions.Add(expedition);
        }
    }

    public void Add_Expedition(Expedition expedition)
    {
        if (!added_expeditions.Contains(expedition)) {
            added_expeditions.Add(expedition);
        }
    }

    public void Delete_Ship(Entity ship)
    {
        if (ships.ContainsValue(ship)) {
            ships.Remove(ships.First(x => x.Value == ship).Key);
        }
    }

    public void Add_Colony_Location(ColonyLocation location)
    {
        if(Colony_Locations.Count < MAX_COLONY_LOCATIONS) {
            Colony_Locations.Add(location);
        } else {
            for(int i = 0; i < MAX_COLONY_LOCATIONS - 1; i++) {
                Colony_Locations[i + 1] = Colony_Locations[i];
            }
            Colony_Locations[0] = location;
        }
    }

    public void Remove_Colony_Location(ColonyLocation location)
    {
        if (Colony_Locations.Contains(location)) {
            Colony_Locations.Remove(location);
        }
    }

    public List<ForeignCity> Colonies
    {
        get {
            return Contacts.Instance.Cities.Where(x => x.City_Type == ForeignCity.CityType.Colony).ToList();
        }
    }

    public int Max_Colonies
    {
        get {
            if(noble_count == 0) {
                return 0;
            }
            if(noble_count < 50) {
                return 1;
            }
            if(noble_count < 100) {
                return 2;
            }
            if (noble_count < 250) {
                return 3;
            }
            return 4;
        }
    }

    public float Colony_Effectiveness
    {
        get {
            return Colonies.Count <= Max_Colonies ? 1.0f : (Colonies.Count == Max_Colonies + 1 ? 0.35f : 0.0f);
        }
    }

    public CitySaveData Save_Data()
    {
        return new CitySaveData() {
            Name = Name,
            Cash = Cash,
            Buildings = new List<BuildingSaveData>(),
            Contacts = Contacts.Instance.Save(),
            Expeditions = Expeditions.Select(x => new ExpeditionSaveData() { Goal = (int)x.Goal, Lenght = (int)x.Lenght, Building_Id = x.Building_Id, Resource = x.Resource != null ? (int)x.Resource.Type : -1,
                State = (int)x.State, Time_Remaining = x.Time_Remaining, Colony_Data = x.Colony_Data == null ? null : x.Colony_Data.Save_Data }).ToList(),
            Colony_Locations = Colony_Locations.Select(x => x.Save_Data).ToList()
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
            if (!building.Allowed_Resources.Contains(resouce) || !building.Is_Complete) {
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

    public bool Has_Outside_Road_Connection()
    {
        bool has_connection = false;
        foreach (Building building in Buildings) {
            if (building.Is_Road && building.Is_Connected) {
                foreach (Tile tile in building.Tiles) {
                    if (Map.Instance.Is_Edge_Tile(tile)) {
                        has_connection = true;
                        break;
                    }
                }
            }
            if (has_connection) {
                break;
            }
        }
        return has_connection;
    }

    private List<Tile> Bridge_Tiles(Tile tile)
    {
        List<Tile> bridge_tiles = new List<Tile>();
        Tile next_tile = Map.Instance.Get_Tile_At(tile.Coordinates, BuildMenuManager.Instance.Flip_Bridge ? Coordinates.Direction.North : Coordinates.Direction.East);
        while (next_tile != null && next_tile.Is_Water && next_tile.Building == null) {
            bridge_tiles.Add(next_tile);
            next_tile = Map.Instance.Get_Tile_At(next_tile.Coordinates, BuildMenuManager.Instance.Flip_Bridge ? Coordinates.Direction.North : Coordinates.Direction.East);
        }
        next_tile = Map.Instance.Get_Tile_At(tile.Coordinates, BuildMenuManager.Instance.Flip_Bridge ? Coordinates.Direction.South : Coordinates.Direction.West);
        while (next_tile != null && next_tile.Is_Water && next_tile.Building == null) {
            bridge_tiles.Add(next_tile);
            next_tile = Map.Instance.Get_Tile_At(next_tile.Coordinates, BuildMenuManager.Instance.Flip_Bridge ? Coordinates.Direction.South : Coordinates.Direction.West);
        }
        return bridge_tiles;
    }

    private Dictionary<Resource, int> Bridge_Cost(Building prototype, List<Tile> bridge_tiles)
    {
        if (!prototype.Tags.Contains(Building.Tag.Bridge)) {
            CustomLogger.Instance.Error(string.Format("{0} is not a bridge", prototype.Internal_Name));
            return null;
        }
        Dictionary<Resource, int> total_cost = new Dictionary<Resource, int>();
        foreach (KeyValuePair<Resource, int> pair in prototype.Cost) {
            total_cost.Add(pair.Key, (bridge_tiles.Count + 1) * pair.Value);
        }
        return total_cost;
    }
}
