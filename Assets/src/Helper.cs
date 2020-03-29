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
        return string.Format("{0}{1}", (show_plus_sign && f >= 0.0f ? "+" : string.Empty), rounded_s);
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

    public static string Snake_Case_To_UI(string snake_case, bool capitalize)
    {
        if (string.IsNullOrEmpty(snake_case)) {
            return string.Empty;
        }
        snake_case = snake_case.ToLower().Replace('_', ' ');
        if (capitalize) {
            snake_case = snake_case[0].ToString().ToUpper() + snake_case.Substring(1);
        }
        return snake_case;
    }

    public static Coordinates.Direction Rotate(Coordinates.Direction direction, int amount)
    {
        int direction_i = (int)direction;
        direction_i += amount;
        while(direction_i < 0) {
            direction_i += 8;
        }
        while(direction_i > 7) {
            direction_i -= 8;
        }
        return (Coordinates.Direction)direction_i;
    }

    public static string Abreviation(Coordinates.Direction direction)
    {
        switch (direction) {
            case Coordinates.Direction.North:
                return "N";
            case Coordinates.Direction.North_East:
                return "NE";
            case Coordinates.Direction.East:
                return "E";
            case Coordinates.Direction.South_East:
                return "SE";
            case Coordinates.Direction.South:
                return "S";
            case Coordinates.Direction.South_West:
                return "SW";
            case Coordinates.Direction.West:
                return "W";
            case Coordinates.Direction.North_West:
                return "NW";
        }
        return "?";
    }
}
