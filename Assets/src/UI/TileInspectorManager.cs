using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TileInspectorManager : MonoBehaviour {
    public static TileInspectorManager Instance { get; private set; }

    public GameObject Panel;

    public Text Name_Text;
    public Text Coordinates_Text;
    public Image Image;

    public Text Base_Appeal_Text;
    public Text Base_Appeal_Range_Text;
    public Text Current_Appeal_Text;
    public GameObject Water_Container;
    public Text Water_Flow_Text;

    public GameObject Worker_Row_Prototype;
    public GameObject Workers_Content;

    public GameObject Mineral_Row_Prototype;
    public GameObject Minerals_Content;

    private Tile tile;
    private List<GameObject> worker_rows;
    private List<GameObject> mineral_rows;
    private long current_id;

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
        Panel.SetActive(false);
        Worker_Row_Prototype.SetActive(false);
        Mineral_Row_Prototype.SetActive(false);
        worker_rows = new List<GameObject>();
        mineral_rows = new List<GameObject>();
        current_id = 0;
    }

    /// <summary>
    /// Per frame update
    /// </summary>
    private void Update()
    { }

    public bool Active
    {
        get {
            return Panel.activeSelf;
        }
        set {
            if ((value && Panel.activeSelf) || (!value && !Panel.activeSelf)) {
                return;
            }
            if (value) {
                MasterUIManager.Instance.Close_Others(GetType().Name);
            } else {
                tile = null;
            }
            Panel.SetActive(value);
        }
    }

    public Tile Tile
    {
        get {
            return tile;
        }
        set {
            if(tile == value) {
                return;
            }
            tile = value;
            if(tile != null) {
                Active = true;
                Update_GUI();
            } else {
                Active = false;
            }
        }
    }

    private void Update_GUI()
    {
        Name_Text.text = Tile.Terrain;
        Coordinates_Text.text = Tile.Coordinates.Parse_Text(false, true);
        Image.sprite = SpriteManager.Instance.Get(Tile.Sprite, SpriteManager.SpriteType.Terrain);
        Base_Appeal_Text.text = Helper.Float_To_String(Tile.Base_Appeal, 2);
        Base_Appeal_Range_Text.text = Helper.Float_To_String(Tile.Base_Appeal_Range, 2);
        Current_Appeal_Text.text = Helper.Float_To_String(Tile.Appeal, 2);
        Water_Container.SetActive(Tile.Is_Water);
        if (Tile.Is_Water) {
            Water_Flow_Text.text = Tile.Water_Flow.HasValue ? Helper.Snake_Case_To_UI(Tile.Water_Flow.Value.ToString(), true) : "None";
        }

        if(current_id > 99999) {
            current_id = 0;
        }

        foreach(GameObject row in worker_rows) {
            GameObject.Destroy(row);
        }
        worker_rows.Clear();
        foreach (GameObject row in mineral_rows) {
            GameObject.Destroy(row);
        }
        mineral_rows.Clear();

        foreach(Tile.WorkData data in Tile.Worked_By) {
            GameObject row = GameObject.Instantiate(
                Worker_Row_Prototype,
                new Vector3(
                    Worker_Row_Prototype.transform.position.x,
                    Worker_Row_Prototype.transform.position.y - (worker_rows.Count * 20.0f),
                    Worker_Row_Prototype.transform.position.z
                ),
                Quaternion.identity,
                Workers_Content.transform
            );
            row.name = string.Format("worker_row_#{0}", current_id);
            current_id++;
            row.SetActive(true);

            GameObject.Find(string.Format("{0}/TypeText", row.name)).GetComponent<Text>().text = Helper.Snake_Case_To_UI(data.Type.ToString(), true);
            GameObject.Find(string.Format("{0}/BuildingText", row.name)).GetComponent<Text>().text = string.Format("{0} #{1}", data.Building.Name, data.Building.Id);
            if (!Tile.Can_Work(data.Building, data.Type)) {
                GameObject.Find(string.Format("{0}/BuildingText", row.name)).GetComponent<Text>().color = new Color(0.45f, 0.45f, 0.45f);
            }

            worker_rows.Add(row);
        }
        Workers_Content.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, worker_rows.Count * 20.0f + 5.0f);

        foreach (KeyValuePair<Mineral, float> mineral_data in Tile.Minerals) {
            GameObject row = GameObject.Instantiate(
                Mineral_Row_Prototype,
                new Vector3(
                    Mineral_Row_Prototype.transform.position.x,
                    Mineral_Row_Prototype.transform.position.y - (mineral_rows.Count * 20.0f),
                    Mineral_Row_Prototype.transform.position.z
                ),
                Quaternion.identity,
                Minerals_Content.transform
            );
            row.name = string.Format("mineral_row_#{0}", current_id);
            current_id++;
            row.SetActive(true);

            GameObject.Find(string.Format("{0}/AmountText", row.name)).GetComponent<Text>().text = Helper.Float_To_String(mineral_data.Value, 1);
            GameObject.Find(string.Format("{0}/MineralText", row.name)).GetComponent<Text>().text = Helper.Snake_Case_To_UI(mineral_data.Key.ToString(), true);

            mineral_rows.Add(row);
        }
        Minerals_Content.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, mineral_rows.Count * 20.0f + 5.0f);
    }
}
