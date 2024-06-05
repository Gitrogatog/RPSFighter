using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;

public partial class ServerLogManager : Node
{
    List<BattleLogElement> logs = new List<BattleLogElement>();
    public void StartTurnLog()
    {
        AddLog(BattleLogType.StartTurn);
    }
    public void RegisterAction(int team, string actionID)
    {
        ActionLog log = new ActionLog(team, actionID);
        string json = JsonSerializer.Serialize(log);
        AddLog(BattleLogType.Action, json);
    }
    public void RegisterSwap(int team, int swapToIndex, string fighterID)
    {
        SwapLog log = new SwapLog(team, swapToIndex, fighterID);
        string json = JsonSerializer.Serialize(log);
        AddLog(BattleLogType.Swap, json);
    }
    public void RegisterDamage(int targetTeam, int amount)
    {
        DamageLog log = new DamageLog(targetTeam, amount);
        string json = JsonSerializer.Serialize(log);
        AddLog(BattleLogType.Damage, json);
    }
    public void RegisterDeath(int team)
    {
        AddLog(BattleLogType.Death, team.ToString());
    }
    public void EndTurnLog()
    {
        AddLog(BattleLogType.EndTurn);
    }
    void AddLog(BattleLogType type, string data = "")
    {
        logs.Add(new BattleLogElement(type, data));
    }
    public BattleLogElement[] GetLog()
    {
        BattleLogElement[] elements = logs.ToArray();
        logs.Clear();
        return elements;
    }
}

