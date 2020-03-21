using System;
using System.Collections.Generic;

[Serializable]
public class TileSaveData {
    public int X;
    public int Y;
    public string Internal_Name;
    public List<long> Worked_By;
    public List<MineralSaveData> Minerals;
}

[Serializable]
public class MineralSaveData {
    public int Mineral;
    public float Amount;
}
