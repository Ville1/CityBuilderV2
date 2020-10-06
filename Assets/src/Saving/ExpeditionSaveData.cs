using System;
using System.Collections.Generic;

[Serializable]
public class ExpeditionSaveData {
    public int Lenght;
    public int Goal;
    public long Building_Id;
    public int Resource;
    public float Time_Remaining;
    public int State;
    public ColonyLocationSaveData Colony_Data;
}

[Serializable]
public class ColonyLocationSaveData {
    public string Name;
    public List<int> Preferred_Imports;
    public List<int> Disliked_Imports;
    public List<int> Unaccepted_Imports;
    public List<int> Exports;
    public List<int> Cheap_Exports;
    public List<int> Expensive_Exports;
    public int Trade_Route_Type;
}