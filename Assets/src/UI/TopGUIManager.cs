using UnityEngine;
using UnityEngine.UI;

public class TopGUIManager : MonoBehaviour {
    public static TopGUIManager Instance;

    public GameObject Panel;
    public Text Name_Text;
    public Text Time_Text;
    public Text Speed_Text;
    public Text Cash_Text;
    public Text Wood_Text;
    public Text Lumber_Text;
    public Text Stone_Text;
    public Text Tools_Text;
    public Text Peasant_Info_Text;
    public Text Citizen_Info_Text;
    public Text Noble_Info_Text;

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
    }

    /// <summary>
    /// Per frame update
    /// </summary>
    private void Update() {

    }

    public bool Active
    {
        get {
            return Panel.activeSelf;
        }
        set {
            Panel.SetActive(value);
        }
    }

    public void Update_City_Info(string name, float cash, float income, int wood, int lumber, int stone, int tools)
    {
        if (!Active) {
            return;
        }
        Name_Text.text = name;
        Cash_Text.text = string.Format("{0} {1}", Helper.Float_To_String(cash, 0), Helper.Float_To_String(income, 1, true));
        Wood_Text.text = wood.ToString();
        Lumber_Text.text = lumber.ToString();
        Stone_Text.text = stone.ToString();
        Tools_Text.text = tools.ToString();
    }

    public void Update_Speed(TimeManager.Speed speed)
    {
        switch (speed) {
            case TimeManager.Speed.Paused:
                Speed_Text.text = "=";
                break;
            case TimeManager.Speed.Normal:
                Speed_Text.text = ">";
                break;
            case TimeManager.Speed.Fast:
                Speed_Text.text = ">>";
                break;
            case TimeManager.Speed.Very_Fast:
                Speed_Text.text = ">>>";
                break;
            default:
                Speed_Text.text = "???";
                CustomLogger.Instance.Warning(string.Format("Invalid speed: {0}", speed.ToString()));
                break;
        }
    }

    public void Update_Time(string time)
    {
        Time_Text.text = time;
    }
}
