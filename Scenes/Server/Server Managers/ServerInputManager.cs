using Godot;
using System;
using System.Collections.Generic;

public partial class ServerInputManager : Node
{
    [Export] ServerTurnManager battleManager;
    IAction player1Action;
    IAction player2Action;
    bool waitingToReset = false;
    ExpectedActionResponse p1Input = ExpectedActionResponse.Any;
    ExpectedActionResponse p2Input = ExpectedActionResponse.Any;
    public override void _Ready()
    {
        battleManager.DeathSwapEvent += OnDeathSwap;
        battleManager.TurnEndEvent += OnTurnEnd;
    }

    public bool RegisterAction(int playerIndex, int actionIndex)
    {
        if (p1Input != ExpectedActionResponse.Any && playerIndex == 0) return false;
        if (p2Input != ExpectedActionResponse.Any && playerIndex == 1) return false;
        BaseFighter fighter = battleManager.GetActiveFighter(playerIndex);
        if (fighter == null) return false;
        if (actionIndex < 0 || actionIndex >= fighter.actions.Length)
        {
            return false;
        }
        // battleManager.AddTurn(playerIndex, fighter.actions[actionIndex]);
        SetHeldTurn(playerIndex, fighter.actions[actionIndex]);
        return true;
    }
    public bool RegisterSwap(int playerIndex, int swapIndex)
    {
        if (p1Input == ExpectedActionResponse.None && playerIndex == 0) return false;
        if (p2Input == ExpectedActionResponse.None && playerIndex == 1) return false;
        if (!battleManager.IsSwapIndexValid(playerIndex, swapIndex)) return false;
        // battleManager.AddTurn(playerIndex, new SwapAction(swapIndex));
        SetHeldTurn(playerIndex, new SwapAction(swapIndex));
        return true;
    }
    void OnTurnEnd()
    {
        p1Input = ExpectedActionResponse.Any;
        p2Input = ExpectedActionResponse.Any;
        player1Action = null;
        player2Action = null;
    }
    void OnDeathSwap(bool p1Dead, bool p2Dead)
    {
        // waitingToReset = false;
        p1Input = p1Dead ? ExpectedActionResponse.Swap : ExpectedActionResponse.None;
        p2Input = p2Dead ? ExpectedActionResponse.Swap : ExpectedActionResponse.None;
        player1Action = p1Dead ? null : new BlankAction();
        player2Action = p2Dead ? null : new BlankAction();
        GD.Print($"p1action: {player1Action != null} p2Action: {player2Action != null}");
        if (!p1Dead && !p2Dead)
        {
            GD.PushError("server input manager found both players alive on death swap!");
        }
    }
    // void RegisterSelectedAction(int playerIndex, IAction action)
    // {
    //     if (action is ActionData actionData)
    //     {

    //     }
    // }
    void SetHeldTurn(int playerIndex, IAction action)
    {
        if (playerIndex == 0)
        {
            player1Action = action;
        }
        else
        {
            player2Action = action;
        }
        GD.Print($"p1action: {player1Action != null} p2Action: {player2Action != null}");
        if (player1Action != null && player2Action != null)
        {
            SendTurnsToBattleManager();
        }
    }
    void SendTurnsToBattleManager()
    {
        battleManager.RunActions(player1Action, player2Action);
    }
}

