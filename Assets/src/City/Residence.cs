using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Residence : Building {
    private static readonly int AMOUNT = 0;
    private static readonly int QUALITY = 1;

    public static readonly Dictionary<Resident, float> BASE_HAPPINESS = new Dictionary<Resident, float>() {
        { Resident.Peasant, 0.5f },
        { Resident.Citizen, 0.4f },
        { Resident.Noble, 0.25f }
    };

    public enum ServiceType { Food, Fuel }

    public Dictionary<Resident, int> Resident_Space { get; private set; }
    public Dictionary<Resident, int> Current_Residents { get; private set; }
    public Dictionary<Resident, float> Happiness { get; private set; }
    public Dictionary<Resident, List<string>> Happiness_Info { get; private set; }
    public float Food_Consumed { get; private set; }

    private Dictionary<Resident, float> migration_progress;
    private Dictionary<ServiceType, float[]> services;

    public Residence(Residence prototype, Tile tile, List<Tile> tiles, bool is_preview) : base(prototype, tile, tiles, is_preview)
    {
        Resident_Space = Helper.Clone_Dictionary(prototype.Resident_Space);
        Current_Residents = new Dictionary<Resident, int>();
        foreach (Resident resident in Enum.GetValues(typeof(Resident))) {
            Current_Residents.Add(resident, 0);
        }
        Happiness = new Dictionary<Resident, float>();
        foreach (Resident resident in Enum.GetValues(typeof(Resident))) {
            Happiness.Add(resident, 0.0f);
        }
        migration_progress = new Dictionary<Resident, float>();
        Happiness_Info = new Dictionary<Resident, List<string>>();
        foreach (Resident resident in Enum.GetValues(typeof(Resident))) {
            Happiness_Info.Add(resident, new List<string>());
            migration_progress.Add(resident, 0.0f);
        }
        services = new Dictionary<ServiceType, float[]>();
        foreach (ServiceType service in Enum.GetValues(typeof(ServiceType))) {
            services.Add(service, new float[2] { 0.0f, 0.0f });
        }
    }

    public Residence(string name, string internal_name, UI_Category category, string sprite, BuildingSize size, int hp, Dictionary<Resource, int> cost, int cash_cost, List<Resource> allowed_resources, int storage_limit, int construction_time,
        Dictionary<Resource, float> upkeep, float cash_upkeep, float construction_speed, float construction_range, Dictionary<Resident, int> resident_space, OnBuiltDelegate on_built, OnUpdateDelegate on_update, OnDeconstructDelegate on_deconstruct,
        OnHighlightDelegate on_highlight, List<Resource> consumes, List<Resource> produces) :
        base(name, internal_name, category, sprite, size, hp, cost, cash_cost, allowed_resources, storage_limit, 0.0f, construction_time, upkeep, cash_upkeep, construction_speed, construction_range, new Dictionary<Resident, int>(), 0, false, false,
            true, 0.0f, 0, on_built, on_update, on_deconstruct, on_highlight, consumes, produces)
    {
        Resident_Space = Helper.Clone_Dictionary(resident_space);
        foreach (Resident resident in Enum.GetValues(typeof(Resident))) {
            if (!Resident_Space.ContainsKey(resident)) {
                Resident_Space.Add(resident, 0);
            }
        }
        Current_Residents = new Dictionary<Resident, int>();
        Happiness = new Dictionary<Resident, float>();
        migration_progress = new Dictionary<Resident, float>();
    }

    public Residence(BuildingSaveData data) : base(data)
    {
        Resident_Space = Helper.Clone_Dictionary(BuildingPrototypes.Instance.Get_Residence(data.Internal_Name).Resident_Space);
        Current_Residents = new Dictionary<Resident, int>();
        foreach (Resident resident in Enum.GetValues(typeof(Resident))) {
            Current_Residents.Add(resident, 0);
        }
        foreach (ResidentSaveData resident_data in data.Residents) {
            Current_Residents[(Resident)resident_data.Resident] = resident_data.Count;
        }
        Happiness = new Dictionary<Resident, float>();
        foreach (Resident resident in Enum.GetValues(typeof(Resident))) {
            Happiness.Add(resident, 0.0f);
        }
        migration_progress = new Dictionary<Resident, float>();
        Happiness_Info = new Dictionary<Resident, List<string>>();
        foreach (Resident resident in Enum.GetValues(typeof(Resident))) {
            Happiness_Info.Add(resident, new List<string>());
            migration_progress.Add(resident, 0.0f);
        }
        services = new Dictionary<ServiceType, float[]>();
        foreach (ServiceType service in Enum.GetValues(typeof(ServiceType))) {
            ServiceSaveData service_data = data.Services.FirstOrDefault(x => x.Service == (int)service);
            if (service_data != null) {
                services.Add(service, new float[2] { service_data.Amount, service_data.Quality });
            } else {
                services.Add(service, new float[2] { 0.0f, 0.0f });
            }
        }
    }

    public new void Update(float delta_time)
    {
        base.Update(delta_time);
        if (!update_on_last_call || !Is_Built) {
            return;
        }
        delta_time = update_cooldown * TimeManager.Instance.Multiplier;
        foreach(KeyValuePair<Resident, List<string>> pair in Happiness_Info) {
            pair.Value.Clear();
        }

        if (!Is_Operational) {
            Current_Residents[Resident.Peasant] = 0;
            Current_Residents[Resident.Citizen] = 0;
            Current_Residents[Resident.Noble] = 0;
            return;
        }

        float max_unemployment_penalty = 0.5f;

        if(Resident_Space[Resident.Peasant] != 0) {
            if (City.Instance.Grace_Time) {
                Happiness[Resident.Peasant] = 0.5f;
                Happiness_Info[Resident.Peasant].Add(string.Format("Ignoring needs: {0} day{1}", Mathf.RoundToInt(City.Instance.Grace_Time_Remaining), Helper.Plural(Mathf.RoundToInt(City.Instance.Grace_Time_Remaining))));
            } else {
                Happiness[Resident.Peasant] = BASE_HAPPINESS[Resident.Peasant];
                Happiness_Info[Resident.Peasant].Add(string.Format("Base: {0}", UI_Happiness(BASE_HAPPINESS[Resident.Peasant])));

                if(City.Instance.Unemployment[Resident.Peasant] > 0.0f) {
                    Happiness[Resident.Peasant] -= max_unemployment_penalty * City.Instance.Unemployment[Resident.Peasant];
                    Happiness_Info[Resident.Peasant].Add(string.Format("Unemployment: -{0}", UI_Happiness(max_unemployment_penalty * City.Instance.Unemployment[Resident.Peasant])));
                }

                Happiness[Resident.Peasant] = Math.Max(0.0f, Happiness[Resident.Peasant]);
            }
        }

        if (Resident_Space[Resident.Citizen] != 0) {
            Happiness[Resident.Citizen] = BASE_HAPPINESS[Resident.Citizen];
            Happiness_Info[Resident.Citizen].Add(string.Format("Base: {0}", UI_Happiness(BASE_HAPPINESS[Resident.Citizen])));

            if (City.Instance.Unemployment[Resident.Citizen] > 0.0f) {
                Happiness[Resident.Citizen] -= max_unemployment_penalty * City.Instance.Unemployment[Resident.Citizen];
                Happiness_Info[Resident.Citizen].Add(string.Format("Unemployment: -{0}", UI_Happiness(max_unemployment_penalty * City.Instance.Unemployment[Resident.Citizen])));
            }

            Happiness[Resident.Citizen] = Math.Max(0.0f, Happiness[Resident.Citizen]);
        }

        if (Resident_Space[Resident.Noble] != 0) {
            Happiness[Resident.Noble] = BASE_HAPPINESS[Resident.Noble];
            Happiness_Info[Resident.Noble].Add(string.Format("Base: {0}", UI_Happiness(BASE_HAPPINESS[Resident.Noble])));

            if (City.Instance.Unemployment[Resident.Noble] > 0.0f) {
                Happiness[Resident.Noble] -= max_unemployment_penalty * City.Instance.Unemployment[Resident.Noble];
                Happiness_Info[Resident.Noble].Add(string.Format("Unemployment: -{0}", UI_Happiness(max_unemployment_penalty * City.Instance.Unemployment[Resident.Noble])));
            }

            Happiness[Resident.Noble] = Math.Max(0.0f, Happiness[Resident.Noble]);
        }

        float migration_threshold = 0.5f;
        float emigration_threshold = 0.25f;
        foreach (Resident resident in Enum.GetValues(typeof(Resident))) {
            if(Happiness[resident] >= migration_threshold && Resident_Space[resident] - Current_Residents[resident] > 0) {
                migration_progress[resident] += (delta_time / (float)((int)resident + 2));
                if(migration_progress[resident] >= 1.0f) {
                    migration_progress[resident] -= 1.0f;
                    Current_Residents[resident]++;
                }
            } else if(Happiness[resident] < emigration_threshold && Current_Residents[resident] > 0) {
                migration_progress[resident] -= (delta_time * (1.0f + (((float)((int)resident + 1)) / 3.0f)));
                if (migration_progress[resident] <= -1.0f) {
                    migration_progress[resident] += 1.0f;
                    Current_Residents[resident]--;
                }
            }
        }
    }

    public void Serve(ServiceType service, float amount, float quality)
    {
        float needed = 1.0f - services[service][AMOUNT];
        if (needed == 0.0f) {
            return;
        }
        float served = Math.Min(needed, amount);
        float remaining_quality_total = services[service][QUALITY] * services[service][AMOUNT];
        float new_quality_total = served * quality;
        float total_quality = remaining_quality_total + new_quality_total;
        services[service][AMOUNT] += served;
        services[service][QUALITY] = quality;
        if(services[service][AMOUNT] < 0.0f || services[service][AMOUNT] > 1.0f) {
            CustomLogger.Instance.Error(string.Format("services[service][AMOUNT] = {0}", services[service][AMOUNT]));
        }
        if (services[service][QUALITY] < 0.0f || services[service][QUALITY] > 1.0f) {
            CustomLogger.Instance.Error(string.Format("services[service][QUALITY] = {0}", services[service][QUALITY]));
        }
    }

    public float Service_Needed(ServiceType service)
    {
        return 1.0f - services[service][AMOUNT];
    }

    public float Service_Level(ServiceType service)
    {
        return services[service][AMOUNT];
    }

    public float Service_Quality(ServiceType service)
    {
        return services[service][QUALITY];
    }

    private string UI_Happiness(float happiness)
    {
        return Mathf.RoundToInt(100.0f * happiness).ToString();
    }
}
