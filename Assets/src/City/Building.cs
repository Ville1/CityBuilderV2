using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building {
    private static long current_id = 0;

    public enum UI_Category { Admin, Infrastructure, Housing, Services, Forestry, Agriculture, Industry }
    public enum Resident { Peasant, Citizen, Noble }

    public long Id { get; private set; }
    public string Name { get; private set; }
    public string Internal_Name { get; private set; }
    public UI_Category Category { get; private set; }
    public string Sprite { get; private set; }
    public bool Is_Prototype { get { return Id < 0; } }

    public Building(Building prototype)
    {
        Id = current_id;
        current_id++;
        Name = prototype.Name;
        Internal_Name = prototype.Internal_Name;
        Category = prototype.Category;
        Sprite = prototype.Sprite;
    }

    public Building(string internal_name, string name, UI_Category category, string sprite)
    {
        Id = -1;
        Name = name;
        Internal_Name = internal_name;
        Category = category;
        Sprite = sprite;
    }
}
