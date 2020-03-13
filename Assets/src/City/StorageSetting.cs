using System.Collections.Generic;
using System.Linq;

public class StorageSetting {
    public enum StoragePriority { Very_Low, Low, Medium, High, Very_High }

    public Resource Resource { get; set; }
    public int Limit { get; set; }
    public StoragePriority Priority { get; set; }
}

public class StorageSettings {
    public List<StorageSetting> Settings { get; private set; }

    public StorageSettings(Building building)
    {
        Settings = new List<StorageSetting>();
        foreach(Resource allowed in building.Allowed_Resources) {
            Settings.Add(new StorageSetting() {
                Resource = allowed,
                Limit = building.Storage_Limit,
                Priority = StorageSetting.StoragePriority.Medium
            });
        }
    }

    public bool Has(Resource resource)
    {
        return Settings.FirstOrDefault(x => x.Resource == resource) != null;
    }

    public StorageSetting Get(Resource resource)
    {
        return Settings.FirstOrDefault(x => x.Resource == resource);
    }
}