public class SpriteData {
    public delegate string UpdateDelegate(Building building);

    public string Name { get; private set; }
    public UpdateDelegate Logic { get; private set; }
    public bool Simple { get { return Logic == null; } } 
    public SpriteManager.SpriteType Type { get; private set; }


    public SpriteData(string name)
    {
        Name = name;
        Type = SpriteManager.SpriteType.Building;
    }

    public SpriteData Clone()
    {
        SpriteData data = new SpriteData(Name);
        if (!Simple) {
            data.Add_Logic(Logic);
        }
        return data;
    }

    public void Add_Logic(UpdateDelegate p_delegate)
    {
        Logic = p_delegate;
    }
}
