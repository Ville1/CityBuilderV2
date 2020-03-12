using UnityEngine;
using UnityEngine.UI;

public class NewGameGUIManager : MonoBehaviour
{
    public static readonly int DEFAULT_SIZE = 25;
    public static readonly int SIZE_MIN = 10;
    public static readonly int SIZE_MAX = 100;

    public static NewGameGUIManager Instance { get; private set; }

    public GameObject Panel;

    public Slider Width_Slider;
    public InputField Width_Input;
    public Slider Height_Slider;
    public InputField Height_Input;

    public Slider Forest_Count_Slider;
    public InputField Forest_Count_Input;
    public Slider Forest_Size_Slider;
    public InputField Forest_Size_Input;
    public Slider Forest_Density_Slider;
    public InputField Forest_Density_Input;

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
            if ((value && Panel.activeSelf) || (!value && !Panel.activeSelf)) {
                return;
            }
            if (value) {
                MasterUIManager.Instance.Close_Others(GetType().Name);
                Set_Defaults();
            }
            Panel.SetActive(value);
        }
    }
    
    private void Set_Defaults()
    {
        Width_Slider.value = (DEFAULT_SIZE - SIZE_MIN) / (SIZE_MAX - (float)SIZE_MIN);
        Width_Input.text = DEFAULT_SIZE.ToString();
        Height_Slider.value = (DEFAULT_SIZE - SIZE_MIN) / (SIZE_MAX - (float)SIZE_MIN);
        Height_Input.text = DEFAULT_SIZE.ToString();

        Forest_Count_Slider.value = 0.5f;
        Forest_Count_Input.text = "50%";
        Forest_Size_Slider.value = 0.5f;
        Forest_Size_Input.text = "50%";
        Forest_Density_Slider.value = 0.5f;
        Forest_Density_Input.text = "50%";
    }
    
    public void Update_Sliders()
    {
        Update_Size_Slider(Width_Input, Width_Slider);
        Update_Size_Slider(Height_Input, Height_Slider);
        Update_Forest_Slider(Forest_Count_Input, Forest_Count_Slider);
        Update_Forest_Slider(Forest_Size_Input, Forest_Size_Slider);
        Update_Forest_Slider(Forest_Density_Input, Forest_Density_Slider);
    }

    private void Update_Size_Slider(InputField input, Slider slider)
    {
        int value = 0;
        if(!int.TryParse(input.text, out value)) {
            value = DEFAULT_SIZE;
        }
        if(value < SIZE_MIN) {
            value = SIZE_MIN;
        }
        if(value > SIZE_MAX) {
            value = SIZE_MAX;
        }
        input.text = value.ToString();
        slider.value = (value - SIZE_MIN) / (SIZE_MAX - (float)SIZE_MIN);
    }


    private void Update_Forest_Slider(InputField input, Slider slider)
    {
        if (!input.text.EndsWith("%")) {
            input.text = input.text + "%";
        }
        string number_string = input.text.Substring(0, input.text.Length - 1);
        int int_value = 0;
        if(!int.TryParse(number_string, out int_value)) {
            int_value = 50;
        }
        if(int_value < 0) {
            int_value = 0;
        }
        if(int_value > 100) {
            int_value = 100;
        }
        input.text = int_value + "%";
        slider.value = int_value / 100.0f;
    }

    public void Update_Inputs()
    {
        Update_Size_Input(Width_Input, Width_Slider);
        Update_Size_Input(Height_Input, Height_Slider);
        Update_Forest_Input(Forest_Count_Input, Forest_Count_Slider);
        Update_Forest_Input(Forest_Size_Input, Forest_Size_Slider);
        Update_Forest_Input(Forest_Density_Input, Forest_Density_Slider);
    }

    private void Update_Size_Input(InputField input, Slider slider)
    {
        input.text = (Mathf.RoundToInt(slider.value * (SIZE_MAX - SIZE_MIN)) + SIZE_MIN).ToString();
    }

    private void Update_Forest_Input(InputField input, Slider slider)
    {
        input.text = Helper.Float_To_String(slider.value * 100.0f, 0) + "%";
    }

    public void Start_New()
    {
        int width = 0;
        if(!int.TryParse(Width_Input.text, out width)) {
            CustomLogger.Instance.Warning(string.Format("Invalid width: {0}", Width_Input.text));
            return;
        }
        int height = 0;
        if (!int.TryParse(Height_Input.text, out height)) {
            CustomLogger.Instance.Warning(string.Format("Invalid height: {0}", Height_Input.text));
            return;
        }
        Map.Instance.Start_Generation(width, height, Forest_Count_Slider.value, Forest_Size_Slider.value, Forest_Density_Slider.value);
        Active = false;
    }
}
