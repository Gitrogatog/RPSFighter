public class FighterJson
{
    public string Name;
    public string Nickname;
    public string[] actionNames;
}

public class TeamJson
{
    FighterJson[] fighters;
}