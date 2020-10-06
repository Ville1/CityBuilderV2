using System.Collections.Generic;
using System.Linq;

public class Expedition {
    public enum ExpeditionLenght { Short, Medium, Long }
    public enum ExpeditionGoal { Collect_Resources, Find_Colony_Locations, Establish_Colony }
    public enum ExpeditionState { Active, Returning, Finished }

    public static readonly Dictionary<ExpeditionGoal, Dictionary<ExpeditionLenght, float>> COSTS = new Dictionary<ExpeditionGoal, Dictionary<ExpeditionLenght, float>>() {
        { ExpeditionGoal.Collect_Resources, new Dictionary<ExpeditionLenght, float>() {
            { ExpeditionLenght.Short, 100.0f },
            { ExpeditionLenght.Medium, 750.0f },
            { ExpeditionLenght.Long, 2500.0f }
        } },
        { ExpeditionGoal.Find_Colony_Locations, new Dictionary<ExpeditionLenght, float>() {
            { ExpeditionLenght.Short, 100.0f },
            { ExpeditionLenght.Medium, 500.0f },
            { ExpeditionLenght.Long, 1000.0f }
        } },
        { ExpeditionGoal.Establish_Colony, new Dictionary<ExpeditionLenght, float>() {
            { ExpeditionLenght.Short, 3000.0f },
            { ExpeditionLenght.Medium, 3000.0f },
            { ExpeditionLenght.Long, 3000.0f }
        } }
    };
    public static readonly Dictionary<ExpeditionGoal, Dictionary<ExpeditionLenght, int[]>> ACTIVE_TIME = new Dictionary<ExpeditionGoal, Dictionary<ExpeditionLenght, int[]>>() {
        { ExpeditionGoal.Collect_Resources, new Dictionary<ExpeditionLenght, int[]>() {
            { ExpeditionLenght.Short,  new int[2] { 10, 20 } }, //ave 15
            { ExpeditionLenght.Medium, new int[2] { 25, 50 } }, //ave 37.5
            { ExpeditionLenght.Long,   new int[2] { 50, 75 } }  //ave 62.5‬
        } },
        { ExpeditionGoal.Find_Colony_Locations, new Dictionary<ExpeditionLenght, int[]>() {
            { ExpeditionLenght.Short,  new int[2] { 10, 20 } }, //ave 15
            { ExpeditionLenght.Medium, new int[2] { 25, 50 } }, //ave 37.5
            { ExpeditionLenght.Long,   new int[2] { 50, 75 } }  //ave 62.5‬
        } },
        { ExpeditionGoal.Establish_Colony, new Dictionary<ExpeditionLenght, int[]>() {
            { ExpeditionLenght.Short , new int[2] { 60, 80 } }, //ave 70
            { ExpeditionLenght.Medium, new int[2] { 60, 80 } }, //ave 70
            { ExpeditionLenght.Long,   new int[2] { 60, 80 } }  //ave 70
        } }
    };
    public static readonly Dictionary<ExpeditionGoal, Dictionary<ExpeditionLenght, int[]>> RETURN_TIME = new Dictionary<ExpeditionGoal, Dictionary<ExpeditionLenght, int[]>>() {
        { ExpeditionGoal.Collect_Resources, new Dictionary<ExpeditionLenght, int[]>() {
            { ExpeditionLenght.Short, new int[2]  { 5, 10 } },  //ave 7.5
            { ExpeditionLenght.Medium, new int[2] { 10, 20 } }, //ave 15
            { ExpeditionLenght.Long, new int[2]   { 20, 30 } }  //ave 25
        } },
        { ExpeditionGoal.Find_Colony_Locations, new Dictionary<ExpeditionLenght, int[]>() {
            { ExpeditionLenght.Short, new int[2]  { 5, 10 } },  //ave 7.5
            { ExpeditionLenght.Medium, new int[2] { 10, 20 } }, //ave 15
            { ExpeditionLenght.Long, new int[2]   { 20, 30 } }  //ave 25
        } },
        { ExpeditionGoal.Establish_Colony, new Dictionary<ExpeditionLenght, int[]>() {
            { ExpeditionLenght.Short, new int[2]  { 40, 50 } }, //ave 45
            { ExpeditionLenght.Medium, new int[2] { 40, 50 } }, //ave 45
            { ExpeditionLenght.Long, new int[2]   { 40, 50 } }  //ave 45
        } }
    };

    public ExpeditionLenght Lenght { get; private set; }
    public ExpeditionGoal Goal { get; private set; }
    public long Building_Id { get; private set; }
    public Resource Resource { get; private set; }
    public float Time_Remaining { get; private set; }
    public ExpeditionState State { get; private set; }
    public ColonyLocation Colony_Data { get; private set; }

    public Expedition(ExpeditionGoal goal, ExpeditionLenght lenght, long building_id, Resource resource)
    {
        Goal = goal;
        Lenght = lenght;
        Building_Id = building_id;
        Resource = resource;
        Time_Remaining = RNG.Instance.Next(ACTIVE_TIME[goal][lenght][0], ACTIVE_TIME[goal][lenght][1]);
        State = ExpeditionState.Active;
        Colony_Data = null;
    }

    public Expedition(long building_id, ColonyLocation location)
    {
        Goal = ExpeditionGoal.Establish_Colony;
        Lenght = ExpeditionLenght.Long;
        Building_Id = building_id;
        Resource = null;
        Time_Remaining = RNG.Instance.Next(ACTIVE_TIME[Goal][Lenght][0], ACTIVE_TIME[Goal][Lenght][1]);
        State = ExpeditionState.Active;
        Colony_Data = location;
    }

