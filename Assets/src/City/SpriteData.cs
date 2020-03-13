using System.Collections.Generic;

public class SpriteData {
    public delegate string UpdateDelegate(Building building);

    public string Name { get; private set; }
    public UpdateDelegate Logic { get; private set; }
    public bool Simple { get { return Logic == null && Animation_Sprites.Count == 0; } } 
    public SpriteManager.SpriteType Type { get; private set; }
    public float Animation_Frame_Time { get; set; }
    public List<string> Animation_Sprites { get; set; }

    public SpriteData(string name)
    {
        Name = name;
        Type = SpriteManager.SpriteType.Building;
        Animation_Frame_Time = 0.0f;
        Animation_Sprites = new List<string>();
    }

    public SpriteData Clone()
    {
        SpriteData data = new SpriteData(Name);
        data.Logic = Logic;
        data.Animation_Frame_Time = Animation_Frame_Time;
        data.Animation_Sprites = Helper.Clone_List(Animation_Sprites);
        return data;
    }

    public void Add_Logic(UpdateDelegate p_delegate)
    {
        Logic = p_delegate;
    }
}
