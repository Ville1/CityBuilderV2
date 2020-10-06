using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ColonyLocation {
    public string Name { get; set; }
    public List<Resource> Preferred_Imports { get; private set; }
    public List<Resource> Disliked_Imports { get; private set; }
    public List<Resource> Unaccepted_Imports { get; private set; }
    public List<Resource> Exports { get; private set; }
    public List<Resource> Cheap_Exports { get; private set; }
    public List<Resource> Expensive_Exports { get; private set; }
    public ForeignCity.TradeRouteType Trade_Route_Type { get; private set; }

    public ColonyLocation(Resource desired_resource)
    {
        Name = "???";
        Preferred_Imports = new List<Resource>();
        Disliked_Imports = new List<Resource>();
        Unaccepted_Imports = new List<Resource>();
        Exports = new List<Resource>();
        Cheap_Exports = new List<Resource>();
        Expensive_Exports = new List<Resource>();
        List<Resource> added_resources = new List<Resource>() { desired_resource };

        //Imports
        //Preferred
        List<Resource> possible_preferred_imports = new List<Resource>();
        foreach(Resource resource in Resource.All.Where(x => !x.Tags.Contains(Resource.ResourceTag.Non_Tradeable)).ToList()) {
            if (resource.Type != desired_resource.Type) {
                if (resource.Tags.Contains(Resource.ResourceTag.Construction) && (RNG.Instance.Next(0, 100) < 20)) {
                    possible_preferred_imports.Add(resource);
                } else if (resource.Tags.Contains(Resource.ResourceTag.Industrial) && (RNG.Instance.Next(0, 100) < 5)) {
                    possible_preferred_imports.Add(resource);
                }
            }
        }
        Preferred_Imports = RNG.Instance.Cut(possible_preferred_imports, RNG.Instance.Next(2, 4));
        added_resources.AddRange(Preferred_Imports);

        //TODO: Duplicated code
        //Disliked
        List<Resource> possible_disliked_imports = new List<Resource>();
        foreach (Resource resource in Resource.All.Where(x => !x.Tags.Contains(Resource.ResourceTag.Non_Tradeable)).ToList()) {
            if(resource.Tags.Exists(x => desired_resource.Tags.Contains(x)) && !added_resources.Contains(resource)) {
                if (resource.Tags.Contains(Resource.ResourceTag.Basic) && (RNG.Instance.Next(0, 100) < 20)) {
                    possible_disliked_imports.Add(resource);
                } else if (RNG.Instance.Next(0, 100) < 5) {
                    possible_disliked_imports.Add(resource);
                }
            }
        }
        Disliked_Imports = RNG.Instance.Cut(possible_disliked_imports, RNG.Instance.Next(1, 3));
        Disliked_Imports.Add(desired_resource);
        added_resources.AddRange(Disliked_Imports);

        //Exports
        //Cheap
        List<Resource> possible_cheap_exports = new List<Resource>();
        foreach (Resource resource in Resource.All.Where(x => !x.Tags.Contains(Resource.ResourceTag.Non_Tradeable)).ToList()) {
            if (resource.Tags.Exists(x => desired_resource.Tags.Contains(x)) && !added_resources.Contains(resource)) {
                if (resource.Tags.Contains(Resource.ResourceTag.Basic) && (RNG.Instance.Next(0, 100) < 25)) {
                    possible_cheap_exports.Add(resource);
                } else if (RNG.Instance.Next(0, 100) < 10) {
                    possible_cheap_exports.Add(resource);
                }
            }
        }
        Cheap_Exports = RNG.Instance.Cut(possible_cheap_exports, RNG.Instance.Next(1, 3));
        Cheap_Exports.Add(desired_resource);
        added_resources.AddRange(Cheap_Exports);

        //Normal
        List<Resource> possible_normal_exports = new List<Resource>();
        foreach (Resource resource in Resource.All.Where(x => !x.Tags.Contains(Resource.ResourceTag.Non_Tradeable)).ToList()) {
            if (resource.Tags.Exists(x => desired_resource.Tags.Contains(x)) && !added_resources.Contains(resource)) {
                if (resource.Tags.Contains(Resource.ResourceTag.Basic) && (RNG.Instance.Next(0, 100) < 30)) {
                    possible_normal_exports.Add(resource);
                } else if (RNG.Instance.Next(0, 100) < 20) {
                    possible_normal_exports.Add(resource);
                }
            }
        }
        Exports = RNG.Instance.Cut(possible_normal_exports, RNG.Instance.Next(1, 2));
        added_resources.AddRange(Exports);

        //Expensive
        List<Resource> possible_expensive_exports = new List<Resource>();
        foreach (Resource resource in Resource.All.Where(x => !x.Tags.Contains(Resource.ResourceTag.Non_Tradeable)).ToList()) {
            if (resource.Tags.Exists(x => desired_resource.Tags.Contains(x)) && !added_resources.Contains(resource)) {
                if (resource.Tags.Contains(Resource.ResourceTag.Basic) && (RNG.Instance.Next(0, 100) < 15)) {
                    possible_expensive_exports.Add(resource);
                } else if (RNG.Instance.Next(0, 100) < 10) {
                    possible_expensive_exports.Add(resource);
                }
            }
        }
        Expensive_Exports = RNG.Instance.Cut(possible_expensive_exports, RNG.Instance.Next(0, 1));
        added_resources.AddRange(Expensive_Exports);

        //Trade route
        int both_chance = 25;
        if (desired_resource.Tags.Contains(Resource.ResourceTag.Coastal) || desired_resource.Tags.Contains(Resource.ResourceTag.Exotic)) {
            both_chance -= 20;
        } else if (desired_resource.Tags.Contains(Resource.ResourceTag.Alcohol)) {
            both_chance -= 5;
        }
        if (desired_resource.Tags.Contains(Resource.ResourceTag.Livestock) || desired_resource.Tags.Contains(Resource.ResourceTag.Mining) || desired_resource.Tags.Contains(Resource.ResourceTag.Hunting)) {
            both_chance += 15;
        } else if (desired_resource.Tags.Contains(Resource.ResourceTag.Foraging) || desired_resource.Tags.Contains(Resource.ResourceTag.Agricultural) || desired_resource.Tags.Contains(Resource.ResourceTag.Crop)) {
            both_chance += 10;
        } else if (desired_resource.Tags.Contains(Resource.ResourceTag.Forestry)) {
            both_chance += 5;
        }
        Trade_Route_Type = RNG.Instance.Next(0, 100) < Mathf.Clamp(both_chance, 1, 50) ? ForeignCity.TradeRouteType.Both : ForeignCity.TradeRouteType.Water;
    }

    public ColonyLocation(ColonyLocationSaveData save_data)
    {
        Name = save_data.Name;
        Preferred_Imports = save_data.Preferred_Imports.Select(x => Resource.All.First(y => (int)y.Type == x)).ToList();
        Disliked_Imports = save_data.Disliked_Imports.Select(x => Resource.All.First(y => (int)y.Type == x)).ToList();
        Unaccepted_Imports = save_data.Unaccepted_Imports.Select(x => Resource.All.First(y => (int)y.Type == x)).ToList();
        Exports = save_data.Exports.Select(x => Resource.All.First(y => (int)y.Type == x)).ToList();
        Cheap_Exports = save_data.Cheap_Exports.Select(x => Resource.All.First(y => (int)y.Type == x)).ToList();
        Expensive_Exports = save_data.Expensive_Exports.Select(x => Resource.All.First(y => (int)y.Type == x)).ToList();
        Trade_Route_Type = (ForeignCity.TradeRouteType)save_data.Trade_Route_Type;
    }

    public ColonyLocationSaveData Save_Data
    {
        get {
            ColonyLocationSaveData data = new ColonyLocationSaveData();
            data.Name = Name;
            data.Preferred_Imports = Preferred_Imports.Select(x => (int)x.Type).ToList();
            data.Disliked_Imports = Disliked_Imports.Select(x => (int)x.Type).ToList();
            data.Unaccepted_Imports = Unaccepted_Imports.Select(x => (int)x.Type).ToList();
            data.Exports = Exports.Select(x => (int)x.Type).ToList();
            data.Cheap_Exports = Cheap_Exports.Select(x => (int)x.Type).ToList();
            data.Expensive_Exports = Expensive_Exports.Select(x => (int)x.Type).ToList();
            data.Trade_Route_Type = (int)Trade_Route_Type;
            return data;
        }
    }
}
