using Godot;
using System;

public partial class TestSpinboxUI : Control
{
    [Export] SpinBox spinBox;
    [Export] int playerIndex;
    [Export] ServerInputManager inputManager;
    public void SelectAction()
    {
        int actionIndex = (int)(spinBox.Value);
        GD.Print($"Registering action with player index {playerIndex} and action ID {actionIndex}");
        bool success = inputManager.RegisterAction(playerIndex, actionIndex);
        if (!success)
        {
            GD.PrintErr("Input manager rejected action input!");
        }
    }
    public void SelectSwap()
    {
        int swapIndex = (int)(spinBox.Value);
        GD.Print($"Registering swap with player index {playerIndex} and swap ID {swapIndex}");
        bool success = inputManager.RegisterSwap(playerIndex, swapIndex);
        if (!success)
        {
            GD.PrintErr("Input manager rejected swap input!");
        }
    }
}
