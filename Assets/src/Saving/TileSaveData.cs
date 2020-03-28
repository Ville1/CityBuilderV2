using System;
using System.Collections.Generic;

[Serializable]
public class TileSaveData {
    public int X;
    public int Y;
    public string Internal_Name;
    public List<WorkSaveData> Worked_By;
    public List<MineralSaveData> Minerals;
    public bool Adjacent_To_Water;
}

[Serializable]
public class WorkSaveData {
    public long Id;
    public int Type;
}

[Serializable]
public class MineralSaveData {
    public int Mineral;
    public float Amount;
}
