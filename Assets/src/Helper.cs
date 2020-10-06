using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class Helper {
    public delegate void ButtonClickDelegate();

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

    public static string Float_To_String_K(float f, int digits, int max_thousands)
    {
        if (f < max_thousands * 1000.0f) {
            return Float_To_String(f, digits);
        }
        return Float_To_String(f / 1000.0f, digits) + "k";
    }

    public static decimal Round(decimal d, int decimals)
    {
        int multiplier = (int)Mathf.Pow(10, decimals);
        d *= multiplier;
        int rounded = (int)Math.Round(d);
        return (decimal)rounded / multiplier;
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
    public static Dictionary<T, float> Add_Dictionary<T>(Dictionary<T, float> dictionary_1, Dictionary<T, float> dictionary_2)
    {
        Dictionary<T, float> dictionary_3 = new Dictionary<T, float>();
        foreach (KeyValuePair<T, float> pair in dictionary_1) {
            dictionary_3.Add(pair.Key, pair.Value);
        }
        foreach (KeyValuePair<T, float> pair in dictionary_2) {
            if (!dictionary_3.ContainsKey(pair.Key)) {
                dictionary_3.Add(pair.Key, pair.Value);
            } else {
                dictionary_3[pair.Key] += pair.Value;
            }
        }
        return dictionary_3;
    }

    public static Dictionary<T, float> Clamp_Dictionary<T>(Dictionary<T, float> dictionary, float min, float max)
    {
        Dictionary<T, float> new_dictionary = new Dictionary<T, float>();
        foreach (KeyValuePair<T, float> pair in dictionary) {
            new_dictionary.Add(pair.Key, Mathf.Clamp(pair.Value, min, max));
        }
        return new_dictionary;
    }

    public static Dictionary<T, float> Instantiate_Dictionary<T>(float value)
    {
        Dictionary<T, float> dictionary = new Dictionary<T, float>();
        foreach (T type in Enum.GetValues(typeof(T))) {
            dictionary.Add(type, value);
        }
        return dictionary;
    }

    public static Dictionary<T, int> Instantiate_Dictionary<T>(int value)
    {
        Dictionary<T, int> dictionary = new Dictionary<T, int>();
        foreach (T type in Enum.GetValues(typeof(T))) {
            dictionary.Add(type, value);
        }
        return dictionary;
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

    public static void Delete_All(List<GameObject> list)
    {
        foreach (GameObject obj in list) {
            GameObject.Destroy(obj);
        }
        list.Clear();
    }

    public static void Delete_All<T>(Dictionary<T, GameObject> dictionary)
    {
        foreach (KeyValuePair<T, GameObject> pair in dictionary) {
            GameObject.Destroy(pair.Value);
        }
        dictionary.Clear();
    }

    public static void Set_Text(string parent_game_object_name, string text_game_object_name, string text, Color? color = null)
    {
        GameObject text_game_object = GameObject.Find(string.Format("{0}/{1}", parent_game_object_name, text_game_object_name));
        text_game_object.GetComponentInChildren<Text>().text = text;
        if (color.HasValue) {
            text_game_object.GetComponentInChildren<Text>().color = color.Value;
        }
    }

    public static void Set_Image(string parent_game_object_name, string text_game_object_name, IHasSprite obj)
    {
        Set_Image(parent_game_object_name, text_game_object_name, obj.Sprite_Name, obj.Sprite_Type);
    }

    public static void Set_Image(string parent_game_object_name, string text_game_object_name, string sprite_name, SpriteManager.SpriteType sprite_type)
    {
        GameObject image_game_object = GameObject.Find(string.Format("{0}/{1}", parent_game_object_name, text_game_object_name));
        image_game_object.GetComponentInChildren<Image>().sprite = SpriteManager.Instance.Get(sprite_name, sprite_type);
    }

    public static void Set_Image(string parent_game_object_name, string text_game_object_name, IHasSprite obj, Color color)
    {
        Set_Image(parent_game_object_name, text_game_object_name, obj.Sprite_Name, obj.Sprite_Type, color);
    }

    public static void Set_Image(string parent_game_object_name, string text_game_object_name, string sprite_name, SpriteManager.SpriteType sprite_type, Color color)
    {
        GameObject image_game_object = GameObject.Find(string.Format("{0}/{1}", parent_game_object_name, text_game_object_name));
        Image image = image_game_object.GetComponentInChildren<Image>();
        image.sprite = SpriteManager.Instance.Get(sprite_name, sprite_type);
        image.color = color;
    }

    public static void Set_Button_On_Click(string parent_game_object_name, string button_game_object_name, ButtonClickDelegate delegate_p)
    {
        GameObject button_game_object = GameObject.Find(string.Format("{0}/{1}", parent_game_object_name, button_game_object_name));
        Button.ButtonClickedEvent on_click = new Button.ButtonClickedEvent();
        on_click.AddListener(delegate () {
            delegate_p();
        });
        button_game_object.GetComponentInChildren<Button>().onClick = on_click;
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

    public static string Random_String(int lenght)
    {
        string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        characters += characters.ToLower();
        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < lenght; i++) {
            builder.Append(RNG.Instance.Item(characters.ToCharArray().ToList()));
        }
        return builder.ToString();
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
