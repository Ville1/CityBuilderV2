using System;
using System.Collections.Generic;
using UnityEngine;

public class Residence : Building {
    public Dictionary<Resident, int> Resident_Space { get; private set; }
    public Dictionary<Resident, int> Current_Residents { get; private set; }
    public Dictionary<Resident, float> Happiness { get; private set; }

    private float migration_progress;

    public Residence(Residence prototype, Tile tile, List<Tile> tiles, bool is_preview) : base(prototype, tile, tiles, is_preview)
    {
        Resident_Space = Helper.Clone_Dictionary(prototype.Resident_Space);
        Current_Residents = new Dictionary<Resident, int>();
        foreach(Resident resident in Enum.GetValues(typeof(Resident))) {
            Current_Residents.Add(resident, 0);
        }
        Happiness = new Dictionary<Resident, float>();
        foreach (Resident resident in Enum.GetValues(typeof(Resident))) {
            Happiness.Add(resident, 0.0f);
        }
        migration_progress = 0.0f;
    }

    public Residence(string name, string internal_name, UI_Category category, string sprite, BuildingSize size, int hp, Dictionary<Resource, int> cost, int cash_cost, List<Resource> allowed_resources, int storage_limit, int construction_time,
        Dictionary<Resource, float> upkeep, float cash_upkeep, float construction_speed, float construction_range, Dictionary<Resident, int> resident_space) : base(name, internal_name, category, sprite, size, hp, cost, cash_cost,
            allowed_resources, storage_limit, construction_time, upkeep, cash_upkeep, construction_speed, construction_range, new Dictionary<Resident, int>(), 0, false, false, true, 0.0f, 0)
    {
        Resident_Space = Helper.Clone_Dictionary(resident_space);
        foreach (Resident resident in Enum.GetValues(typeof(Resident))) {
            if (!Resident_Space.ContainsKey(resident)) {
                Resident_Space.Add(resident, 0);
            }
        }
        Current_Residents = new Dictionary<Resident, int>();
        Happiness = new Dictionary<Resident, float>();
        migration_progress = -1.0f;
    }


    public new void Update(float delta_time)
    {
        base.Update(delta_time);
        if (!update_on_last_call || !Is_Built) {
            return;
        }
        delta_time = update_cooldown * TimeManager.Instance.Multiplier;

        if (!Is_Operational) {
            Current_Residents[Resident.Peasant] = 0;
            Current_Residents[Resident.Citizen] = 0;
            Current_Residents[Resident.Noble] = 0;
            return;
        }

        if(Resident_Space[Resident.Peasant] != 0) {
            if (City.Instance.Grace_Time) {
                Happiness[Resident.Peasant] = 0.5f;
            } else {
                Happiness[Resident.Peasant] = 0.0f;
            }
        }

        if (Resident_Space[Resident.Citizen] != 0) {
            Happiness[Resident.Citizen] = 0.0f;
        }

        if (Resident_Space[Resident.Noble] != 0) {
            Happiness[Resident.Noble] = 0.0f;
        }

        if (City.Instance.Grace_Time) {
            migration_progress = delta_time * 0.25f;
        } else {
            migration_progress = delta_time * -0.25f;
        }
        int migration_delta = (int)migration_progress;
        migration_progress -= migration_delta;
        if(migration_delta != 0) {
            foreach (Resident resident in Enum.GetValues(typeof(Resident))) {
                if(City.Instance.Grace_Time && resident != Resident.Peasant) {
                    continue;
                }
                Current_Residents[resident] = Mathf.Clamp(Current_Residents[resident] + migration_delta, 0, Resident_Space[resident]);
            }
        }
    }
}
