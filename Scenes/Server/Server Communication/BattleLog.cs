public struct BattleLogMessage
{
    public ExpectedActionResponse response { get; set; }
    public BattleLogElement[] logs { get; set; }
    public BattleLogMessage(ExpectedActionResponse response, BattleLogElement[] logs)
    {
        this.response = response;
        this.logs = logs;
    }
}
public enum ExpectedActionResponse
{
    Any, Swap, None
}
public struct BattleLogElement
{
    public BattleLogType type { get; set; }
    public string data { get; set; }
    public BattleLogElement(BattleLogType type, string data = "")
    {
        this.type = type;
        this.data = data;
    }
}
public enum BattleLogType
{
    Action, Swap, Damage, Death, StartTurn, EndTurn
}

public struct DamageLog
{
    public int team { get; set; }
    public int damage { get; set; }
    public DamageLog(int team, int damage)
    {
        this.team = team;
        this.damage = damage;
    }
}
public struct ActionLog
{
    public int team { get; set; }
    public string actionID { get; set; }
    public ActionLog(int team, string actionID)
    {
        this.team = team;
        this.actionID = actionID;
    }
}
public struct SwapLog
{
    public int team { get; set; }
    public int swapToIndex { get; set; }
    public string fighterID { get; set; }
    public SwapLog(int team, int swapToIndex, string fighterID)
    {
        this.team = team;
        this.swapToIndex = swapToIndex;
        this.fighterID = fighterID;
    }
}