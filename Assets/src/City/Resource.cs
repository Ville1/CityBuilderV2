﻿using System;
using System.Collections.Generic;

public class Resource {
    public enum ResourceType { Wood, Stone, Lumber, Tools, Roots, Berries, Mushrooms, Herbs, Firewood, Charcoal, Game, Hide, Leather, Potatoes, Corn, Bread, Iron_Ore, Coal, Salt, Iron_Bars, Ale, Wool, Cloth, Thread, Mutton,
        Barrels, Simple_Clothes, Leather_Clothes, Wheat, Flour, Mechanisms, Clay, Bricks, Fish, Marble, Bananas, Oranges, Beer, Rum, Wine, Coffee, Pretzels, Cakes, Copper_Ore, Copper_Bars, Tin_Ore, Tin_Bars, Pewter_Bars, Pewterware }
    public enum FoodType { None, Meat, Vegetable, Delicacy }
    public enum ResourceTag { Agricultural, Industrial, Forestry, Archaic, Coastal, Mining, Opulent, Foraging, Hunting, Construction, Livestock, Food, Crop, Clothing, Fine, Exotic, Alcohol, Pastry }
    public enum ResourceRarity { Very_Rare, Rare, Uncommon, Common }

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
    public static Resource Potatoes { get { return Get(ResourceType.Potatoes); } }
    public static Resource Corn { get { return Get(ResourceType.Corn); } }
    public static Resource Bread { get { return Get(ResourceType.Bread); } }
    public static Resource Iron_Ore { get { return Get(ResourceType.Iron_Ore); } }
    public static Resource Coal { get { return Get(ResourceType.Coal); } }
    public static Resource Salt { get { return Get(ResourceType.Salt); } }
    public static Resource Iron_Bars { get { return Get(ResourceType.Iron_Bars); } }
    public static Resource Ale { get { return Get(ResourceType.Ale); } }
    public static Resource Wool { get { return Get(ResourceType.Wool); } }
    public static Resource Mutton { get { return Get(ResourceType.Mutton); } }
    public static Resource Cloth { get { return Get(ResourceType.Cloth); } }
    public static Resource Thread { get { return Get(ResourceType.Thread); } }
    public static Resource Barrels { get { return Get(ResourceType.Barrels); } }
    public static Resource Simple_Clothes { get { return Get(ResourceType.Simple_Clothes); } }
    public static Resource Leather_Clothes { get { return Get(ResourceType.Leather_Clothes); } }
    public static Resource Wheat { get { return Get(ResourceType.Wheat); } }
    public static Resource Flour { get { return Get(ResourceType.Flour); } }
    public static Resource Mechanisms { get { return Get(ResourceType.Mechanisms); } }
    public static Resource Clay { get { return Get(ResourceType.Clay); } }
    public static Resource Bricks { get { return Get(ResourceType.Bricks); } }
    public static Resource Fish { get { return Get(ResourceType.Fish); } }
    public static Resource Marble { get { return Get(ResourceType.Marble); } }
    public static Resource Bananas { get { return Get(ResourceType.Bananas); } }
    public static Resource Oranges { get { return Get(ResourceType.Oranges); } }
    public static Resource Beer { get { return Get(ResourceType.Beer); } }
    public static Resource Rum { get { return Get(ResourceType.Rum); } }
    public static Resource Wine { get { return Get(ResourceType.Wine); } }
    public static Resource Coffee { get { return Get(ResourceType.Coffee); } }
    public static Resource Pretzels { get { return Get(ResourceType.Pretzels); } }
    public static Resource Cake { get { return Get(ResourceType.Cakes); } }
    public static Resource Copper_Ore { get { return Get(ResourceType.Tin_Ore); } }
    public static Resource Copper_Bars { get { return Get(ResourceType.Tin_Bars); } }
    public static Resource Tin_Ore { get { return Get(ResourceType.Tin_Ore); } }
    public static Resource Tin_Bars { get { return Get(ResourceType.Tin_Bars); } }
    public static Resource Pewter_Bars { get { return Get(ResourceType.Pewter_Bars); } }
    public static Resource Pewterware { get { return Get(ResourceType.Pewterware); } }
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
    public List<ResourceTag> Tags { get; private set; }
    public ResourceRarity Rarity { get; private set; }

