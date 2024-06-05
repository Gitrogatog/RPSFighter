public record struct BattleLogMessage(ExpectedActionResponse response, BattleLogElement[] logs);
public enum ExpectedActionResponse
{
    Any, Swap, None
}
public struct BattleLogElement
{
    public BattleLogType type;
    public string data;
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

public record struct DamageLog(int team, int damage);
public record struct ActionLog(int team, string actionID);
public record struct SwapLog(int team, int swapToIndex, string fighterID);