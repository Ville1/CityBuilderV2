using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour {
    public static MenuManager Instance { get; private set; }

    public Button Menu_Button;
    public Button City_Button;

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
    }

    /// <summary>
    /// Per frame update
    /// </summary>
    private void Update()
    {
        City_Button.interactable = Map.Instance != null && Map.Instance.State == Map.MapState.Normal;
    }

    public void Menu_On_Click()
    {
        MainMenuManager.Instance.Toggle();
    }

    public void City_On_Click()
    {
        CityInfoGUIManager.Instance.Active = true;
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
