using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class NameManager
{
    private static readonly string FILE_PATH = "/Resources/names/";

    public enum NameType { City }
    private static NameManager instance;
    private List<NameData> names;
    private List<DefaultNameData> default_names;

    private NameManager()
    {
        names = new List<NameData>();
        default_names = new List<DefaultNameData>();

        string city_names_file = Application.dataPath + FILE_PATH + "cities.json";
        try {
            CityNameData city_names = JsonUtility.FromJson<CityNameData>(File.ReadAllText(city_names_file));
            foreach (string name in city_names.City) {
                names.Add(new NameData() {
                    Name = name,
                    Type = NameType.City,
                    Used = false
                });
            }
        } catch (Exception e) {
            CustomLogger.Instance.Warning(string.Format("Failed to load file: {0} Exception: {1}", city_names_file, e.Message));
        }
    }

    public static NameManager Instance
    {
        get {
            if (instance == null) {
                instance = new NameManager();
            }
            return instance;
        }
    }

    public void Reset()
    {
        foreach (NameData name in names) {
            name.Used = false;
        }
        default_names.Clear();
    }

    public string Get_Name(NameType type, bool allow_duplicates)
    {
        List<NameData> valid_names = names.Where(x => x.Type == type && (!x.Used || allow_duplicates)).ToList();
        if (valid_names.Count == 0) {
            DefaultNameData default_name = default_names.FirstOrDefault(x => x.Type == type);
            if (default_name == null) {
                default_name = new DefaultNameData() { Type = type, Count = 0 };
                default_names.Add(default_name);
            } else {
                default_name.Count++;
            }
            return default_name.ToString();
        }
        NameData name = RNG.Instance.Item(valid_names);
        name.Used = true;
        return name.Name;
    }

    private class NameData
    {
        public string Name { get; set; }
        public NameType Type { get; set; }
        public bool Used { get; set; }
    }

    private class DefaultNameData
    {
        public NameType Type { get; set; }
        public int Count { get; set; }

        public override string ToString()
        {
            return string.Format("{0}#{1}", Type.ToString(), Count);
        }
    }

    private class CityNameData
    {
        public List<string> City;
    }
}
