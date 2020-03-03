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
        return generator.Next(max);
    }

    public int Next(int min, int max)
    {
        return generator.Next(min, max);
    }

    /// <summary>
    /// TODO: use differenct generator? Seed should propably not affect this
    /// TODO: mode for debuff resist?
    /// </summary>
    /// <param name="success_weight"></param>
    /// <param name="failure_weight"></param>
    /// <param name="mode"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    public bool Roll(float success_weight, float failure_weight, Mode mode, out string message, float mod_flat = 0.0f, float mod_mult = 1.0f)
    {
        switch (mode) {
            case Mode.Critical:
                return Roll_Critical(success_weight, failure_weight, out message, mod_flat, mod_mult);
            case Mode.Accuracy:
                return Roll_Accuracy(success_weight, failure_weight, out message, mod_flat, mod_mult);
            case Mode.Debuff:
                return Roll_Debuff(success_weight, failure_weight, out message, mod_flat, mod_mult);
        }
        CustomLogger.Instance.Error("Mode not implented: " + mode.ToString());
        message = "ERROR";
        return false;
    }

    public bool Roll_Critical(float success_weight, float failure_weight, out string message, float mod_flat = 0.0f, float mod_mult = 1.0f)
    {
        float final_success_weight = success_weight / (success_weight + failure_weight);
        //float final_failure_weight = 1.0f - final_success_weight;
        float chance = final_success_weight >= 0.5f ?
            (((final_success_weight * final_success_weight) / 2.5f) - 0.05f) :
            final_success_weight / 10.0f;
        if (chance < 0.01f) {
            chance = 0.01f;
        }
        return Roll(Mathf.RoundToInt(((chance * mod_mult) + mod_flat) * 100.0f), out message);
    }

    public bool Roll_Accuracy(float success_weight, float failure_weight, out string message, float mod_flat = 0.0f, float mod_mult = 1.0f)
    {
        float final_success_weight = success_weight / (success_weight + failure_weight);
        //float final_failure_weight = 1.0f - final_success_weight;
        float chance = ((final_success_weight * 2.0f) + 1.0f) / 2.0f;
        return Roll(Mathf.RoundToInt(((chance * mod_mult) + mod_flat) * 100.0f), out message);
    }

    public bool Roll_Debuff(float success_weight, float failure_weight, out string message, float mod_flat = 0.0f, float mod_mult = 1.0f)
    {
        float final_success_weight = success_weight / (success_weight + failure_weight);
        //float final_failure_weight = 1.0f - final_success_weight;
        float chance = final_success_weight > 0.5f ?
            (((final_success_weight - 0.5f) * 2.0f) + 0.5f) :
            final_success_weight;
        if (chance < 0.01f) {
            chance = 0.01f;
        }
        if (chance > 1.00f) {
            chance = 1.00f;
        }
        return Roll(Mathf.RoundToInt(((chance * mod_mult) + mod_flat) * 100.0f), out message);
    }

    public bool Roll(int chance_percent, out string message)
    {
        if (chance_percent > 100) {
            chance_percent = 100;
        } else if (chance_percent < 0) {
            chance_percent = 0;
        }
        int roll = Next(101);
        bool result = roll <= chance_percent;
        message = string.Format("{0} / {1} -> {2}", roll, chance_percent, result ? "success" : "failure");
        return result;
    }

    public string String(int lenght)
    {
        if (lenght <= 0) {
            lenght = 1;
        }
        string chars = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        StringBuilder result = new StringBuilder();
        while (result.Length < lenght) {
            result.Append(chars[Next(chars.Length)]);
        }
        return result.ToString();
    }

    public T Item<T>(List<T> list)
    {
        return list[Next(list.Count)];
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
}
