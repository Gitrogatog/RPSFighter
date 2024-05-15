using Godot;
using System;

public partial class ActionData : Resource, IAction
{
    [Export] public int Priority { get; set; }
    public virtual void UseAction()
    {

    }
    public ActionData()
    {

    }
}

public interface IAction
{
    public int Priority { get; }
    public void UseAction();
}

public class SwapAction : IAction
{
    public int Priority { get; set; } = 5;
    public int swapToIndex;
    public void UseAction()
    {

    }
}