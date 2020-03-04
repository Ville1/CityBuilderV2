using System.Collections.Generic;

public class City {
    private static City instance;

    public bool Has_Town_Hall { get; private set; }
    
    public List<Building> Buildings { get; private set; }

    private City()
    {

    }

    public void Start_New()
    {
        Has_Town_Hall = false;
        Buildings = new List<Building>();
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
            Buildings.Add(new Building(prototype, tile, false));
            return;
        }
    }
}
