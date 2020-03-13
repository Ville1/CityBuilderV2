using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ConfirmationDialogManager : MonoBehaviour
{
    public static ConfirmationDialogManager Instance { get; private set; }

    public delegate void ConfirmAction();

    public GameObject Panel;
    public Text Text;
    public Button Confirm_Button;
    public Button Cancel_Button;

    private ConfirmAction current_action;

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
        Button.ButtonClickedEvent click = new Button.ButtonClickedEvent();
        click.AddListener(new UnityAction(delegate () { Active = false; }));
        Cancel_Button.onClick = click;
        Button.ButtonClickedEvent click2 = new Button.ButtonClickedEvent();
        click2.AddListener(new UnityAction(delegate () { Confirm(); }));
        Confirm_Button.onClick = click2;
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
            Panel.SetActive(value);
            if (Active) {
                MasterUIManager.Instance.Close_Others(GetType().Name);
            } else {
                current_action = null;
            }
        }
    }

    public void Show(string message, ConfirmAction action)
    {
        Text.text = message;
        current_action = action;
        Active = true;
    }

    private void Confirm()
    {
        if (current_action == null) {
            return;
        }
        current_action();
        Active = false;
    }
}