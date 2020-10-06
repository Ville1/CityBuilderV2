using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// TODO:This class has RPG-stuff
/// </summary>
public class RNG
{
    public enum Mode { Accuracy, Critical, Debuff }

    private static RNG instance;

    private System.Random generator;

    private RNG()
    {
        generator = new System.Random();
    }

    public static RNG Instance
    {
        get {
            if (instance == null) {
                instance = new RNG();
            }
            return instance;
        }
    }

    public int Next()
    {
        return generator.Next();
    }

    public int Next(int max)
    {
        return generator.Next(max + 1);
    }

    public int Next(int min, int max)
    {
        return generator.Next(min, max + 1);
    }

    public float Next_F()
    {
        return Next(100) * 0.01f;
    }
    
    public T Item<T>(List<T> list)
    {
        return list[Next(list.Count - 1)];
    }

    public List<T> Shuffle<T>(List<T> list)
    {
        if(list.Count <= 1) {
            return list;
        }
        List<T> new_list = new List<T>();
        while(list.Count > 1) {
            T item = Item(list);
            list.Remove(item);
            new_list.Add(item);
        }
        new_list.Add(list[0]);
        return new_list;
    }

    public List<T> Cut<T>(List<T> list, int max)
    {
        if(max == 0) {
            return new List<T>();
        }
        if(list.Count <= max) {
            return Helper.Clone_List(list);
        }
        List<T> new_list = new List<T>();
        while (new_list.Count < max) {
            T item = Item(list);
            if (!new_list.Contains(item)) {
                new_list.Add(item);
            }
        }
        return new_list;
    }
}
