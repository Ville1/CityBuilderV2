using UnityEngine;

public class MasterUIManager : MonoBehaviour {
    public static MasterUIManager Instance { get; private set; }

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

    public void Close_Others(string type_name)
    {
        if (ConsoleManager.Instance != null && typeof(ConsoleManager).Name != type_name) {
            ConsoleManager.Instance.Close_Console();
        }
        if (MainMenuManager.Instance != null && typeof(MainMenuManager).Name != type_name) {
            MainMenuManager.Instance.Active = false;
        }
        if (BuildMenuManager.Instance != null && typeof(BuildMenuManager).Name != type_name) {
            BuildMenuManager.Instance.Active = false;
        }
        if (InspectorManager.Instance != null && typeof(InspectorManager).Name != type_name) {
            InspectorManager.Instance.Active = false;
        }
        if (NewGameGUIManager.Instance != null && typeof(NewGameGUIManager).Name != type_name) {
            NewGameGUIManager.Instance.Active = false;
        }
        if (ConfirmationDialogManager.Instance != null && typeof(ConfirmationDialogManager).Name != type_name) {
            ConfirmationDialogManager.Instance.Active = false;
        }
        if (SaveGUIManager.Instance != null && typeof(SaveGUIManager).Name != type_name) {
            SaveGUIManager.Instance.Active = false;
        }
        if (LoadGUIManager.Instance != null && typeof(LoadGUIManager).Name != type_name) {
            LoadGUIManager.Instance.Active = false;
        }
        if(StorageSettingsGUIManager.Instance != null && typeof(StorageSettingsGUIManager).Name != type_name) {
            StorageSettingsGUIManager.Instance.Active = false;
        }
        if (CityInfoGUIManager.Instance != null && typeof(CityInfoGUIManager).Name != type_name) {
            CityInfoGUIManager.Instance.Active = false;
        }
        if (SpecialSettingsGUIManager.Instance != null && typeof(SpecialSettingsGUIManager).Name != type_name) {
            SpecialSettingsGUIManager.Instance.Active = false;
        }
        if (TileInspectorManager.Instance != null && typeof(TileInspectorManager).Name != type_name) {
            TileInspectorManager.Instance.Active = false;
        }
        if(ContactsGUIManager.Instance != null && typeof(ContactsGUIManager).Name != type_name) {
            ContactsGUIManager.Instance.Active = false;
        }
        if (TradeGUIManager.Instance != null && typeof(TradeGUIManager).Name != type_name) {
            TradeGUIManager.Instance.Active = false;
        }
        if (NewExpeditionGUIManager.Instance != null && typeof(NewExpeditionGUIManager).Name != type_name) {
            NewExpeditionGUIManager.Instance.Active = false;
        }
        MenuManager.Instance.Close_Views_Panel();
    }

    public bool Intercept_Keyboard_Input
    {
        get {
            return ConsoleManager.Instance.Is_Open() || SaveGUIManager.Instance.Active || LoadGUIManager.Instance.Active || StorageSettingsGUIManager.Instance.Active || TradeGUIManager.Instance.Active;
        }
    }

    public void Read_Keyboard_Input()
    {
        if (ConsoleManager.Instance.Is_Open()) {
            if (Input.GetButtonDown("Submit")) {
                ConsoleManager.Instance.Run_Command();
            }
            if (Input.GetButtonDown("Console scroll down")) {
                ConsoleManager.Instance.Scroll_Down();
            }
            if (Input.GetButtonDown("Console scroll up")) {
                ConsoleManager.Instance.Scroll_Up();
            }
            if (Input.GetButtonDown("Auto complete")) {
                ConsoleManager.Instance.Auto_Complete();
            }
            if (Input.GetButtonDown("Console history up")) {
                ConsoleManager.Instance.Command_History_Up();
            }
            if (Input.GetButtonDown("Console history down")) {
                ConsoleManager.Instance.Command_History_Down();
            }
        }
    }
}
