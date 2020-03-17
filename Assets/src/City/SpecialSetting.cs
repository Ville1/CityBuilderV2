public class SpecialSetting {
    public enum SettingType { Input, Slider }

    public string Name { get; private set; }
    public string Label { get; private set; }
    public SettingType Type { get; private set; }
    public float Slider_Value { get; set; }

    public SpecialSetting(string name, string label, SettingType type, float slider_value)
    {
        Name = name;
        Label = label;
        Type = type;
        Slider_Value = slider_value;
    }

    public SpecialSetting(SpecialSetting setting)
    {
        Name = setting.Name;
        Label = setting.Label;
        Type = setting.Type;
        Slider_Value = setting.Slider_Value;
    }
}
