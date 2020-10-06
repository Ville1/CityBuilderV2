using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RowScrollView<T>
{
    private static readonly float PADDING = 5.0f;

    private static long current_id = 0;

    public string Name { get; private set; }
    public long Id { get; private set; }
    public GameObject Content { get; private set; }
    public GameObject Row_Prototype { get; private set; }
    public float Row_Spacing { get; private set; }

    private Dictionary<T, GameObject> rows;
    private long current_row_id;

    public RowScrollView(string name, GameObject content, GameObject row_prototype, float row_spacing)
    {
        Name = name;
        Id = current_id;
        current_id = current_id != long.MaxValue ? current_id + 1 : 0;
        Content = content;
        Row_Prototype = row_prototype;
        Row_Spacing = row_spacing;
        current_row_id = 0;

        Row_Prototype.SetActive(false);
        rows = new Dictionary<T, GameObject>();
    }

    public void Clear()
    {
        Helper.Delete_All(rows);
    }

    public GameObject Add(T key, List<UIElementData> data)
    {
        GameObject row = GameObject.Instantiate(
            Row_Prototype,
            new Vector3(
                Row_Prototype.transform.position.x,
                Row_Prototype.transform.position.y - (rows.Count * Row_Spacing),
                Row_Prototype.transform.position.z
            ),
            Quaternion.identity,
            Content.transform
        );
        row.SetActive(true);
        row.name = string.Format("scroll_{0}_row_{1}", Name, current_row_id);
        current_row_id = current_row_id == long.MaxValue ? 0 : current_row_id + 1;

        foreach (UIElementData data_item in data) {
            data_item.Set(row);
        }

        rows.Add(key, row);
        Content.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (rows.Count * Row_Spacing) + PADDING);
        return row;
    }

    public void Update(T key, List<UIElementData> data)
    {
        if (!rows.ContainsKey(key)) {
            Add(key, data);
            return;
        }
        foreach (UIElementData data_item in data) {
            data_item.Set(rows[key]);
        }
    }

    public override string ToString()
    {
        return string.Format("{0}#{1}", Name, Id);
    }
}

public class UIElementData : IHasSprite
{
    public enum DataType { Text, Button, Image }
    public delegate void OnClickDelegate();

    public string GameObject_Name { get; set; }
    public string Text { get; set; }
    public Color? Text_Color { get; set; }
    public OnClickDelegate On_Click { get; set; }
    public string Sprite_Name { get; set; }
    public SpriteManager.SpriteType Sprite_Type { get; set; }

    public UIElementData(string gameobject_name, string text, Color? color = null)
    {
        GameObject_Name = gameobject_name;
        Text = text;
        On_Click = null;
        Text_Color = color;
        Sprite_Name = null;
        Sprite_Type = SpriteManager.SpriteType.UI;
    }

    public UIElementData(string gameobject_name, string button_text, OnClickDelegate on_click)
    {
        GameObject_Name = gameobject_name;
        Text = button_text;
        On_Click = on_click;
        Sprite_Name = null;
        Sprite_Type = SpriteManager.SpriteType.UI;
    }

    public UIElementData(string gameobject_name, string sprite_name, SpriteManager.SpriteType sprite_type)
    {
        GameObject_Name = gameobject_name;
        Text = null;
        On_Click = null;
        Sprite_Name = sprite_name;
        Sprite_Type = sprite_type;
    }

    public void Set(GameObject obj)
    {
        switch (Type) {
            case DataType.Text:
                Helper.Set_Text(obj.name, GameObject_Name, Text);
                if (Text_Color.HasValue) {
                    GameObject.Find(string.Format("{0}/{1}", obj.name, GameObject_Name)).GetComponentInChildren<Text>().color = Text_Color.Value;
                }
                break;
            case DataType.Button:
                GameObject button_game_object = GameObject.Find(string.Format("{0}/{1}", obj.name, GameObject_Name));
                Button.ButtonClickedEvent on_click = new Button.ButtonClickedEvent();
                on_click.AddListener(delegate () {
                    On_Click();
                });
                button_game_object.GetComponentInChildren<Button>().onClick = on_click;
                if (Text != null) {
                    Text button_text = button_game_object.GetComponentInChildren<Text>();
                    if (button_text != null) {
                        button_text.text = Text;
                    }
                }
                break;
            case DataType.Image:
                Helper.Set_Image(obj.name, GameObject_Name, this);
                break;
        }
    }

    public DataType Type
    {
        get {
            if (On_Click != null) {
                return DataType.Button;
            }
            if (!string.IsNullOrEmpty(Sprite_Name)) {
                return DataType.Image;
            }
            return DataType.Text;
        }
    }
}