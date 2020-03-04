﻿using UnityEngine;

public class KeyboardManager : MonoBehaviour
{
    public static KeyboardManager Instance { get; private set; }

    public bool Keep_Building { get; private set; }

    /// <summary>
    /// Initialization
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
    /// Update is called once per frame
    /// </summary>
    private void Update()
    {
        if (Input.GetButtonDown("Console")) {
            ConsoleManager.Instance.Toggle_Console();
        }

        Keep_Building = Input.GetButton("Keep building");
        if(Input.GetButtonUp("Keep building") && BuildMenuManager.Instance.Preview_Active) {
            BuildMenuManager.Instance.Preview_Active = false;
        }

        if (Input.GetButtonDown("Escape")) {
            if (BuildMenuManager.Instance.Preview_Active) {
                BuildMenuManager.Instance.Preview_Active = false;
            } else if (BuildMenuManager.Instance.Active) {
                BuildMenuManager.Instance.Active = false;
            } else if (MainMenuManager.Instance.Active) {
                MainMenuManager.Instance.Active = false;
            } else {
                MainMenuManager.Instance.Active = true;
            }
        }

        if (Input.anyKeyDown && !Input.GetMouseButtonDown(0) && !Input.GetMouseButtonDown(1) && !Input.GetMouseButtonDown(2)) {
            MessageManager.Instance.Close_Message();
        }

        if (!MasterUIManager.Instance.Intercept_Keyboard_Input) {
            //Move camera
            if (Input.GetAxis("Vertical") > 0.0f) {
                CameraManager.Instance.Move_Camera(Coordinates.Direction.North);
            }
            if (Input.GetAxis("Horizontal") < 0.0f) {
                CameraManager.Instance.Move_Camera(Coordinates.Direction.West);
            }
            if (Input.GetAxis("Vertical") < 0.0f) {
                CameraManager.Instance.Move_Camera(Coordinates.Direction.South);
            }
            if (Input.GetAxis("Horizontal") > 0.0f) {
                CameraManager.Instance.Move_Camera(Coordinates.Direction.East);
            }
        } else {
            MasterUIManager.Instance.Read_Keyboard_Input();
        }
    }
}
