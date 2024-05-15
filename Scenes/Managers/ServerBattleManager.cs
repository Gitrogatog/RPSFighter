using Godot;
using System;
using System.Collections.Generic;

public partial class ServerBattleManager : Node
{
    [Export] ServerLogManager logManager;
    PlayerTeamInfo player1Team;
    PlayerTeamInfo player2Team;
    IAction[] actionQueue = new IAction[2];

    void RunTurn(IAction player1Action, IAction player2Action)
    {
        // RunStartTurnEffects();
        DecideTurnOrder(player1Action, player2Action);
        actionQueue[0].UseAction();
        actionQueue[1].UseAction();
        RunEndOfTurnEffects();
        if (player1Team.activeFighterIndex == -1)
        {
            // expect swap input
        }
        if (player2Team.activeFighterIndex == -1)
        {
            // expect swap input 
        }
        // await
        EndTurn();
    }

    void RunStartTurnEffects()
    {

    }

    void RunEndOfTurnEffects()
    {

    }
    void EndTurn()
    {

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

    void Swap(int userTeam, int swapToIndex)
    {
        PlayerTeamInfo team = userTeam == 0 ? player1Team : player2Team;
        if (team.activeFighterIndex != swapToIndex)
        {
            logManager.RegisterSwap(userTeam, swapToIndex);
            team.activeFighterIndex = swapToIndex;
        }
    }
    void DealDamage(int targetTeam, int baseDamage, int baseHits, RPSTyping attackTyping)
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
    public List<BaseFighter> team;
    public int activeFighterIndex;
}