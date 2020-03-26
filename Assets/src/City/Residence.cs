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
    public static readonly Dictionary<Resident, float[]> FOOD_QUALITY_THRESHOLDS = new Dictionary<Resident, float[]>() {
        { Resident.Peasant, new float[3] { 0.20f, 0.40f, 0.50f } },
        { Resident.Citizen, new float[3] { 0.50f, 0.60f, 0.70f } },
        { Resident.Noble, new float[3] { 0.60f, 0.70f, 0.90f } }
    };
    public static readonly Dictionary<Resident, float[]> APPEAL_THRESHOLDS = new Dictionary<Resident, float[]>() {
        { Resident.Peasant, new float[4] { -3.5f, -2.0f, 1.00f, 2.00f } },
        { Resident.Citizen, new float[4] { -10.0f, 0.00f, 0.50f, 3.00f } },
        { Resident.Noble, new float[4] { -10.0f, 5.00f, 10.00f, 15.00f } }
    };
    public static readonly Dictionary<Resident, List<ServiceType>> SERVICES_CONSUMED = new Dictionary<Resident, List<ServiceType>>() {
        { Resident.Peasant, new List<ServiceType>() { ServiceType.Food, ServiceType.Fuel, ServiceType.Herbs, ServiceType.Salt, ServiceType.Tavern, ServiceType.Chapel, ServiceType.Taxes, ServiceType.Clothes } },
        { Resident.Citizen, new List<ServiceType>() { ServiceType.Food, ServiceType.Fuel, ServiceType.Herbs, ServiceType.Salt, ServiceType.Tavern, ServiceType.Chapel, ServiceType.Taxes, ServiceType.Clothes } },
        { Resident.Noble, new List<ServiceType>() { ServiceType.Food, ServiceType.Fuel, ServiceType.Herbs, ServiceType.Salt, ServiceType.Taxes } }
    };
    public static readonly Dictionary<ServiceType, float> OTHER_SERVICE_CONSUMPTION = new Dictionary<ServiceType, float>() {
        { ServiceType.Herbs, 0.0025f },
        { ServiceType.Salt, 0.1f },
        { ServiceType.Tavern, 0.025f }, //100 people consume 2.5 ale per day
        { ServiceType.Chapel, 0.05f },
        { ServiceType.Taxes, 2.00f },
        { ServiceType.Clothes, 0.01f }
    };
    public static readonly float FUEL_CONSUMPTION_PER_TILE = 0.025f;//Per day
    public static readonly float FUEL_CONSUMPTION_PER_RESIDENT = 0.005f;//Per day
    public static readonly float FOOD_CONSUMPTION = 0.15f;//Per day, per resident
    public static readonly float UNEMPLOYMENT_PENALTY_THRESHOLD = 0.1f;
    public static readonly float MAX_UNEMPLOYMENT_PENALTY = 0.5f;
    public static readonly float STARVATION_PENALTY = 1.0f;
    public static readonly float NO_FUEL_PENALTY = 0.25f;
    public static readonly float BAD_FUEL_SERVICE_PENALTY_MAX = 0.5f;
    public static readonly float BAD_FUEL_SERVICE_PENALTY_THRESHOLD = 0.25f;
    public static readonly float RESOURCES_FOR_FULL_SERVICE = 10.0f;
    public static readonly float MAX_DISREPAIR_PENALTY = 0.75f;
    public static readonly float MAX_TAX_PENALTY = 0.25f;

    public enum ServiceType { Food, Fuel, Herbs, Salt, Tavern, Chapel, Taxes, Clothes }

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
        OnHighlightDelegate on_highlight, List<Resource> consumes, List<Resource> produces, float appeal, float appeal_range) :
        base(name, internal_name, category, sprite, size, hp, cost, cash_cost, allowed_resources, storage_limit, 0.0f, construction_time, upkeep, cash_upkeep, construction_speed, construction_range, new Dictionary<Resident, int>(), 0, false, false,
            true, 0.0f, 0, on_built, on_update, on_deconstruct, on_highlight, consumes, produces, appeal, appeal_range)
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

    public float Current_Appeal
    {
        get {
            float total = 0.0f;
            foreach(Tile tile in Tiles) {
                total += tile.Appeal;
            }
            return total / Tiles.Count;
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
        
        int consumers = (City.Instance.Grace_Time ? 0 : Current_Residents[Resident.Peasant]) + Current_Residents[Resident.Citizen] + Current_Residents[Resident.Noble];
        float food_consumed_per_day = consumers * FOOD_CONSUMPTION;
        float food_consumed = Calculate_Actual_Amount(food_consumed_per_day, delta_time);
        float fuel_consumed = consumers == 0 ? 0 : Calculate_Actual_Amount((((Width * Height) * FUEL_CONSUMPTION_PER_TILE) + (consumers * FUEL_CONSUMPTION_PER_RESIDENT)), delta_time);
        services[ServiceType.Food][AMOUNT] = Math.Max(services[ServiceType.Food][AMOUNT] - (food_consumed / RESOURCES_FOR_FULL_SERVICE), 0.0f);
        services[ServiceType.Fuel][AMOUNT] = Math.Max(services[ServiceType.Fuel][AMOUNT] - (fuel_consumed / RESOURCES_FOR_FULL_SERVICE), 0.0f);
        Food_Consumed = (Current_Residents[Resident.Peasant] + Current_Residents[Resident.Citizen] + Current_Residents[Resident.Noble]) * FOOD_CONSUMPTION;
        
        foreach(ServiceType service in Enum.GetValues(typeof(ServiceType))) {
            if(!OTHER_SERVICE_CONSUMPTION.ContainsKey(service)) {
                continue;
            }
            consumers = Consumer_Count(service);
            float consumed_per_day = consumers * OTHER_SERVICE_CONSUMPTION[service];
            float consumed = Calculate_Actual_Amount(consumed_per_day, delta_time);
            services[service][AMOUNT] = Math.Max(services[service][AMOUNT] - (consumed / RESOURCES_FOR_FULL_SERVICE), 0.0f);
            if (services[service][AMOUNT] == 0.0f) {
                services[service][QUALITY] = 0.0f;
            }
        }
        if(services[ServiceType.Taxes][AMOUNT] != 0.0f && Current_Residents[Resident.Peasant] + Current_Residents[Resident.Citizen] + Current_Residents[Resident.Noble] == 0) {
            services[ServiceType.Taxes][AMOUNT] = 0.0f;
        }

        if (Resident_Space[Resident.Peasant] != 0) {
            if (City.Instance.Grace_Time) {
                Happiness[Resident.Peasant] = 0.5f;
                Happiness_Info[Resident.Peasant].Add(string.Format("Ignoring needs: {0} day{1}", Mathf.RoundToInt(City.Instance.Grace_Time_Remaining), Helper.Plural(Mathf.RoundToInt(City.Instance.Grace_Time_Remaining))));
            } else {
                Happiness[Resident.Peasant] = BASE_HAPPINESS[Resident.Peasant];
                Happiness_Info[Resident.Peasant].Add(string.Format("Base: {0}", UI_Happiness(BASE_HAPPINESS[Resident.Peasant])));
                if (Current_Residents[Resident.Peasant] != 0) {
                    float unemployment = City.Instance.Unemployment[Resident.Peasant] - UNEMPLOYMENT_PENALTY_THRESHOLD;
                    if (unemployment > 0.0f) {
                        Happiness[Resident.Peasant] -= MAX_UNEMPLOYMENT_PENALTY * unemployment;
                        Happiness_Info[Resident.Peasant].Add(string.Format("Unemployment: -{0}", UI_Happiness(MAX_UNEMPLOYMENT_PENALTY * unemployment)));
                    }

                    if (HP < Max_HP) {
                        float disrepair = (MAX_DISREPAIR_PENALTY * (1.0f - (HP / Max_HP))) * 0.75f;
                        Happiness[Resident.Peasant] -= disrepair;
                        Happiness_Info[Resident.Peasant].Add(string.Format("Disrepair: -{0}", UI_Happiness(disrepair)));
                    }

                    if (services[ServiceType.Fuel][AMOUNT] == 0.0f) {
                        Happiness[Resident.Peasant] -= NO_FUEL_PENALTY;
                        Happiness_Info[Resident.Peasant].Add(string.Format("No fuel: -{0}", UI_Happiness(NO_FUEL_PENALTY)));
                    } else if (services[ServiceType.Fuel][QUALITY] < BAD_FUEL_SERVICE_PENALTY_THRESHOLD) {
                        float fuel_penalty = 0.1f * ((BAD_FUEL_SERVICE_PENALTY_THRESHOLD - services[ServiceType.Fuel][QUALITY]) / BAD_FUEL_SERVICE_PENALTY_THRESHOLD);
                        Happiness[Resident.Peasant] -= fuel_penalty;
                        Happiness_Info[Resident.Peasant].Add(string.Format("Bad fuel service: -{0}", UI_Happiness(fuel_penalty)));
                    }

                    if (services[ServiceType.Food][AMOUNT] == 0.0f) {
                        Happiness[Resident.Peasant] -= STARVATION_PENALTY;
                        Happiness_Info[Resident.Peasant].Add(string.Format("Starvation: -{0}", UI_Happiness(STARVATION_PENALTY)));
                    } else {
                        if (services[ServiceType.Food][QUALITY] < FOOD_QUALITY_THRESHOLDS[Resident.Peasant][0]) {
                            float missing_quality = FOOD_QUALITY_THRESHOLDS[Resident.Peasant][0] - services[ServiceType.Food][QUALITY];
                            float penalty = missing_quality * 0.5f;
                            Happiness[Resident.Peasant] -= penalty;
                            Happiness_Info[Resident.Peasant].Add(string.Format("Food quality: -{0}", UI_Happiness(penalty)));
                        } else if (services[ServiceType.Food][QUALITY] > FOOD_QUALITY_THRESHOLDS[Resident.Peasant][1]) {
                            float extra_quality = services[ServiceType.Food][QUALITY] - FOOD_QUALITY_THRESHOLDS[Resident.Peasant][1];
                            extra_quality = Math.Min(extra_quality, FOOD_QUALITY_THRESHOLDS[Resident.Peasant][2] - FOOD_QUALITY_THRESHOLDS[Resident.Peasant][1]);
                            float bonus = extra_quality * 0.5f;
                            Happiness[Resident.Peasant] += bonus;
                            Happiness_Info[Resident.Peasant].Add(string.Format("Food quality: +{0}", UI_Happiness(bonus)));
                        }
                    }

                    float appeal = Current_Appeal;
                    if (appeal < APPEAL_THRESHOLDS[Resident.Peasant][1]) {
                        float missing_appeal = Mathf.Min(APPEAL_THRESHOLDS[Resident.Peasant][1] - appeal, APPEAL_THRESHOLDS[Resident.Peasant][1] - APPEAL_THRESHOLDS[Resident.Peasant][0]);
                        float appeal_penalty = missing_appeal * 0.10f;
                        Happiness[Resident.Peasant] -= appeal_penalty;
                        Happiness_Info[Resident.Peasant].Add(string.Format("Appeal: -{0}", UI_Happiness(appeal_penalty)));
                    } else if (appeal > APPEAL_THRESHOLDS[Resident.Peasant][2]) {
                        float bonus_appeal = Mathf.Min(appeal - APPEAL_THRESHOLDS[Resident.Peasant][2], APPEAL_THRESHOLDS[Resident.Peasant][3] - APPEAL_THRESHOLDS[Resident.Peasant][2]);
                        float appeal_bonus = bonus_appeal * 0.10f;
                        Happiness[Resident.Peasant] += appeal_bonus;
                        Happiness_Info[Resident.Peasant].Add(string.Format("Appeal: +{0}", UI_Happiness(appeal_bonus)));
                    }

                    if (services[ServiceType.Herbs][AMOUNT] != 0.0f) {
                        float base_herb_bonus = 0.05f;
                        float herb_bonus = base_herb_bonus * Mathf.Clamp(services[ServiceType.Herbs][QUALITY], 0.9f, 1.1f);
                        Happiness[Resident.Peasant] += herb_bonus;
                        Happiness_Info[Resident.Peasant].Add(string.Format("Herbs: +{0}", UI_Happiness(herb_bonus)));
                    }

                    if (services[ServiceType.Salt][AMOUNT] != 0.0f) {
                        float base_salt_bonus = 0.05f;
                        float salt_bonus = base_salt_bonus * Mathf.Clamp(services[ServiceType.Salt][QUALITY], 0.9f, 1.1f);
                        Happiness[Resident.Peasant] += salt_bonus;
                        Happiness_Info[Resident.Peasant].Add(string.Format("Salt: +{0}", UI_Happiness(salt_bonus)));
                    }

                    if (services[ServiceType.Clothes][AMOUNT] != 0.0f) {
                        //Simple clothes = 0.5f quality, Leather clothes = 1.0f quality
                        if (services[ServiceType.Clothes][QUALITY] > 0.5f) {
                            float base_clothing_bonus = 0.05f;
                            float clothing_bonus = base_clothing_bonus * (2.0f * (services[ServiceType.Clothes][QUALITY] - 0.5f));
                            Happiness[Resident.Peasant] += clothing_bonus;
                            Happiness_Info[Resident.Peasant].Add(string.Format("Clothing: +{0}", UI_Happiness(clothing_bonus)));
                        }
                    } else {
                        float clothing_penalty = 0.05f;
                        Happiness[Resident.Peasant] -= clothing_penalty;
                        Happiness_Info[Resident.Peasant].Add(string.Format("Clothing: -{0}", UI_Happiness(clothing_penalty)));
                    }

                    if (services[ServiceType.Tavern][AMOUNT] != 0.0f) {
                        float base_tavern_bonus = 0.10f;
                        float tavern_bonus = base_tavern_bonus * services[ServiceType.Tavern][QUALITY];
                        Happiness[Resident.Peasant] += tavern_bonus;
                        Happiness_Info[Resident.Peasant].Add(string.Format("Tavern: +{0}", UI_Happiness(tavern_bonus)));
                    }

                    if (services[ServiceType.Chapel][AMOUNT] != 0.0f) {
                        float base_chapel_bonus = 0.05f;
                        float chapel_bonus = base_chapel_bonus * services[ServiceType.Chapel][QUALITY];
                        Happiness[Resident.Peasant] += chapel_bonus;
                        Happiness_Info[Resident.Peasant].Add(string.Format("Chapel: +{0}", UI_Happiness(chapel_bonus)));
                    }

                    if (services[ServiceType.Taxes][AMOUNT] != 0.0f) {
                        float tax_penalty = MAX_TAX_PENALTY * services[ServiceType.Taxes][QUALITY];
                        Happiness[Resident.Peasant] -= tax_penalty;
                        Happiness_Info[Resident.Peasant].Add(string.Format("Taxes: -{0}", UI_Happiness(tax_penalty)));
                    }

                    Happiness[Resident.Peasant] = Math.Max(0.0f, Happiness[Resident.Peasant]);
                }
            }
        }

        if (Resident_Space[Resident.Citizen] != 0) {
            Happiness[Resident.Citizen] = BASE_HAPPINESS[Resident.Citizen];
            Happiness_Info[Resident.Citizen].Add(string.Format("Base: {0}", UI_Happiness(BASE_HAPPINESS[Resident.Citizen])));
            if (Current_Residents[Resident.Citizen] != 0) {
                float unemployment = City.Instance.Unemployment[Resident.Citizen] - UNEMPLOYMENT_PENALTY_THRESHOLD;
                if (unemployment > 0.0f) {
                    Happiness[Resident.Citizen] -= MAX_UNEMPLOYMENT_PENALTY * unemployment;
                    Happiness_Info[Resident.Citizen].Add(string.Format("Unemployment: -{0}", UI_Happiness(MAX_UNEMPLOYMENT_PENALTY * unemployment)));
                }

                if (HP < Max_HP) {
                    float disrepair = MAX_DISREPAIR_PENALTY * (1.0f - (HP / Max_HP));
                    Happiness[Resident.Citizen] -= disrepair;
                    Happiness_Info[Resident.Citizen].Add(string.Format("Disrepair: -{0}", UI_Happiness(disrepair)));
                }

                if (services[ServiceType.Fuel][AMOUNT] == 0.0f) {
                    Happiness[Resident.Citizen] -= NO_FUEL_PENALTY;
                    Happiness_Info[Resident.Citizen].Add(string.Format("No fuel: -{0}", UI_Happiness(NO_FUEL_PENALTY)));
                } else if (services[ServiceType.Fuel][QUALITY] < BAD_FUEL_SERVICE_PENALTY_THRESHOLD) {
                    float fuel_penalty = 0.15f * ((BAD_FUEL_SERVICE_PENALTY_THRESHOLD - services[ServiceType.Fuel][QUALITY]) / BAD_FUEL_SERVICE_PENALTY_THRESHOLD);
                    Happiness[Resident.Citizen] -= fuel_penalty;
                    Happiness_Info[Resident.Citizen].Add(string.Format("Bad fuel service: -{0}", UI_Happiness(fuel_penalty)));
                }

                if (services[ServiceType.Food][AMOUNT] == 0.0f) {
                    Happiness[Resident.Citizen] -= STARVATION_PENALTY;
                    Happiness_Info[Resident.Citizen].Add(string.Format("Starvation: -{0}", UI_Happiness(STARVATION_PENALTY)));
                } else {
                    if (services[ServiceType.Food][QUALITY] < FOOD_QUALITY_THRESHOLDS[Resident.Citizen][0]) {
                        float missing_quality = FOOD_QUALITY_THRESHOLDS[Resident.Citizen][0] - services[ServiceType.Food][QUALITY];
                        float penalty = missing_quality * 0.5f;
                        Happiness[Resident.Citizen] -= penalty;
                        Happiness_Info[Resident.Citizen].Add(string.Format("Food quality: -{0}", UI_Happiness(penalty)));
                    } else if (services[ServiceType.Food][QUALITY] > FOOD_QUALITY_THRESHOLDS[Resident.Citizen][1]) {
                        float extra_quality = services[ServiceType.Food][QUALITY] - FOOD_QUALITY_THRESHOLDS[Resident.Citizen][1];
                        extra_quality = Math.Min(extra_quality, FOOD_QUALITY_THRESHOLDS[Resident.Citizen][2] - FOOD_QUALITY_THRESHOLDS[Resident.Citizen][1]);
                        float bonus = extra_quality * 0.5f;
                        Happiness[Resident.Citizen] += bonus;
                        Happiness_Info[Resident.Citizen].Add(string.Format("Food quality: +{0}", UI_Happiness(bonus)));
                    }
                }

                float appeal = Current_Appeal;
                if (appeal < APPEAL_THRESHOLDS[Resident.Citizen][1]) {
                    float missing_appeal = Mathf.Min(APPEAL_THRESHOLDS[Resident.Citizen][1] - appeal, APPEAL_THRESHOLDS[Resident.Citizen][1] - APPEAL_THRESHOLDS[Resident.Citizen][0]);
                    float appeal_penalty = missing_appeal * 0.10f;
                    Happiness[Resident.Citizen] -= appeal_penalty;
                    Happiness_Info[Resident.Citizen].Add(string.Format("Appeal: -{0}", UI_Happiness(appeal_penalty)));
                } else if (appeal > APPEAL_THRESHOLDS[Resident.Citizen][2]) {
                    float bonus_appeal = Mathf.Min(appeal - APPEAL_THRESHOLDS[Resident.Citizen][2], APPEAL_THRESHOLDS[Resident.Citizen][3] - APPEAL_THRESHOLDS[Resident.Citizen][2]);
                    float appeal_bonus = bonus_appeal * 0.025f;
                    Happiness[Resident.Citizen] += appeal_bonus;
                    Happiness_Info[Resident.Citizen].Add(string.Format("Appeal: +{0}", UI_Happiness(appeal_bonus)));
                }

                if (services[ServiceType.Herbs][AMOUNT] != 0.0f) {
                    float base_herb_bonus = 0.05f;
                    float herb_bonus = base_herb_bonus * Mathf.Clamp(services[ServiceType.Herbs][QUALITY], 0.75f, 1.1f);
                    Happiness[Resident.Citizen] += herb_bonus;
                    Happiness_Info[Resident.Citizen].Add(string.Format("Herbs: +{0}", UI_Happiness(herb_bonus)));
                }

                if (services[ServiceType.Salt][AMOUNT] != 0.0f) {
                    float base_salt_bonus = 0.05f;
                    float salt_bonus = base_salt_bonus * Mathf.Clamp(services[ServiceType.Salt][QUALITY], 0.9f, 1.1f);
                    Happiness[Resident.Citizen] += salt_bonus;
                    Happiness_Info[Resident.Citizen].Add(string.Format("Salt: +{0}", UI_Happiness(salt_bonus)));
                }

                if (services[ServiceType.Clothes][AMOUNT] != 0.0f) {
                    //Simple clothes = 0.5f quality, Leather clothes = 1.0f quality
                    if (services[ServiceType.Clothes][QUALITY] > 0.5f) {
                        float base_clothing_bonus = 0.05f;
                        float clothing_bonus = base_clothing_bonus * (2.0f * (services[ServiceType.Clothes][QUALITY] - 0.5f));
                        Happiness[Resident.Citizen] += clothing_bonus;
                        Happiness_Info[Resident.Citizen].Add(string.Format("Clothing: +{0}", UI_Happiness(clothing_bonus)));
                    }
                } else {
                    float clothing_penalty = 0.25f;
                    Happiness[Resident.Citizen] -= clothing_penalty;
                    Happiness_Info[Resident.Citizen].Add(string.Format("Clothing: -{0}", UI_Happiness(clothing_penalty)));
                }

                if (services[ServiceType.Tavern][AMOUNT] != 0.0f) {
                    float base_tavern_bonus = 0.10f;
                    float tavern_bonus = base_tavern_bonus * services[ServiceType.Tavern][QUALITY];
                    Happiness[Resident.Citizen] += tavern_bonus;
                    Happiness_Info[Resident.Citizen].Add(string.Format("Tavern: +{0}", UI_Happiness(tavern_bonus)));
                }

                if (services[ServiceType.Chapel][AMOUNT] != 0.0f) {
                    float base_chapel_bonus = 0.05f;
                    float chapel_bonus = base_chapel_bonus * services[ServiceType.Chapel][QUALITY];
                    Happiness[Resident.Citizen] += chapel_bonus;
                    Happiness_Info[Resident.Citizen].Add(string.Format("Chapel: +{0}", UI_Happiness(chapel_bonus)));
                }

                if (services[ServiceType.Taxes][AMOUNT] != 0.0f) {
                    float tax_penalty = MAX_TAX_PENALTY * services[ServiceType.Taxes][QUALITY];
                    Happiness[Resident.Citizen] -= tax_penalty;
                    Happiness_Info[Resident.Citizen].Add(string.Format("Taxes: -{0}", UI_Happiness(tax_penalty)));
                }

                Happiness[Resident.Citizen] = Math.Max(0.0f, Happiness[Resident.Citizen]);
            }
        }

        if (Resident_Space[Resident.Noble] != 0) {
            Happiness[Resident.Noble] = BASE_HAPPINESS[Resident.Noble];
            Happiness_Info[Resident.Noble].Add(string.Format("Base: {0}", UI_Happiness(BASE_HAPPINESS[Resident.Noble])));
            if (Current_Residents[Resident.Noble] != 0) {
                float unemployment = City.Instance.Unemployment[Resident.Noble] - UNEMPLOYMENT_PENALTY_THRESHOLD;
                if (unemployment > 0.0f) {
                    Happiness[Resident.Noble] -= (MAX_UNEMPLOYMENT_PENALTY * unemployment) * 0.5f;
                    Happiness_Info[Resident.Noble].Add(string.Format("Unemployment: -{0}", UI_Happiness((MAX_UNEMPLOYMENT_PENALTY * unemployment) * 0.5f)));
                }

                if (HP < Max_HP) {
                    float disrepair = (MAX_DISREPAIR_PENALTY * (1.0f - (HP / Max_HP))) * 1.25f;
                    Happiness[Resident.Noble] -= disrepair;
                    Happiness_Info[Resident.Noble].Add(string.Format("Disrepair: -{0}", UI_Happiness(disrepair)));
                }

                if (services[ServiceType.Fuel][AMOUNT] == 0.0f) {
                    Happiness[Resident.Noble] -= NO_FUEL_PENALTY;
                    Happiness_Info[Resident.Noble].Add(string.Format("No fuel: -{0}", UI_Happiness(NO_FUEL_PENALTY)));
                } else if (services[ServiceType.Fuel][QUALITY] < BAD_FUEL_SERVICE_PENALTY_THRESHOLD) {
                    float fuel_penalty = 0.25f * ((BAD_FUEL_SERVICE_PENALTY_THRESHOLD - services[ServiceType.Fuel][QUALITY]) / BAD_FUEL_SERVICE_PENALTY_THRESHOLD);
                    Happiness[Resident.Noble] -= fuel_penalty;
                    Happiness_Info[Resident.Noble].Add(string.Format("Bad fuel service: -{0}", UI_Happiness(fuel_penalty)));
                }

                if (services[ServiceType.Food][AMOUNT] == 0.0f) {
                    Happiness[Resident.Noble] -= STARVATION_PENALTY;
                    Happiness_Info[Resident.Noble].Add(string.Format("Starvation: -{0}", UI_Happiness(STARVATION_PENALTY)));
                } else {
                    if (services[ServiceType.Food][QUALITY] < FOOD_QUALITY_THRESHOLDS[Resident.Noble][0]) {
                        float missing_quality = FOOD_QUALITY_THRESHOLDS[Resident.Noble][0] - services[ServiceType.Food][QUALITY];
                        float penalty = missing_quality * 1.25f;
                        Happiness[Resident.Noble] -= penalty;
                        Happiness_Info[Resident.Noble].Add(string.Format("Food quality: -{0}", UI_Happiness(penalty)));
                    } else if (services[ServiceType.Food][QUALITY] > FOOD_QUALITY_THRESHOLDS[Resident.Noble][1]) {
                        float extra_quality = services[ServiceType.Food][QUALITY] - FOOD_QUALITY_THRESHOLDS[Resident.Noble][1];
                        extra_quality = Math.Min(extra_quality, FOOD_QUALITY_THRESHOLDS[Resident.Noble][2] - FOOD_QUALITY_THRESHOLDS[Resident.Noble][1]);
                        float bonus = extra_quality * 1.10f;
                        Happiness[Resident.Noble] += bonus;
                        Happiness_Info[Resident.Noble].Add(string.Format("Food quality: +{0}", UI_Happiness(bonus)));
                    }
                }

                float appeal = Current_Appeal;
                if (appeal < APPEAL_THRESHOLDS[Resident.Noble][1]) {
                    float missing_appeal = Mathf.Min(APPEAL_THRESHOLDS[Resident.Noble][1] - appeal, APPEAL_THRESHOLDS[Resident.Noble][1] - APPEAL_THRESHOLDS[Resident.Noble][0]);
                    float appeal_penalty = missing_appeal * 0.10f;
                    Happiness[Resident.Noble] -= appeal_penalty;
                    Happiness_Info[Resident.Noble].Add(string.Format("Appeal: -{0}", UI_Happiness(appeal_penalty)));
                } else if (appeal > APPEAL_THRESHOLDS[Resident.Noble][2]) {
                    float bonus_appeal = Mathf.Min(appeal - APPEAL_THRESHOLDS[Resident.Noble][2], APPEAL_THRESHOLDS[Resident.Noble][3] - APPEAL_THRESHOLDS[Resident.Noble][2]);
                    float appeal_bonus = bonus_appeal * 0.10f;
                    Happiness[Resident.Noble] += appeal_bonus;
                    Happiness_Info[Resident.Noble].Add(string.Format("Appeal: +{0}", UI_Happiness(appeal_bonus)));
                }

                if (services[ServiceType.Herbs][AMOUNT] != 0.0f) {
                    float base_herb_bonus = 0.025f;
                    float herb_bonus = base_herb_bonus * Mathf.Clamp(services[ServiceType.Herbs][QUALITY], 0.75f, 1.1f);
                    Happiness[Resident.Noble] += herb_bonus;
                    Happiness_Info[Resident.Noble].Add(string.Format("Herbs: +{0}", UI_Happiness(herb_bonus)));
                }

                if (services[ServiceType.Salt][AMOUNT] != 0.0f) {
                    float base_salt_bonus = 0.05f;
                    float salt_bonus = base_salt_bonus * Mathf.Clamp(services[ServiceType.Salt][QUALITY], 0.9f, 1.1f);
                    Happiness[Resident.Noble] += salt_bonus;
                    Happiness_Info[Resident.Noble].Add(string.Format("Salt: +{0}", UI_Happiness(salt_bonus)));
                }

                if (services[ServiceType.Taxes][AMOUNT] != 0.0f) {
                    float tax_penalty = MAX_TAX_PENALTY * services[ServiceType.Taxes][QUALITY];
                    Happiness[Resident.Noble] -= tax_penalty;
                    Happiness_Info[Resident.Noble].Add(string.Format("Taxes: -{0}", UI_Happiness(tax_penalty)));
                }

                Happiness[Resident.Noble] = Math.Max(0.0f, Happiness[Resident.Noble]);
            }
        }

        float migration_threshold = 0.35f;
        float emigration_threshold = 0.20f;
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
        float needed = Service_Needed(service);
        if (needed == 0.0f) {
            return;
        }
        if (float.IsNaN(services[service][QUALITY])) {
            services[service][QUALITY] = 0.0f;
            CustomLogger.Instance.Error(string.Format("{0} service quality is NaN", service.ToString()));
        }
        float served = Math.Min(needed, amount);
        float old_quality_relative = services[service][AMOUNT] / (services[service][AMOUNT] + served);
        float new_quality_realtive = served / (services[service][AMOUNT] + served);
        float total_quality = (old_quality_relative * services[service][QUALITY]) + (new_quality_realtive * quality);
        services[service][AMOUNT] += served;
        services[service][QUALITY] = total_quality;
        if(services[service][AMOUNT] < 0.0f || services[service][AMOUNT] > 1.0f) {
            CustomLogger.Instance.Error(string.Format("services[service][AMOUNT] = {0}", services[service][AMOUNT]));
        }
        if (services[service][QUALITY] < 0.0f || services[service][QUALITY] > 1.0f) {
            CustomLogger.Instance.Error(string.Format("services[service][QUALITY] = {0}", services[service][QUALITY]));
        }
    }

    public float Service_Needed(ServiceType service)
    {
        if(Max_Consumer_Count(service) == 0) {
            return 0.0f;
        }
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

    private int Consumer_Count(ServiceType service)
    {
        int count = 0;
        foreach(KeyValuePair<Resident, List<ServiceType>> pair in SERVICES_CONSUMED) {
            if (pair.Value.Contains(service) && (pair.Key != Resident.Peasant || !City.Instance.Grace_Time)) {
                count += Current_Residents[pair.Key];
            }
        }
        return count;
    }

    private int Max_Consumer_Count(ServiceType service)
    {
        int count = 0;
        foreach (KeyValuePair<Resident, List<ServiceType>> pair in SERVICES_CONSUMED) {
            if (pair.Value.Contains(service)) {
                count += Resident_Space[pair.Key];
            }
        }
        return count;
    }
}
