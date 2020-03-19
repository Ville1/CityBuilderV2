using System;
using System.Collections.Generic;

public class Resource {
    public enum ResourceType { Wood, Stone, Lumber, Tools, Roots, Berries, Mushrooms, Herbs, Firewood, Charcoal, Game, Hide, Leather }
    public enum FoodType { None, Meat, Vegetable }

    public static Resource Wood { get { return Get(ResourceType.Wood); } }
    public static Resource Stone { get { return Get(ResourceType.Stone); } }
    public static Resource Lumber { get { return Get(ResourceType.Lumber); } }
    public static Resource Tools { get { return Get(ResourceType.Tools); } }
    public static Resource Roots { get { return Get(ResourceType.Roots); } }
    public static Resource Berries { get { return Get(ResourceType.Berries); } }
    public static Resource Mushrooms { get { return Get(ResourceType.Mushrooms); } }
    public static Resource Herbs { get { return Get(ResourceType.Herbs); } }
    public static Resource Firewood { get { return Get(ResourceType.Firewood); } }
    public static Resource Charcoal { get { return Get(ResourceType.Charcoal); } }
    public static Resource Game { get { return Get(ResourceType.Game); } }
    public static Resource Hide { get { return Get(ResourceType.Hide); } }
    public static Resource Leather { get { return Get(ResourceType.Leather); } }
    private static Dictionary<ResourceType, Resource> resources;

    public int Id { get { return (int)Type; } }
    public ResourceType Type { get; private set; }
    public string UI_Name { get; private set; }
    public string Sprite_Name { get; private set; }
    public SpriteManager.SpriteType Sprite_Type { get; private set; }
    public bool Has_Sprite { get { return !string.IsNullOrEmpty(Sprite_Name); } }
    public FoodType Food_Type { get; private set; }
    public bool Is_Food { get { return Food_Type != FoodType.None; } }
    public float Food_Quality { get; private set; }
    public float Value { get; private set; }
    public float Fuel_Value { get; private set; }
    public bool Is_Fuel { get { return Fuel_Value > 0.0f; } }

    private Resource(ResourceType type, string ui_name, string sprite_name, SpriteManager.SpriteType sprite_type, float value)
    {
        Type = type;
        UI_Name = ui_name;
        Sprite_Name = sprite_name;
        Sprite_Type = sprite_type;
        Food_Type = FoodType.None;
        Food_Quality = 0.0f;
        Value = value;
        Fuel_Value = 0.0f;
    }

    private Resource(ResourceType type, string ui_name, string sprite_name, SpriteManager.SpriteType sprite_type, float value, float fuel_value)
    {
        Type = type;
        UI_Name = ui_name;
        Sprite_Name = sprite_name;
        Sprite_Type = sprite_type;
        Food_Type = FoodType.None;
        Food_Quality = 0.0f;
        Value = value;
        Fuel_Value = fuel_value;
    }

    private Resource(ResourceType type, string ui_name, string sprite_name, SpriteManager.SpriteType sprite_type, FoodType food_type, float food_quality, float value)
    {
        Type = type;
        UI_Name = ui_name;
        Sprite_Name = sprite_name;
        Sprite_Type = sprite_type;
        Food_Type = food_type;
        Food_Quality = food_quality;
        Value = value;
        Fuel_Value = 0.0f;
    }

    public static Resource Get(ResourceType type)
    {
        if(resources == null) {
            resources = new Dictionary<ResourceType, Resource>();
            resources.Add(ResourceType.Wood, new Resource(ResourceType.Wood, "Wood", "wood", SpriteManager.SpriteType.UI, 0.5f));
            resources.Add(ResourceType.Stone, new Resource(ResourceType.Stone, "Stone", "stone", SpriteManager.SpriteType.UI, 0.55f));
            resources.Add(ResourceType.Lumber, new Resource(ResourceType.Lumber, "Lumber", "lumber", SpriteManager.SpriteType.UI, 1.0f));
            resources.Add(ResourceType.Tools, new Resource(ResourceType.Tools, "Tools", "tools", SpriteManager.SpriteType.UI, 3.0f));
            resources.Add(ResourceType.Roots, new Resource(ResourceType.Roots, "Roots", null, SpriteManager.SpriteType.UI, FoodType.Vegetable, 0.25f, 0.75f));
            resources.Add(ResourceType.Berries, new Resource(ResourceType.Berries, "Berries", null, SpriteManager.SpriteType.UI, FoodType.Vegetable, 0.75f, 1.25f));
            resources.Add(ResourceType.Mushrooms, new Resource(ResourceType.Mushrooms, "Mushrooms", null, SpriteManager.SpriteType.UI, FoodType.Vegetable, 0.55f, 0.95f));
            resources.Add(ResourceType.Herbs, new Resource(ResourceType.Herbs, "Herbs", null, SpriteManager.SpriteType.UI, FoodType.Vegetable, 1.25f, 5.0f));
            resources.Add(ResourceType.Firewood, new Resource(ResourceType.Firewood, "Firewood", null, SpriteManager.SpriteType.UI, 0.75f, 1.0f));
            resources.Add(ResourceType.Charcoal, new Resource(ResourceType.Charcoal, "Charcoal", null, SpriteManager.SpriteType.UI, 1.0f, 10.0f));
            resources.Add(ResourceType.Game, new Resource(ResourceType.Game, "Game", null, SpriteManager.SpriteType.UI, FoodType.Meat, 1.05f, 1.15f));
            resources.Add(ResourceType.Hide, new Resource(ResourceType.Hide, "Hide", null, SpriteManager.SpriteType.UI, 0.25f));
            resources.Add(ResourceType.Leather, new Resource(ResourceType.Leather, "Leather", null, SpriteManager.SpriteType.UI, 1.00f));
        }
        return resources[type];
    }
    
    public static List<Resource> All
    {
        get {
            List<Resource> list = new List<Resource>();
            foreach(ResourceType type in Enum.GetValues(typeof(ResourceType))) {
                list.Add(Get(type));
            }
            return list;
        }
    }

    public override string ToString()
    {
        return Type.ToString();
    }
}
