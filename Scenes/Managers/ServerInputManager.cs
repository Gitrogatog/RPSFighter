using Godot;
using System;
using System.Collections.Generic;

public partial class ServerInputManager : Node
{
    [Export] ServerBattleManager battleManager;
    IAction player1Action;
    IAction player2Action;
    bool RegisterAction(int playerIndex, int actionIndex)
    {
        BaseFighter fighter = battleManager.GetActiveFighter(playerIndex);
        if (fighter == null) return false;
    }
    bool RegisterSwap(int playerIndex, int swapIndex)
    {

    }
    void RegisterSelectedAction(int playerIndex, IAction action)
    {
        if (action is ActionData actionData)
        {

        }
    }
}