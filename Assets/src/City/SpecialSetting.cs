public class SpecialSetting {
    public enum SettingType { Input, Slider, Toggle }

    public string Name { get; private set; }
    public string Label { get; private set; }
    public SettingType Type { get; private set; }
    public float Slider_Value { get; set; }
    public bool Toggle_Value { get; set; }

    public SpecialSetting(string name, string label, SettingType type, float slider_value = 0.0f, bool toggle_value = false)
    {
        Name = name;
        Label = label;
        Type = type;
        Toggle_Value = toggle_value;
    }

    public SpecialSetting(SpecialSetting setting)
    {
        Name = setting.Name;
        Label = setting.Label;
        Type = setting.Type;
        Slider_Value = setting.Slider_Value;
        Toggle_Value = setting.Toggle_Value;
    }
}
