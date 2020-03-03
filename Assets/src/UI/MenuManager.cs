using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour {
    public static MenuManager Instance { get; private set; }

    public Button Menu_Button;

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

    }

    public void Menu_On_Click()
    {
        MainMenuManager.Instance.Toggle();
    }

    public bool Interactable
    {
        get {
            return Menu_Button.interactable;
        }
        set {
            Menu_Button.interactable = value;
        }
    }
}
