using System;
using System.Collections.Generic;
using System.Text;

public class Helper {
    public static bool Is_Diagonal(Coordinates.Direction direction)
    {
        return direction == Coordinates.Direction.North_East || direction == Coordinates.Direction.North_West
            || direction == Coordinates.Direction.South_East || direction == Coordinates.Direction.South_West;
    }

    public static string Float_To_String(float f, int digits, bool show_plus_sign = false, bool show_zeros = true)
    {
        double rounded = Math.Round(double.Parse(f.ToString()), digits);
        if (!show_zeros && rounded == 0.0d) {
            return string.Empty;
        }
        if (f < 0.0f) {
            return rounded.ToString();
        }
        string rounded_s = rounded.ToString();
        if (show_zeros && digits > 0) {
            StringBuilder builder = new StringBuilder(rounded_s);
            int current_digits = 0;
            if (!rounded_s.Contains(".")) {
                builder.Append(".0");
                current_digits = 1;
            } else {
                current_digits = rounded_s.Substring(rounded_s.IndexOf(".")).Length - 1;
            }
            while (current_digits < digits) {
                builder.Append("0");
                current_digits++;
            }
            rounded_s = builder.ToString();
        }
        return string.Format("{0}{1}", (show_plus_sign ? "+" : string.Empty), rounded_s);
    }

    public static string Plural(int i)
    {
        return i == 1 ? string.Empty : "s";
    }

    public static List<T> Clone_List<T>(List<T> list)
    {
        List<T> clone = new List<T>();
        foreach (T item in list) {
            clone.Add(item);
        }
        return clone;
    }

    public static Dictionary<T1, T2> Clone_Dictionary<T1, T2>(Dictionary<T1, T2> dictionary)
    {
        Dictionary<T1, T2> clone = new Dictionary<T1, T2>();
        foreach (KeyValuePair<T1, T2> pair in dictionary) {
            clone.Add(pair.Key, pair.Value);
        }
        return clone;
    }
}
