using System;
using System.Collections.Generic;

[Serializable]
public class MapSaveData {
    public int Width;
    public int Height;
    public List<TileSaveData> Tiles;
    public List<CoordinateSaveData> Ship_Spawns;
}

[Serializable]
public class CoordinateSaveData
{
    public int X;
    public int Y;
}