    private Resource(ResourceType type, string ui_name, string sprite_name, SpriteManager.SpriteType sprite_type, float value, ResourceRarity rarity, List<ResourceTag> tags)
    {
        Type = type;
        UI_Name = ui_name;
        Sprite_Name = sprite_name;
        Sprite_Type = sprite_type;
        Food_Type = FoodType.None;
        Food_Quality = 0.0f;
        Value = value;
        Fuel_Value = 0.0f;
        Rarity = rarity;
        Tags = tags;
    }

    private Resource(ResourceType type, string ui_name, string sprite_name, SpriteManager.SpriteType sprite_type, float value, float fuel_value, ResourceRarity rarity, List<ResourceTag> tags)
    {
        Type = type;
        UI_Name = ui_name;
        Sprite_Name = sprite_name;
        Sprite_Type = sprite_type;
        Food_Type = FoodType.None;
        Food_Quality = 0.0f;
        Value = value;
        Fuel_Value = fuel_value;
        Rarity = rarity;
        Tags = tags;
    }

    private Resource(ResourceType type, string ui_name, string sprite_name, SpriteManager.SpriteType sprite_type, FoodType food_type, float food_quality, float value, ResourceRarity rarity, List<ResourceTag> tags)
    {
        Type = type;
        UI_Name = ui_name;
        Sprite_Name = sprite_name;
        Sprite_Type = sprite_type;
        Food_Type = food_type;
        Food_Quality = food_quality;
        Value = value;
        Fuel_Value = 0.0f;
        Rarity = rarity;
        Tags = tags;
    }

