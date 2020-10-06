using System;
using System.Collections.Generic;

[Serializable]
public class CitySaveData {
    public string Name;
    public float Cash;
    public List<BuildingSaveData> Buildings;
    public ContactsSaveData Contacts;
    public List<ExpeditionSaveData> Expeditions;
    public List<ColonyLocationSaveData> Colony_Locations;
}
