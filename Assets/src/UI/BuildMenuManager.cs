using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class BuildMenuManager : MonoBehaviour
{
    public static BuildMenuManager Instance { get; private set; }

    public GameObject Panel;

    public GameObject Tab_Button_Container;
    public Button Tab_Button_Prototype;

    public GameObject Tab_Prototype;
    public GameObject Building_Container_Prototype;

    private Vector3 tab_button_container_original_position;
    private Dictionary<Building.UI_Category, Button> tab_buttons;
    private Dictionary<Building.UI_Category, GameObject> tabs;

    private Building preview_building;

    /// <summary>
    /// Initializiation
    /// </summary>
    private void Start()
    {
        if (Instance != null) {
            CustomLogger.Instance.Error(LogMessages.MULTIPLE_INSTANCES);
            return;
        }
        Instance = this;
        tab_button_container_original_position = new Vector3(Tab_Button_Container.transform.position.x, Tab_Button_Container.transform.position.y, Tab_Button_Container.transform.position.z);
        Tab_Button_Prototype.gameObject.SetActive(false);
        Tab_Prototype.SetActive(false);
        Building_Container_Prototype.SetActive(false);
        Active = false;
        tab_buttons = new Dictionary<Building.UI_Category, Button>();
        tabs = new Dictionary<Building.UI_Category, GameObject>();
        Initialize();
        Interactable = false;
    }

    /// <summary>
    /// Per frame update
    /// </summary>
    private void Update()
    {
        if (Preview_Active && MouseManager.Instance.Tile_Under_Cursor != null) {
            Tile tile = MouseManager.Instance.Tile_Under_Cursor;
            if (preview_building.Is_Prototype) {
                preview_building = new Building(preview_building, tile, null, true);
            } else {
                preview_building.Move(tile);
            }
        }
    }

    public bool Active
    {
        get {
            return Panel.activeSelf;
        }
        set {
            Panel.SetActive(value);
            if (Active) {
                Tab_Button_Container.transform.position = new Vector3(
                    tab_button_container_original_position.x,
                    tab_button_container_original_position.y,
                    tab_button_container_original_position.z
                );
                Update_GUI();
            } else {
                Tab_Button_Container.transform.position = new Vector3(
                    tab_button_container_original_position.x,
                    tab_button_container_original_position.y - 95.0f,
                    tab_button_container_original_position.z
                );
            }
        }
    }

    public bool Interactable
    {
        get {
            return tab_buttons.Count == 0 ? false : tab_buttons.First().Value.interactable;
        }
        set {
            foreach(KeyValuePair<Building.UI_Category, Button> pair in tab_buttons) {
                pair.Value.interactable = value;
            }
        }
    }

    public bool Preview_Active
    {
        get {
            return preview_building != null;
        }
        set {
            if (value) {
                return;
            } else {
                End_Preview();
            }
        }
    }

    public void Build()
    {
        if (!Preview_Active && MouseManager.Instance.Tile_Under_Cursor != null) {
            return;
        }
        Tile tile = MouseManager.Instance.Tile_Under_Cursor;
        string message;
        if(!City.Instance.Can_Build(preview_building, tile, out message)) {
            MessageManager.Instance.Show_Message(message);
            return;
        }
        City.Instance.Build(preview_building, tile);
        if (!KeyboardManager.Instance.Keep_Building) {
            End_Preview();
        }
    }

    private void Initialize()
    {
        foreach (Building.UI_Category category in Enum.GetValues(typeof(Building.UI_Category))) {
            Button button = GameObject.Instantiate(
                Tab_Button_Prototype,
                new Vector3(
                    Tab_Button_Prototype.transform.position.x + (tab_buttons.Count * 150.0f),
                    Tab_Button_Prototype.transform.position.y,
                    Tab_Button_Prototype.transform.position.z
                ),
                Quaternion.identity,
                Tab_Button_Container.transform
            );
            button.name = string.Format("{0}_button", category.ToString());
            button.gameObject.SetActive(true);
            button.GetComponentInChildren<Text>().text = category.ToString();

            Button.ButtonClickedEvent click = new Button.ButtonClickedEvent();
            click.AddListener(new UnityAction(delegate () { Select_Tab(category); }));
            button.onClick = click;

            tab_buttons.Add(category, button);
        }
    }

    private void Update_GUI()
    {
        foreach(KeyValuePair<Building.UI_Category, GameObject> pair in tabs) {
            GameObject.Destroy(pair.Value);
        }
        tabs.Clear();

        foreach(Building.UI_Category category in Enum.GetValues(typeof(Building.UI_Category))) {
            GameObject tab = GameObject.Instantiate(
                Tab_Prototype,
                new Vector3(
                    Tab_Prototype.transform.position.x,
                    Tab_Prototype.transform.position.y,
                    Tab_Prototype.transform.position.z
                ),
                Quaternion.identity,
                Panel.transform
            );
            tab.name = string.Format("{0}_tab", category.ToString());
            tabs.Add(category, tab);

            int index = 0;
            foreach(Building building in BuildingPrototypes.Instance.Get(category)) {
                GameObject container = GameObject.Instantiate(
                    Building_Container_Prototype,
                    new Vector3(
                        Building_Container_Prototype.transform.position.x + (index * 100.0f),
                        Building_Container_Prototype.transform.position.y,
                        Building_Container_Prototype.transform.position.z
                    ),
                    Quaternion.identity,
                    tab.transform
                );
                container.SetActive(true);
                container.name = string.Format("{0}_container", category.ToString());

                container.GetComponentInChildren<Text>().text = building.Name;
                container.GetComponentInChildren<Image>().sprite = SpriteManager.Instance.Get(building.Sprite, SpriteManager.SpriteType.Building);

                Button.ButtonClickedEvent click = new Button.ButtonClickedEvent();
                click.AddListener(new UnityAction(delegate () { Select_Building(building); }));
                container.GetComponentInChildren<Button>().onClick = click;

                index++;
            }
        }
    }

    private void Select_Tab(Building.UI_Category tab)
    {
        if(Map.Instance.State != Map.MapState.Normal) {
            return;
        }
        if (!Active) {
            Active = true;
        }
        foreach(KeyValuePair<Building.UI_Category, GameObject> pair in tabs) {
            pair.Value.SetActive(pair.Key == tab);
        }
    }

    private void Select_Building(Building building)
    {
        //Active = false;
        if (Preview_Active) {
            End_Preview();
        }
        preview_building = building;
        InspectorManager.Instance.Building = building;
    }

    private void End_Preview()
    {
        if (preview_building.Is_Preview) {
            preview_building.Delete();
        }
        preview_building = null;
        InspectorManager.Instance.Building = null;
    }
}
