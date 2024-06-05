using Godot;
using System;
using System.Collections.Generic;

public partial class ServerBattleManager : Node
{
    [Export] FighterData[] p1FighterData;
    [Export] FighterData[] p2FighterData;
    BaseFighter[] p1Fighters;
    BaseFighter[] p2Fighters;
    [Export] public ServerTurnManager turnManager;
    [Export] public ServerInputManager inputManager;
    [Export] public ServerLogManager logManager;
    PackedScene baseFighter;
    public int p1ID;
    public int p2ID;
    [Export] public bool hasMatchStarted = false;
    public override void _Ready()
    {
        baseFighter = GD.Load<PackedScene>("res://Scenes/Fighters/base_fighter.tscn");
        // LoadPlayerTeam(0, p1FighterData);
        // LoadPlayerTeam(1, p2FighterData);
        // StartBattle();
    }
    public void LoadPlayerTeam(int playerIndex, BaseFighter[] team)
    {
        foreach (BaseFighter fighter in team)
        {
            AddChild(fighter);
        }
        if (playerIndex == 0)
        {
            p1Fighters = team;

        }
        else
        {
            p2Fighters = team;
        }
    }

    public void StartBattle()
    {
        if (!hasMatchStarted)
        {
            turnManager.Initialize(p1Fighters, p2Fighters);
            hasMatchStarted = true;
        }

    }
    // public void LoadPlayerTeam(int playerIndex, FighterData[] fighterDatas)
    // {
    //     if (playerIndex == 0)
    //     {
    //         // p1FighterData = fighterDatas;
    //         p1Fighters = LoadTeam(fighterDatas);
    //     }
    //     else
    //     {
    //         // p2FighterData = fighterDatas;
    //         p2Fighters = LoadTeam(fighterDatas);
    //     }
    // }
    // BaseFighter[] LoadTeam(FighterData[] fighterDatas)
    // {
    //     int teamSize = fighterDatas.Length;
    //     BaseFighter[] team = new BaseFighter[teamSize];
    //     for (int i = 0; i < teamSize; i++)
    //     {
    //         team[i] = LoadFighter(fighterDatas[i]);
    //     }
    //     return team;
    // }
    // BaseFighter LoadFighter(FighterData data)
    // {
    //     BaseFighter fighterNode = baseFighter.Instantiate<BaseFighter>();
    //     fighterNode.Initialize(data);
    //     AddChild(fighterNode);
    //     return fighterNode;
    // }
}