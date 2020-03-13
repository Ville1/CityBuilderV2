using System;
using System.Collections.Generic;

[Serializable]
public class BuildingSaveData {
    public long Id;
    public string Internal_Name;
    public int X;
    public int Y;
    public List<ResourceSaveData> Storage;
    public bool Is_Residence;
    public List<ResidentSaveData> Residents;
    public List<ResidentSaveData> Worker_Allocation;
    public bool Is_Deconstructing;
    public bool Is_Paused;
    public bool Is_Connected;
    public float HP;
    public float Construction_Progress;
    public float Deconstruction_Progress;
}
