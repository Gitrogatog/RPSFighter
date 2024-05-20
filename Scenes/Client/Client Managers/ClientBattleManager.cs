using Godot;
using System;

public partial class ClientBattleManager : Node
{
    // public int TeamID = 0;
    ClientFighter[] myTeam;
    ClientFighter[] enemyTeam;
    PackedScene clientFighter;
    public override void _Ready()
    {
        clientFighter = GD.Load<PackedScene>("res://Scenes/Fighters/client_fighter.tscn");
        // LoadPlayerTeam(0, p1FighterData);
        // LoadPlayerTeam(1, p2FighterData);
        StartBattle();
    }
    public void LoadTeams(FighterData[] mine, FighterData[] enemy)
    {
        myTeam = LoadTeam(mine);
        enemyTeam = LoadTeam(enemy);
    }
    public void StartBattle()
    {
        // turnManager.Initialize(p1Fighters, p2Fighters);
    }
    ClientFighter[] LoadTeam(FighterData[] fighterDatas)
    {
        int teamSize = fighterDatas.Length;
        ClientFighter[] team = new ClientFighter[teamSize];
        for (int i = 0; i < teamSize; i++)
        {
            team[i] = LoadFighter(fighterDatas[i]);
        }
        return team;
    }
    ClientFighter LoadFighter(FighterData data)
    {
        ClientFighter fighterNode = clientFighter.Instantiate<ClientFighter>();
        fighterNode.Initialize(data);
        AddChild(fighterNode);
        return fighterNode;
    }
}
