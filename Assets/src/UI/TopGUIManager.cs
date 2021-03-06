﻿using System;
using UnityEngine;
using UnityEngine.UI;

public class TopGUIManager : MonoBehaviour {
    public static TopGUIManager Instance;

    public GameObject Panel;
    public Text Name_Text;
    public Text Time_Text;
    public Text Speed_Text;
    public Text Cash_Text;
    public Text Wood_Text;
    public Text Lumber_Text;
    public Text Stone_Text;
    public Text Bricks_Text;
    public Text Tools_Text;
    public Text Peasant_Info_Text_1;
    public Text Peasant_Info_Text_2;
    public Text Citizen_Info_Text_1;
    public Text Citizen_Info_Text_2;
    public Text Noble_Info_Text_1;
    public Text Noble_Info_Text_2;

    public Button Expand_Button;
    public GameObject Extra_Panel;
    public Text Marble_Text;
    public Text Mechanisms_Text;
    public Text Glass_Text;

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
        Extra_Panel.SetActive(false);
    }

    /// <summary>
    /// Per frame update
    /// </summary>
    private void Update() {

    }

    public bool Active
    {
        get {
            return Panel.activeSelf;
        }
        set {
            Panel.SetActive(value);
            Close_Extra_Panel();
        }
    }

    public void Update_City_Info(string name, float cash, float income, int wood, int lumber, int stone, int bricks, int tools, int marble, int mechanisms, int glass, int peasant_current, int peasant_max,
        float peasant_happiness, float peasant_employment_relative, int peasant_employment, int citizen_current, int citizen_max, float citizen_happiness, float citizen_employment_relative, int citizen_employment,
        int noble_current, int noble_max, float noble_happiness, float noble_employment_relative, int noble_employment)
    {
        if (!Active) {
            return;
        }
        Name_Text.text = name;
        Cash_Text.text = string.Format("{0} {1}", Helper.Float_To_String(cash, 0), Helper.Float_To_String(income, 1, true));
        Wood_Text.text = wood.ToString();
        Lumber_Text.text = lumber.ToString();
        Stone_Text.text = stone.ToString();
        Bricks_Text.text = bricks.ToString();
        Tools_Text.text = tools.ToString();
        Peasant_Info_Text_1.text = string.Format("{0} / {1}{2}{3}", peasant_current, peasant_max, Environment.NewLine, Helper.Float_To_String(peasant_happiness * 100.0f, 0));
        Citizen_Info_Text_1.text = string.Format("{0} / {1}{2}{3}", citizen_current, citizen_max, Environment.NewLine, Helper.Float_To_String(citizen_happiness * 100.0f, 0));
        Noble_Info_Text_1.text = string.Format("{0} / {1}{2}{3}", noble_current, noble_max, Environment.NewLine, Helper.Float_To_String(noble_happiness * 100.0f, 0));
        Peasant_Info_Text_2.text = string.Format("{0}{1}{2}%", peasant_employment, Environment.NewLine, Helper.Float_To_String(peasant_employment_relative * 100.0f, 0));
        Citizen_Info_Text_2.text = string.Format("{0}{1}{2}%", citizen_employment, Environment.NewLine, Helper.Float_To_String(citizen_employment_relative * 100.0f, 0));
        Noble_Info_Text_2.text = string.Format("{0}{1}{2}%", noble_employment, Environment.NewLine, Helper.Float_To_String(noble_employment_relative * 100.0f, 0));

        Marble_Text.text = marble.ToString();
        Mechanisms_Text.text = mechanisms.ToString();
        Glass_Text.text = glass.ToString();
    }

    public void Update_Speed(TimeManager.Speed speed)
    {
        switch (speed) {
            case TimeManager.Speed.Paused:
                Speed_Text.text = "=";
                break;
            case TimeManager.Speed.Normal:
                Speed_Text.text = ">";
                break;
            case TimeManager.Speed.Fast:
                Speed_Text.text = ">>";
                break;
            case TimeManager.Speed.Very_Fast:
                Speed_Text.text = ">>>";
                break;
            default:
                Speed_Text.text = "???";
                CustomLogger.Instance.Warning(string.Format("Invalid speed: {0}", speed.ToString()));
                break;
        }
    }

    public void Update_Time(int day, int month, int year, float total_days)
    {
        Time_Text.text = string.Format("D:{0} M:{1} Y:{2}", day < 10 ? "0" + day : day.ToString(), month < 10 ? "0" + month : month.ToString(), year);
        TooltipManager.Instance.Register_Tooltip(Time_Text.gameObject, string.Format("Total days: {0}", Helper.Float_To_String(total_days, 1)), gameObject);
    }

    public void Update_Time(string time)
    {
        Time_Text.text = time;
    }

    public void Close_Extra_Panel()
    {
        Extra_Panel.SetActive(false);
        Expand_Button.GetComponentInChildren<Text>().text = "V";
    }

    public void Open_Extra_Panel()
    {
        Extra_Panel.SetActive(true);
        Expand_Button.GetComponentInChildren<Text>().text = "-";
    }

    public void Toggle_Extra_Panel()
    {
        if (Extra_Panel.activeSelf) {
            Close_Extra_Panel();
        } else {
            Open_Extra_Panel();
        }
    }
}
