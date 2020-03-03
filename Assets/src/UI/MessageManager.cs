using UnityEngine;
using UnityEngine.UI;

public class MessageManager : MonoBehaviour {
    private static readonly float grace_time = 0.1f;

    public static MessageManager Instance;

    public GameObject Panel;
    public Text Text;

    public float Message_Time { get; set; }

    private float current_message_time_left;

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

        Message_Time = 1.0f;
        current_message_time_left = 0.0f;

        Panel.SetActive(false);
    }

    /// <summary>
    /// Per frame update
    /// </summary>
    private void Update()
    {
        if (!Active) {
            return;
        }
        current_message_time_left -= Time.deltaTime;
        if (current_message_time_left <= 0.0f) {
            Active = false;
        }
    }

    public bool Active
    {
        get {
            return Panel.activeSelf;
        }
        set {
            if (Active && !value && current_message_time_left >= Message_Time - grace_time) {
                return;
            }
            Panel.SetActive(value);
        }
    }

    public void Show_Message(string message)
    {
        Text.text = message;
        current_message_time_left = Message_Time;
        Active = true;
    }

    public void Close_Message()
    {
        current_message_time_left = 0.0f;
        Active = false;
    }
}
