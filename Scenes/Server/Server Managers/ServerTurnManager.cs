using Godot;
using System;
using System.Collections.Generic;
using System.Net.Http;

public partial class ServerTurnManager : Node
{
    [Export] ServerLogManager logManager;
    PlayerTeamInfo player1Team;
    PlayerTeamInfo player2Team;
    IAction[] actionQueue = new IAction[2];
    IAction p1Action;
    IAction p2Action;
    public bool expectingEndOfTurnSwap = false;
    int currentTurn = 0;
    public static event Action<int, int> AddPlayerToRoom = delegate { };
    public event Action TurnEndEvent = delegate { };
    public event Action<bool, bool> DeathSwapEvent = delegate { };
    public event Action<int> MatchEndedEvent = delegate { };

    public void Initialize(BaseFighter[] p1Team, BaseFighter[] p2Team)
    {
        currentTurn = 1;
        player1Team = new PlayerTeamInfo(p1Team);
        player2Team = new PlayerTeamInfo(p2Team);
    }

    public void RunActions(IAction player1Action, IAction player2Action)
    {
        if (!expectingEndOfTurnSwap)
        {
            RunTurn(player1Action, player2Action);
        }
        else
        {
            RunEndSwaps(player1Action, player2Action);
        }
        CheckTurnFinish();

    }
    void RunTurn(IAction player1Action, IAction player2Action)
    {
        actionQueue[0] = player1Action;
        actionQueue[1] = player2Action;
        RunStartTurnEffects();
        if (actionQueue[0].Priority >= actionQueue[1].Priority)
        {
            actionQueue[0].UseAction(0, this);
            actionQueue[1].UseAction(1, this);
        }
        else
        {
            actionQueue[1].UseAction(1, this);
            actionQueue[0].UseAction(0, this);
        }
        actionQueue[0] = null;
        actionQueue[1] = null;
        RunEndOfTurnEffects();
    }
    void RunEndSwaps(IAction player1Action, IAction player2Action)
    {
        player1Action.UseAction(0, this);
        player2Action.UseAction(1, this);

    }
    void CheckTurnFinish()
    {
        if (player1Team.activeFighterIndex == -1 || player2Team.activeFighterIndex == -1)
        {
            // expect swap input
            expectingEndOfTurnSwap = true;
            GD.Print("Server has initiated death swap!");
            DeathSwapEvent.Invoke(player1Team.activeFighterIndex == -1, player2Team.activeFighterIndex == -1);
        }
        else
        {
            EndTurn();
        }

    }
    void SendBattleStateBackToPlayers()
    {

    }
    void RunStartTurnEffects()
    {
        GD.Print("Start turn effects!");
    }

    void RunEndOfTurnEffects()
    {
        GD.Print("End turn effects!");
    }
    void EndTurn()
    {
        expectingEndOfTurnSwap = false;
        currentTurn++;
        TurnEndEvent.Invoke();
    }

    public void PrintBattleState()
    {
        GD.Print($"Turn {currentTurn}");
        PrintCurrentFighter(0);
        PrintCurrentFighter(1);
    }

    void PrintCurrentFighter(int player)
    {
        GD.Print($"P{player} active fighter state:");
        BaseFighter fighter = GetActiveFighter(player);
        if (fighter != null)
        {
            fighter.PrintState();
        }
        else
        {
            GD.Print("fighter is dead");
        }
    }

    void DecideTurnOrder(IAction player1Action, IAction player2Action)
    {

        if (player1Action.Priority > player2Action.Priority)
        {
            actionQueue[0] = player1Action;
            actionQueue[1] = player2Action;
        }
        else if (player1Action.Priority < player2Action.Priority)
        {
            actionQueue[0] = player2Action;
            actionQueue[1] = player1Action;
        }
        else
        {
            actionQueue[0] = player1Action;
            actionQueue[1] = player2Action;
        }
    }

    public void UseAction(int userTeam, string actionID)
    {
        logManager.RegisterAction(userTeam, actionID);
    }

