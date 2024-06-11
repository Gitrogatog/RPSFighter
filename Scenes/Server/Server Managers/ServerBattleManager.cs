using Godot;
using System;
using System.Collections.Generic;

public partial class ServerBattleManager : Node
{
    [Export] FighterData[] p1FighterData;
    [Export] FighterData[] p2FighterData;
    BaseFighter[] p1Fighters;
    BaseFighter[] p2Fighters;
    public ServerTurnManager turnManager;
    public ServerInputManager inputManager;
    public ServerLogManager logManager;
    PackedScene baseFighter;
    public int p1ID;
    public int p2ID;
    [Export] public bool hasMatchStarted = false;
    public override void _EnterTree()
    {
        baseFighter = GD.Load<PackedScene>("res://Scenes/Fighters/base_fighter.tscn");
        turnManager = GetNode<ServerTurnManager>("Turn Manager");
        inputManager = GetNode<ServerInputManager>("Input Manager");
        logManager = GetNode<ServerLogManager>("Log Manager");
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
}