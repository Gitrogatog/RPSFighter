using Godot;
using System;
using System.Collections.Generic;

public partial class ServerInputManager : Node
{
    [Export] ServerTurnManager battleManager;
    IAction player1Action;
    IAction player2Action;
    public bool RegisterAction(int playerIndex, int actionIndex)
    {
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
        if (!battleManager.IsSwapIndexValid(playerIndex, swapIndex)) return false;
        // battleManager.AddTurn(playerIndex, new SwapAction(swapIndex));
        SetHeldTurn(playerIndex, new SwapAction(swapIndex));
        return true;
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
        if (player1Action != null && player2Action != null)
        {
            SendTurnsToBattleManager();
        }
    }
    void SendTurnsToBattleManager()
    {
        battleManager.RunTurn(player1Action, player2Action);
        player1Action = null;
        player2Action = null;
    }
}