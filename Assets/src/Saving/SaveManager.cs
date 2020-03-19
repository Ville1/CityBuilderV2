using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class SaveManager
{
    public static readonly string DEFAULT_SAVE_LOCATION = "C:\\Users\\Ville\\Documents\\cbsaves\\";

    private static SaveManager instance;

    private SaveData data;
    private string path;

    private SaveManager()
    { }

    public static SaveManager Instance
    {
        get {
            if (instance == null) {
                instance = new SaveManager();
            }
            return instance;
        }
    }

    public bool Start_Saving(string path)
    {
        try {
            this.path = path;
            data = new SaveData();
            data.Days = TimeManager.Instance.Total_Days;
            data.City = City.Instance.Save_Data();
            data.Map = new MapSaveData();
            data.Map.Tiles = new List<TileSaveData>();
            data.Map.Width = Map.Instance.Width;
            data.Map.Height = Map.Instance.Height;
            return true;
        } catch (Exception exception) {
            CustomLogger.Instance.Error(exception.ToString());
            return false;
        }
    }

    public bool Start_Loading(string path)
    {
        try {
            this.path = path;
            data = JsonUtility.FromJson<SaveData>(File.ReadAllText(path));
            return true;
        } catch (Exception exception) {
            CustomLogger.Instance.Error(exception.ToString());
            return false;
        }
    }

    public void Add(TileSaveData tile)
    {
        if (data == null) {
            CustomLogger.Instance.Error("Start_Saving needs to be called before Add");
        } else {
            data.Map.Tiles.Add(tile);
        }
    }

    public void Add(BuildingSaveData building)
    {
        if (data == null) {
            CustomLogger.Instance.Error("Start_Saving needs to be called before Add");
        } else {
            data.City.Buildings.Add(building);
        }
    }

    public TileSaveData Get_Tile(int x, int y)
    {
        if (data == null) {
            CustomLogger.Instance.Error("Start_Loading needs to be called before Get_Tile");
            return null;
        } else {
            return data.Map.Tiles.First(t => t.X == x && t.Y == y);
        }
    }

    public SaveData Data
    {
        get {
            return data;
        }
    }

    public bool Finish_Saving()
    {
        try {
            File.WriteAllText(path, JsonUtility.ToJson(data, true));
            data = null;
            return true;
        } catch (Exception exception) {
            CustomLogger.Instance.Error(exception.ToString());
            data = null;
            return false;
        }
    }

    /*public List<Item> Load_Items()
    {
        if (data == null) {
            CustomLogger.Instance.Error("Start_Loading needs to be called before Load_Items");
            return null;
        } else {
            List<Item> items = new List<Item>();
            foreach (ItemSaveData item_data in data.Player.Items) {
                items.Add(ItemPrototypes.Instance.Is_Tool(item_data.Internal_Name) ? ItemPrototypes.Instance.Get_Tool(item_data.Internal_Name) : ItemPrototypes.Instance.Get_Item(item_data.Internal_Name));
            }
            return items;
        }
    }*/

    public void Finish_Loading()
    {
        data = null;
    }
}
