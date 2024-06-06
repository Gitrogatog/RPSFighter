public class FighterJson
{
    public string Name { get; set; }
    public string Nickname { get; set; }
    public string[] actionNames { get; set; }
}

public class TeamJson
{
    public FighterJson[] fighters { get; set; }
}