    public Expedition(ExpeditionGoal goal, ExpeditionLenght lenght, long building_id, Resource resource, float time_remaining, ExpeditionState state, ColonyLocation colony_data)
    {
        Goal = goal;
        Lenght = lenght;
        Building_Id = building_id;
        Resource = resource;
        Time_Remaining = time_remaining;
        State = state;
        Colony_Data = colony_data;
    }

    public void Update(float delta_time)
    {
        if(State == ExpeditionState.Finished) {
            return;
        }
        Time_Remaining -= TimeManager.Instance.Seconds_To_Days(delta_time);
        if(Time_Remaining <= 0.0f) {
            if(State == ExpeditionState.Active) {
                State = ExpeditionState.Returning;
                Time_Remaining = RNG.Instance.Next(RETURN_TIME[Goal][Lenght][0], ACTIVE_TIME[Goal][Lenght][1]);
            } else {
                State = ExpeditionState.Finished;
                Time_Remaining = 0.0f;
                Finish();
            }
        }
    }

    public void Cheat_Finish()
    {
        Time_Remaining = 0.1f;
        State = ExpeditionState.Returning;
    }

    private void Finish()
    {
        if(NewExpeditionGUIManager.Instance.Active && NewExpeditionGUIManager.Instance.Expedition == this) {
            NewExpeditionGUIManager.Instance.Active = false;
        }
        City.Instance.Remove_Expedition(this);
        Building harbor = City.Instance.Buildings.FirstOrDefault(x => x.Id == Building_Id);
        if (Goal == ExpeditionGoal.Collect_Resources) {
            if(harbor == null) {
                return;
            }
            int additional_chance = 50;
            List<Resource> found_resources = new List<Resource>() { Resource };
            float amount = Get_Random_Resource_Amount(Resource);
            switch (Lenght) {
                case ExpeditionLenght.Short:
                    additional_chance = 20;
                    break;
                case ExpeditionLenght.Long:
                    additional_chance = 80;
                    break;
            }
            Store_Resources(harbor, Resource, amount);
            while (RNG.Instance.Next(0, 100) <= additional_chance) {
                Resource extra = Get_Random_Resource(Resource, found_resources);
                if(extra == null) {
                    break;
                }
                Store_Resources(harbor, extra, Get_Random_Resource_Amount(extra) * 0.35f);
                found_resources.Add(extra);
                additional_chance /= 2;
            }
        } else if(Goal == ExpeditionGoal.Find_Colony_Locations) {
            City.Instance.Add_Colony_Location(new ColonyLocation(Resource));
        } else if(Goal == ExpeditionGoal.Establish_Colony) {
            Contacts.Instance.Cities.Add(new ForeignCity(Colony_Data));
        }
        if(harbor != null) {
            harbor.Lock_Workers = false;
            Building dock = City.Instance.Buildings.FirstOrDefault(x => x.Id == long.Parse(harbor.Data["dock_id"]));
            string sprite = dock != null ? dock.Sprite.Name : harbor.Sprite.Name;
            SpriteManager.SpriteType sprite_type = dock != null ? dock.Sprite.Type : harbor.Sprite.Type;
            NotificationManager.Instance.Add_Notification(new Notification("Expedition finished", sprite, sprite_type, delegate() {
                CameraManager.Instance.Set_Camera_Location(harbor.Tile.Coordinates.Vector);
                InspectorManager.Instance.Building = harbor;
            }));
        }
    }

    private void Store_Resources(Building harbor, Resource resource, float amount)
    {
        if (!harbor.Output_Storage.ContainsKey(resource)) {
            harbor.Output_Storage.Add(resource, amount);
        } else {
            harbor.Output_Storage[resource] += amount;
        }
    }

    private float Get_Random_Resource_Amount(Resource resource)
    {
        float amount = (100.0f / ((resource.Value + 2.5f) / 5.0f)) * (RNG.Instance.Next(50, 150) / 100.0f);
        switch (Lenght) {
            case ExpeditionLenght.Short:
                amount *= 0.30f;
                break;
            case ExpeditionLenght.Long:
                amount *= 2.75f;
                break;
        }
        return amount;
    }

    private Resource Get_Random_Resource(Resource original, List<Resource> exclude)
    {
        List<Resource.ResourceTag> skip_tags = new List<Resource.ResourceTag>() { Resource.ResourceTag.Basic, Resource.ResourceTag.Non_Tradeable, Resource.ResourceTag.Fine, Resource.ResourceTag.Opulent };
        List<Resource> possible_resources = new List<Resource>();
        foreach(Resource.ResourceTag tag in original.Tags) {
            if (skip_tags.Contains(tag)) {
                continue;
            }
            foreach(Resource resource in Resource.All) {
                if(resource.Tags.Contains(tag) && resource != original && !exclude.Contains(resource)) {
                    possible_resources.Add(resource);
                }
            }
        }
        return possible_resources.Count == 0 ? null : RNG.Instance.Item(possible_resources);
    }

    public void Launch(Building harbor)
    {
        City.Instance.Take_Cash(COSTS[Goal][Lenght]);
        City.Instance.Add_Expedition(this);
        harbor.Lock_Workers = true;
        if(Goal == ExpeditionGoal.Establish_Colony) {
            City.Instance.Remove_Colony_Location(Colony_Data);
        }
    }
}
