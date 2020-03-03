using UnityEngine;
using UnityEngine.UI;

public class ProgressBarManager : MonoBehaviour
{
    public static ProgressBarManager Instance { get; private set; }

    public GameObject Panel;
    public Slider Slider;
    public Text Text;

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
        Active = false;
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
            if (Panel.activeSelf == value) {
                return;
            }
            Panel.SetActive(value);
            MenuManager.Instance.Interactable = !value;
            if (Active) {
                MainMenuManager.Instance.Active = false;
            }
        }
    }

    public void Show(string message, float progress)
    {
        Text.text = message;
        Slider.value = progress;
    }
}