    public static Resource Get(ResourceType type)
    {
        if(resources == null) {
            resources = new Dictionary<ResourceType, Resource>();
            resources.Add(ResourceType.Wood, new Resource(ResourceType.Wood, "Wood", "wood", SpriteManager.SpriteType.UI, 0.5f, ResourceRarity.Common, new List<ResourceTag>() { ResourceTag.Forestry, ResourceTag.Archaic, ResourceTag.Construction }));
            resources.Add(ResourceType.Stone, new Resource(ResourceType.Stone, "Stone", "stone", SpriteManager.SpriteType.UI, 0.55f, ResourceRarity.Common, new List<ResourceTag>() { ResourceTag.Archaic, ResourceTag.Construction }));
            resources.Add(ResourceType.Lumber, new Resource(ResourceType.Lumber, "Lumber", "lumber", SpriteManager.SpriteType.UI, 1.0f, ResourceRarity.Uncommon, new List<ResourceTag>() { ResourceTag.Forestry, ResourceTag.Construction }));
            resources.Add(ResourceType.Bricks, new Resource(ResourceType.Bricks, "Bricks", "bricks", SpriteManager.SpriteType.UI, 0.75f, ResourceRarity.Rare, new List<ResourceTag>() { ResourceTag.Industrial, ResourceTag.Construction }));
            resources.Add(ResourceType.Tools, new Resource(ResourceType.Tools, "Tools", "tools", SpriteManager.SpriteType.UI, 3.0f, ResourceRarity.Rare, new List<ResourceTag>() { ResourceTag.Industrial, ResourceTag.Construction, ResourceTag.Fine }));

            resources.Add(ResourceType.Firewood, new Resource(ResourceType.Firewood, "Firewood", "firewood", SpriteManager.SpriteType.UI, 0.50f, 1.0f, ResourceRarity.Common, new List<ResourceTag>() { ResourceTag.Archaic }));
            resources.Add(ResourceType.Charcoal, new Resource(ResourceType.Charcoal, "Charcoal", null, SpriteManager.SpriteType.UI, 1.0f, 10.0f, ResourceRarity.Uncommon, new List<ResourceTag>() { ResourceTag.Forestry, ResourceTag.Industrial }));
            resources.Add(ResourceType.Coal, new Resource(ResourceType.Coal, "Coal", null, SpriteManager.SpriteType.UI, 1.0f, 10.0f, ResourceRarity.Uncommon, new List<ResourceTag>() { ResourceTag.Mining, ResourceTag.Industrial }));

            resources.Add(ResourceType.Hide, new Resource(ResourceType.Hide, "Hide", "hides", SpriteManager.SpriteType.UI, 0.25f, ResourceRarity.Common, new List<ResourceTag>() { ResourceTag.Hunting, ResourceTag.Archaic }));
            resources.Add(ResourceType.Leather, new Resource(ResourceType.Leather, "Leather", "leather", SpriteManager.SpriteType.UI, 1.00f, ResourceRarity.Uncommon, new List<ResourceTag>() { ResourceTag.Hunting, ResourceTag.Archaic, ResourceTag.Livestock }));

            resources.Add(ResourceType.Iron_Ore, new Resource(ResourceType.Iron_Ore, "Iron Ore", "iron_ore", SpriteManager.SpriteType.UI, 1.0f, ResourceRarity.Uncommon, new List<ResourceTag>() { ResourceTag.Mining }));
            resources.Add(ResourceType.Iron_Bars, new Resource(ResourceType.Iron_Bars, "Iron Bars", "iron_bars", SpriteManager.SpriteType.UI, 2.5f, ResourceRarity.Rare, new List<ResourceTag>() { ResourceTag.Mining, ResourceTag.Industrial }));
            resources.Add(ResourceType.Copper_Ore, new Resource(ResourceType.Copper_Ore, "Copper Ore", null, SpriteManager.SpriteType.UI, 0.75f, ResourceRarity.Uncommon, new List<ResourceTag>() { ResourceTag.Mining }));
            resources.Add(ResourceType.Copper_Bars, new Resource(ResourceType.Copper_Bars, "Copper Bars", null, SpriteManager.SpriteType.UI, 1.85f, ResourceRarity.Rare, new List<ResourceTag>() { ResourceTag.Mining, ResourceTag.Industrial }));
            resources.Add(ResourceType.Tin_Ore, new Resource(ResourceType.Tin_Ore, "Tin Ore", null, SpriteManager.SpriteType.UI, 0.75f, ResourceRarity.Uncommon, new List<ResourceTag>() { ResourceTag.Mining }));
            resources.Add(ResourceType.Tin_Bars, new Resource(ResourceType.Tin_Bars, "Tin Bars", null, SpriteManager.SpriteType.UI, 1.85f, ResourceRarity.Rare, new List<ResourceTag>() { ResourceTag.Mining, ResourceTag.Industrial }));
            resources.Add(ResourceType.Pewter_Bars, new Resource(ResourceType.Pewter_Bars, "Pewter Bars", null, SpriteManager.SpriteType.UI, 3.00f, ResourceRarity.Very_Rare, new List<ResourceTag>() { ResourceTag.Mining, ResourceTag.Industrial, ResourceTag.Fine }));

            resources.Add(ResourceType.Herbs, new Resource(ResourceType.Herbs, "Herbs", null, SpriteManager.SpriteType.UI, 5.0f, ResourceRarity.Rare, new List<ResourceTag>() { ResourceTag.Foraging }));
            resources.Add(ResourceType.Salt, new Resource(ResourceType.Salt, "Salt", null, SpriteManager.SpriteType.UI, 0.75f, ResourceRarity.Uncommon, new List<ResourceTag>() { ResourceTag.Coastal, ResourceTag.Mining }));
            resources.Add(ResourceType.Ale, new Resource(ResourceType.Ale, "Ale", null, SpriteManager.SpriteType.UI, 1.25f, ResourceRarity.Uncommon, new List<ResourceTag>() { ResourceTag.Agricultural, ResourceTag.Fine, ResourceTag.Alcohol }));
            resources.Add(ResourceType.Beer, new Resource(ResourceType.Beer, "Beer", null, SpriteManager.SpriteType.UI, 1.25f, ResourceRarity.Uncommon, new List<ResourceTag>() { ResourceTag.Agricultural, ResourceTag.Fine, ResourceTag.Alcohol }));
            resources.Add(ResourceType.Rum, new Resource(ResourceType.Rum, "Rum", null, SpriteManager.SpriteType.UI, 1.35f, ResourceRarity.Rare, new List<ResourceTag>() { ResourceTag.Exotic, ResourceTag.Fine, ResourceTag.Alcohol }));
            resources.Add(ResourceType.Wine, new Resource(ResourceType.Wine, "Wine", null, SpriteManager.SpriteType.UI, 3.50f, ResourceRarity.Very_Rare, new List<ResourceTag>() { ResourceTag.Agricultural, ResourceTag.Opulent, ResourceTag.Alcohol }));
            resources.Add(ResourceType.Coffee, new Resource(ResourceType.Coffee, "Coffee", null, SpriteManager.SpriteType.UI, 2.75f, ResourceRarity.Very_Rare, new List<ResourceTag>() { ResourceTag.Agricultural, ResourceTag.Crop, ResourceTag.Opulent, ResourceTag.Exotic }));
            resources.Add(ResourceType.Pewterware, new Resource(ResourceType.Pewterware, "Pewterware", null, SpriteManager.SpriteType.UI, 3.10f, ResourceRarity.Very_Rare, new List<ResourceTag>() { ResourceTag.Industrial, ResourceTag.Fine }));

            resources.Add(ResourceType.Wool, new Resource(ResourceType.Wool, "Wool", null, SpriteManager.SpriteType.UI, 0.50f, ResourceRarity.Common, new List<ResourceTag>() { ResourceTag.Agricultural, ResourceTag.Archaic, ResourceTag.Livestock }));
            resources.Add(ResourceType.Thread, new Resource(ResourceType.Thread, "Thread", null, SpriteManager.SpriteType.UI, 0.75f, ResourceRarity.Rare, new List<ResourceTag>() { ResourceTag.Agricultural, ResourceTag.Archaic, ResourceTag.Livestock }));
            resources.Add(ResourceType.Cloth, new Resource(ResourceType.Cloth, "Cloth", null, SpriteManager.SpriteType.UI, 1.00f, ResourceRarity.Rare, new List<ResourceTag>() { ResourceTag.Agricultural, ResourceTag.Livestock }));
            resources.Add(ResourceType.Barrels, new Resource(ResourceType.Barrels, "Barrels", null, SpriteManager.SpriteType.UI, 1.00f, ResourceRarity.Uncommon, new List<ResourceTag>() { ResourceTag.Forestry, ResourceTag.Coastal, ResourceTag.Agricultural }));
            resources.Add(ResourceType.Simple_Clothes, new Resource(ResourceType.Simple_Clothes, "Simple Clothes", null, SpriteManager.SpriteType.UI, 5.00f, ResourceRarity.Rare, new List<ResourceTag>() { ResourceTag.Clothing }));
            resources.Add(ResourceType.Leather_Clothes, new Resource(ResourceType.Leather_Clothes, "Leather Clothes", null, SpriteManager.SpriteType.UI, 7.50f, ResourceRarity.Very_Rare, new List<ResourceTag>() { ResourceTag.Clothing, ResourceTag.Fine }));
            resources.Add(ResourceType.Wheat, new Resource(ResourceType.Wheat, "Wheat", null, SpriteManager.SpriteType.UI, 0.50f, ResourceRarity.Common, new List<ResourceTag>() { ResourceTag.Agricultural, ResourceTag.Food, ResourceTag.Crop }));
            resources.Add(ResourceType.Flour, new Resource(ResourceType.Flour, "Flour", null, SpriteManager.SpriteType.UI, 0.60f, ResourceRarity.Uncommon, new List<ResourceTag>() { ResourceTag.Agricultural, ResourceTag.Food }));
            resources.Add(ResourceType.Mechanisms, new Resource(ResourceType.Mechanisms, "Mechanisms", "mechanisms", SpriteManager.SpriteType.UI, 5.00f, ResourceRarity.Very_Rare, new List<ResourceTag>() { ResourceTag.Industrial, ResourceTag.Fine }));
            resources.Add(ResourceType.Clay, new Resource(ResourceType.Clay, "Clay", "clay", SpriteManager.SpriteType.UI, 0.10f, ResourceRarity.Common, new List<ResourceTag>() { ResourceTag.Archaic, ResourceTag.Coastal }));
            resources.Add(ResourceType.Marble, new Resource(ResourceType.Marble, "Marble", null, SpriteManager.SpriteType.UI, 2.50f, ResourceRarity.Rare, new List<ResourceTag>() { ResourceTag.Mining, ResourceTag.Opulent, ResourceTag.Construction, ResourceTag.Fine }));

            resources.Add(ResourceType.Game, new Resource(ResourceType.Game, "Game", null, SpriteManager.SpriteType.UI, FoodType.Meat, 1.05f, 1.10f, ResourceRarity.Common, new List<ResourceTag>() { ResourceTag.Hunting, ResourceTag.Archaic, ResourceTag.Food }));
            resources.Add(ResourceType.Roots, new Resource(ResourceType.Roots, "Roots", "roots", SpriteManager.SpriteType.UI, FoodType.Vegetable, 0.15f, 0.65f, ResourceRarity.Common, new List<ResourceTag>() { ResourceTag.Foraging, ResourceTag.Archaic, ResourceTag.Food }));
            resources.Add(ResourceType.Berries, new Resource(ResourceType.Berries, "Berries", "berries", SpriteManager.SpriteType.UI, FoodType.Vegetable, 0.75f, 1.05f, ResourceRarity.Common, new List<ResourceTag>() { ResourceTag.Foraging, ResourceTag.Archaic, ResourceTag.Food }));
            resources.Add(ResourceType.Mushrooms, new Resource(ResourceType.Mushrooms, "Mushrooms", "mushrooms", SpriteManager.SpriteType.UI, FoodType.Vegetable, 0.30f, 0.95f, ResourceRarity.Common, new List<ResourceTag>() { ResourceTag.Foraging, ResourceTag.Archaic, ResourceTag.Food }));
            resources.Add(ResourceType.Potatoes, new Resource(ResourceType.Potatoes, "Potatoes", null, SpriteManager.SpriteType.UI, FoodType.Vegetable, 0.70f, 0.85f, ResourceRarity.Common, new List<ResourceTag>() { ResourceTag.Agricultural, ResourceTag.Archaic, ResourceTag.Food, ResourceTag.Crop }));
            resources.Add(ResourceType.Corn, new Resource(ResourceType.Corn, "Corn", null, SpriteManager.SpriteType.UI, FoodType.Vegetable, 0.75f, 0.75f, ResourceRarity.Uncommon, new List<ResourceTag>() { ResourceTag.Agricultural, ResourceTag.Food, ResourceTag.Crop }));
            resources.Add(ResourceType.Bread, new Resource(ResourceType.Bread, "Bread", null, SpriteManager.SpriteType.UI, FoodType.Vegetable, 1.10f, 1.0f, ResourceRarity.Rare, new List<ResourceTag>() { ResourceTag.Agricultural, ResourceTag.Food, ResourceTag.Fine }));
            resources.Add(ResourceType.Mutton, new Resource(ResourceType.Mutton, "Mutton", null, SpriteManager.SpriteType.UI, FoodType.Meat, 1.00f, 1.0f, ResourceRarity.Uncommon, new List<ResourceTag>() { ResourceTag.Agricultural, ResourceTag.Archaic, ResourceTag.Food, ResourceTag.Livestock }));
            resources.Add(ResourceType.Fish, new Resource(ResourceType.Fish, "Fish", "fish", SpriteManager.SpriteType.UI, FoodType.Meat, 1.00f, 0.75f, ResourceRarity.Common, new List<ResourceTag>() { ResourceTag.Coastal, ResourceTag.Food }));
            resources.Add(ResourceType.Bananas, new Resource(ResourceType.Bananas, "Bananas", null, SpriteManager.SpriteType.UI, FoodType.Vegetable, 0.85f, 1.10f, ResourceRarity.Rare, new List<ResourceTag>() { ResourceTag.Exotic, ResourceTag.Food, ResourceTag.Crop }));
            resources.Add(ResourceType.Oranges, new Resource(ResourceType.Oranges, "Oranges", null, SpriteManager.SpriteType.UI, FoodType.Vegetable, 0.90f, 1.15f, ResourceRarity.Rare, new List<ResourceTag>() { ResourceTag.Exotic, ResourceTag.Food, ResourceTag.Crop }));
            resources.Add(ResourceType.Pretzels, new Resource(ResourceType.Pretzels, "Pretzels", null, SpriteManager.SpriteType.UI, FoodType.Delicacy, 1.0f, 1.35f, ResourceRarity.Very_Rare, new List<ResourceTag>() { ResourceTag.Food, ResourceTag.Fine, ResourceTag.Pastry }));
            resources.Add(ResourceType.Cakes, new Resource(ResourceType.Cakes, "Cakes", null, SpriteManager.SpriteType.UI, FoodType.Delicacy, 1.0f, 5.00f, ResourceRarity.Very_Rare, new List<ResourceTag>() { ResourceTag.Food, ResourceTag.Opulent, ResourceTag.Pastry }));
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
