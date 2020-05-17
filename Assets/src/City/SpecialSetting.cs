using System.Collections.Generic;

public class SpecialSetting {
    public enum SettingType { Input, Slider, Toggle, Dropdown, Button }

    public string Name { get; private set; }
    public string Label { get; set; }
    public SettingType Type { get; private set; }
    public float Slider_Value { get; set; }
    public bool Toggle_Value { get; set; }
    public List<string> Dropdown_Options { get; private set; }
    public int Dropdown_Selection { get; set; }
    public bool Button_Was_Pressed { get; set; }

    public SpecialSetting(string name, string label, SettingType type, float slider_value = 0.0f, bool toggle_value = false, List<string> dropdown_options = null, int dropdown_selection = 0)
    {
        Name = name;
        Label = label;
        Type = type;
        Toggle_Value = toggle_value;
        Dropdown_Options = dropdown_options == null ? new List<string>() : Helper.Clone_List(dropdown_options);
        Dropdown_Selection = dropdown_selection;
        Button_Was_Pressed = false;
    }

    public SpecialSetting(SpecialSetting setting)
    {
        Name = setting.Name;
        Label = setting.Label;
        Type = setting.Type;
        Slider_Value = setting.Slider_Value;
        Toggle_Value = setting.Toggle_Value;
        Dropdown_Options = Helper.Clone_List(setting.Dropdown_Options);
        Dropdown_Selection = setting.Dropdown_Selection;
        Button_Was_Pressed = setting.Button_Was_Pressed;
    }
}
