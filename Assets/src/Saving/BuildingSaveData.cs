﻿using System;
using System.Collections.Generic;

[Serializable]
public class BuildingSaveData {
    public long Id;
    public string Internal_Name;
    public int X;
    public int Y;
    public List<ResourceSaveData> Storage;
    public List<ResourceSaveData> Input_Storage;
    public List<ResourceSaveData> Output_Storage;
    public bool Is_Residence;
    public List<ResidentSaveData> Residents;
    public List<ResidentSaveData> Recently_Moved_Residents;
    public List<ResidentSaveData> Worker_Allocation;
    public long Taxed_By;
    public bool Is_Deconstructing;
    public bool Is_Paused;
    public bool Is_Connected;
    public float HP;
    public float Construction_Progress;
    public float Deconstruction_Progress;
    public List<SpecialSettingSaveData> Settings;
    public List<ServiceSaveData> Services;
    public List<StorageSettingSaveData> Storage_Settings;
    public int Selected_Sprite;
    public TradeRouteSettingsSaveData Trade_Route_Settings;
    public List<BuildingDictionaryData> Data;
    public bool Lock_Workers;
    public List<StorehouseTransferSaveData> Storehouse_Transfer_Data;
}

[Serializable]
public class SpecialSettingSaveData {
    public string Name;
    public float Slider_Value;
    public bool Toggle_Value;
    public int Dropdown_Selection;
    public bool Button_Was_Pressed;
}

[Serializable]
public class StorageSettingSaveData
{
    public int Resource;
    public int Limit;
    public int Priority;
}

[Serializable]
public class TradeRouteSettingsSaveData
{
    public long Partner;
    public int Action;
    public int Resource;
    public float Amount;
    public float Caravan_Cooldown;
}

[Serializable]
public class BuildingDictionaryData
{
    public string Key;
    public string Value;
}

[Serializable]
public class StorehouseTransferSaveData
{
    public List<long> Remaining_Connected_Buildings;
    public float Resources_Collected;
    public float Resources_Distributed;
    public float Max_Transfer;
}