using Godot;
using System;

public partial class ActionData : Resource, IAction
{
    [Export] public string Name;
    [Export] public int Priority { get; set; }

    public virtual void UseAction(int playerIndex, ServerTurnManager turnManager)
    {

    }
    public ActionData()
    {

    }
}

public interface IAction
{
    public int Priority { get; }
    public void UseAction(int playerIndex, ServerTurnManager turnManager);
}

public class SwapAction : IAction
{
    public int Priority { get; set; } = 5;
    public int SwapToIndex;
    public void UseAction(int playerIndex, ServerTurnManager turnManager)
    {
        turnManager.Swap(playerIndex, SwapToIndex);
    }
    public SwapAction(int swapIndex)
    {
        SwapToIndex = swapIndex;
    }
}