    public void Swap(int userTeam, int swapToIndex)
    {

        PlayerTeamInfo team = userTeam == 0 ? player1Team : player2Team;
        GD.Print($"P{userTeam} is swapping from {team.activeFighterIndex} to {swapToIndex}");
        if (team.activeFighterIndex != swapToIndex)
        {
            BaseFighter baseFighter = team.team[swapToIndex];
            logManager.RegisterSwap(userTeam, swapToIndex, baseFighter.name);
            team.activeFighterIndex = swapToIndex;
        }
    }
    public void DealDamage(int targetTeam, int baseDamage, int baseHits, RPSTyping attackTyping)
    {
        BaseFighter attacker = GetActiveFighter(1 - targetTeam);
        BaseFighter defender = GetActiveFighter(targetTeam);
        if (attacker == null || defender == null)
        {
            return;
        }
        // damage formula: (base damage + attacker's attack - defender's defense) * # of hits * effectiveness
        int additive = baseDamage + attacker.currentStats.attack - defender.currentStats.defense;
        float mult = baseHits * TypingMultiplier(attackTyping, defender.rpsTyping);
        int finalDamage = Mathf.FloorToInt(additive * mult);
        defender.currentStats.health -= finalDamage;
        logManager.RegisterDamage(targetTeam, finalDamage);
        GD.Print($"P{1 - targetTeam} dealt {finalDamage} damage to P{targetTeam}");
        CheckForDefeat();

    }
    void CheckForDefeat()
    {
        BaseFighter player1Fighter = GetActiveFighter(0);
        if (player1Fighter != null && player1Fighter.currentStats.health <= 0)
        {
            player1Fighter.status = StatusCondition.Dead;
            player1Team.activeFighterIndex = -1;
            logManager.RegisterDeath(0);
        }
        BaseFighter player2Fighter = GetActiveFighter(1);
        if (player2Fighter != null && player2Fighter.currentStats.health <= 0)
        {
            player2Fighter.status = StatusCondition.Dead;
            player2Team.activeFighterIndex = -1;
            logManager.RegisterDeath(1);
        }
        CheckForWinLoss();
    }
    void CheckForWinLoss()
    {
        if (CheckLoss(0))
        {
            MatchEndedEvent.Invoke(0);
        }
        else if (CheckLoss(1))
        {
            MatchEndedEvent.Invoke(1);
        }
    }
    bool CheckLoss(int playerID)
    {
        var team = playerID == 0 ? player1Team.team : player2Team.team;
        for (int i = 0; i < team.Length; i++)
        {
            if (team[i].status != StatusCondition.Dead)
            {
                return false;
            }
        }
        return true;
    }
    public BaseFighter GetActiveFighter(int team)
    {
        if (team == 0)
        {
            int index = player1Team.activeFighterIndex;
            if (index == -1) return null;
            return player1Team.team[index];
        }
        else
        {
            int index = player2Team.activeFighterIndex;
            if (index == -1) return null;
            return player2Team.team[index];
        }
    }
    public bool IsSwapIndexValid(int team, int swapIndex)
    {
        PlayerTeamInfo teamInfo = player1Team;
        if (team != 0)
        {
            teamInfo = player2Team;
        }
        if (teamInfo.activeFighterIndex == swapIndex)
        {
            return false;
        }
        else if (swapIndex < 0 || swapIndex >= teamInfo.team.Length)
        {
            return false;
        }
        else if (teamInfo.team[swapIndex] == null)
        {
            return false;
        }
        return true;
    }
    static float TypingMultiplier(RPSTyping attackType, RPSTyping defendType)
    {
        if (attackType == RPSTyping.Rock && defendType == RPSTyping.Scissors)
        {
            return 1.5f;
        }
        if (attackType == RPSTyping.Paper && defendType == RPSTyping.Rock)
        {
            return 1.5f;
        }
        if (attackType == RPSTyping.Scissors && defendType == RPSTyping.Paper)
        {
            return 1.5f;
        }
        if (attackType == RPSTyping.Rock && defendType == RPSTyping.Paper)
        {
            return 0.5f;
        }
        if (attackType == RPSTyping.Paper && defendType == RPSTyping.Scissors)
        {
            return 0.5f;
        }
        if (attackType == RPSTyping.Scissors && defendType == RPSTyping.Rock)
        {
            return 0.5f;
        }
        return 1f;
    }
}

public class PlayerTeamInfo
{
    public BaseFighter[] team;
    public int activeFighterIndex;
    public PlayerTeamInfo(BaseFighter[] inputTeam)
    {
        team = inputTeam;
        activeFighterIndex = 0;
    }
}