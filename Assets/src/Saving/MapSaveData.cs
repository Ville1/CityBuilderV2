using System;
using System.Collections.Generic;

[Serializable]
public class MapSaveData {
    public int Width;
    public int Height;
    public List<TileSaveData> Tiles;
}
