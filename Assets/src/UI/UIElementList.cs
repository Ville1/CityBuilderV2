using System.Collections.Generic;
using UnityEngine;

public class UIElementList<T>
{
    private static long current_id = 0;

    public string Name { get; private set; }
    public long Id { get; private set; }
    public GameObject Container { get; private set; }
    public GameObject Element_Prototype { get; private set; }
    public Vector2 Element_Spacing { get; private set; }

    private Dictionary<T, GameObject> elements;
    private long current_row_id;

    public UIElementList(string name, GameObject container, GameObject element_prototype, Vector2 element_spacing)
    {
        Name = name;
        Id = current_id;
        current_id = current_id != long.MaxValue ? current_id + 1 : 0;
        Container = container;
        Element_Prototype = element_prototype;
        Element_Spacing = element_spacing;
        current_row_id = 0;

        Element_Prototype.SetActive(false);
        elements = new Dictionary<T, GameObject>();
    }

    public void Clear()
    {
        Helper.Delete_All(elements);
    }

    public void Add(T key, List<UIElementData> data)
    {
        GameObject element = GameObject.Instantiate(
            Element_Prototype,
            new Vector3(
                Element_Prototype.transform.position.x + (elements.Count * Element_Spacing.x),
                Element_Prototype.transform.position.y - (elements.Count * Element_Spacing.y),
                Element_Prototype.transform.position.z
            ),
            Quaternion.identity,
            Container.transform
        );
        element.SetActive(true);
        element.name = string.Format("list_{0}_element_{1}", Name, current_row_id);
        current_row_id = current_row_id == long.MaxValue ? 0 : current_row_id + 1;

        foreach (UIElementData data_item in data) {
            data_item.Set(element);
        }
        elements.Add(key, element);
    }
}