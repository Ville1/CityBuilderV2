using System;
using System.Collections.Generic;

[Serializable]
public class ContactsSaveData {
    public List<ForeignCitySaveData> Cities;
}

[Serializable]
public class ForeignCitySaveData {
    public long Id;
    public string Name;
    public float Relations;
    public List<int> Preferred_Imports;
    public List<int> Disliked_Imports;
    public List<int> Unaccepted_Imports;
    public List<int> Exports;
    public List<int> Cheap_Exports;
    public List<int> Expensive_Exports;
    public int Trade_Route_Type;
    public int City_Type;
}