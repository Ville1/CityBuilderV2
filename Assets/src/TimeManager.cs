using System.Collections.Generic;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public enum Speed { Paused, Normal, Fast, Very_Fast }
    public static readonly float REALTIME_SECONDS_PER_DAY = 30.0f;
    public static readonly int DAYS_IN_MONTH = 30;
    public static readonly int MONTHS_IN_YEAR = 12;

    public static TimeManager Instance;

    private static readonly Dictionary<Speed, float> MULTIPLIERS = new Dictionary<Speed, float>() {
        { Speed.Paused, 0.0f },
        { Speed.Normal, 1.0f },
        { Speed.Fast, 2.0f },
        { Speed.Very_Fast, 5.0f }
    };

    private int day;
    private int month;
    private int year;

    private Speed saved_speed;
    private Speed speed;
    private float day_progress;

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
        speed = Speed.Normal;
    }

    /// <summary>
    /// Per frame update
    /// </summary>
    private void Update()
    {
        if(!Paused && Map.Instance.State == Map.MapState.Normal) {
            day_progress += Time.deltaTime * Multiplier * (1.0f / REALTIME_SECONDS_PER_DAY);
            int days_passed = (int)day_progress;
            if(days_passed > 0) {
                day_progress -= days_passed;
                day += days_passed;
                if(day > DAYS_IN_MONTH) {
                    day -= DAYS_IN_MONTH;
                    month++;
                    if(month > MONTHS_IN_YEAR) {
                        month -= MONTHS_IN_YEAR;
                        year++;
                    }
                }
            }
        }

        if(TopGUIManager.Instance != null && TopGUIManager.Instance.Active) {
            TopGUIManager.Instance.Update_Speed(speed);
            TopGUIManager.Instance.Update_Time(day, month, year);
        }
    }

    public bool Paused
    {
        get {
            return speed == Speed.Paused;
        }
        set {
            if(value == (speed == Speed.Paused)) {
                return;
            }
            if (value) {
                saved_speed = speed;
                speed = Speed.Paused;
            } else {
                speed = saved_speed != Speed.Paused ? saved_speed : Speed.Normal;
            }
        }
    }

    public float Multiplier
    {
        get {
            return MULTIPLIERS[speed];
        }
    }

    public Speed Current_Speed
    {
        get {
            return speed;
        }
        set {
            if(value == Speed.Paused) {
                saved_speed = speed;
            }
            speed = value;
        }
    }

    public void Speed_Up()
    {
        if(speed == Speed.Very_Fast) {
            return;
        }
        speed = (Speed)((int)speed + 1);
    }

    public void Speed_Down()
    {
        if (speed == Speed.Paused) {
            return;
        }
        speed = (Speed)((int)speed - 1);
    }

    public void Toggle_Pause()
    {
        Paused = !Paused;
    }

    public void Reset_Time()
    {
        day_progress = 0.0f;
        day = 1;
        month = 1;
        year = 1;
    }
}
