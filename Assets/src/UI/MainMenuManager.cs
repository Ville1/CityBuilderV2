using UnityEngine;

public class MainMenuManager : MonoBehaviour {
    public static MainMenuManager Instance { get; private set; }

    public GameObject Panel;

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
    private void Update()
    {

    }

    public bool Active
    {
        get {
            return Panel.activeSelf;
        }
        set {
            if((value && Panel.activeSelf) || (!value && !Panel.activeSelf)) {
                return;
            }
            if (value) {
                MasterUIManager.Instance.Close_Others(GetType().Name);
            }
            Panel.SetActive(value);
        }
    }

    public void Toggle()
    {
        Active = !Active;
    }

    public void New_Game_Button_On_Click()
    {
        NewGameGUIManager.Instance.Active = true;
    }

    public void Exit_Button_On_Click()
    {
        Main.Quit();
    }
}
