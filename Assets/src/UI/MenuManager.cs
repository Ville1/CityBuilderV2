using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour {
    public static MenuManager Instance { get; private set; }

    public Button Menu_Button;
    public Button City_Button;
    public Button Views_Button;
    public GameObject Views_Panel;
    public Button None_Button;
    public Button Appeal_Button;
    public Button Minerals_Button;
    public Button Water_Flow_Button;
    public Button Alerts_Button;

    /// <summary>
    /// Initializiation
    /// </summary>
    private void Start()
    {
        if (Instance != null) {
            CustomLogger.Instance.Error(LogMessages.MULTIPLE_INSTANCES);
            return;
        }
        Views_Panel.SetActive(false);
        Instance = this;
    }

    /// <summary>
    /// Per frame update
    /// </summary>
    private void Update()
    {
        City_Button.interactable = Map.Instance != null && Map.Instance.State == Map.MapState.Normal;
        Views_Button.interactable = Map.Instance != null && Map.Instance.State == Map.MapState.Normal;
        None_Button.interactable = Map.Instance != null && Map.Instance.View != Map.MapView.None;
        Appeal_Button.interactable = Map.Instance != null && Map.Instance.View != Map.MapView.Appeal;
        Minerals_Button.interactable = Map.Instance != null && Map.Instance.View != Map.MapView.Minerals;
        Water_Flow_Button.interactable = Map.Instance != null && Map.Instance.View != Map.MapView.Water_Flow;
        Alerts_Button.GetComponentInChildren<Text>().text = string.Format("Alerts ({0})", Map.Instance != null && !Map.Instance.Hide_Alerts ? "y" : "n");
    }

    public void Menu_On_Click()
    {
        MainMenuManager.Instance.Toggle();
    }

    public void City_On_Click()
    {
        CityInfoGUIManager.Instance.Active = true;
    }

    public void Views_On_Click()
    {
        Views_Panel.SetActive(!Views_Panel.activeSelf);
    }

    public void None_On_Click()
    {
        Map.Instance.View = Map.MapView.None;
    }

    public void Appeal_On_Click()
    {
        Map.Instance.View = Map.MapView.Appeal;
    }

    public void Minerals_On_Click()
    {
        Map.Instance.View = Map.MapView.Minerals;
    }

    public void Water_Flow_On_Click()
    {
        Map.Instance.View = Map.MapView.Water_Flow;
    }

    public void Alerts_On_Click()
    {
        Map.Instance.Hide_Alerts = !Map.Instance.Hide_Alerts;
    }

    public void Close_Views_Panel()
    {
        Views_Panel.SetActive(false);
    }

    public bool Interactable
    {
        get {
            return Menu_Button.interactable;
        }
        set {
            Menu_Button.interactable = value;
            City_Button.interactable = value;
        }
    }
}
