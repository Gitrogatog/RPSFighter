using Godot;
using System;

public partial class ActionButtonController : VBoxContainer
{
    ActionButton[] actionButtons;
    [Export] string actionButtonPrefabPath;
    [Export] int numActionButtons = 3;
    public event Action<int> OnSelectAction = delegate { };

    public override void _EnterTree()
    {
        var btnPrefab = GD.Load<PackedScene>(actionButtonPrefabPath);
        actionButtons = new ActionButton[numActionButtons];
        for (int i = 0; i < numActionButtons; i++)
        {
            ActionButton btn = btnPrefab.Instantiate<ActionButton>();
            btn.SetIndex(i, SelectAction);
        }
    }
    public void SelectAction(int actionIndex)
    {
        OnSelectAction.Invoke(actionIndex);
    }
}
