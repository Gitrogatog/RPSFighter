using Godot;
using System;

public partial class ActionButtonController : VBoxContainer
{
    ActionButton[] actionButtons;
    [Export] string actionButtonPrefabPath;
    [Export] int numActionButtons = 3;
    public event Action<int> OnSelectAction = delegate { };
    bool initialized = false;

    public override void _EnterTree()
    {

    }
    void Initialize()
    {
        if (!initialized)
        {
            initialized = true;
            GD.Print("Action button controller is initializing");
            var btnPrefab = GD.Load<PackedScene>(actionButtonPrefabPath);
            actionButtons = new ActionButton[numActionButtons];
            for (int i = 0; i < numActionButtons; i++)
            {
                ActionButton btn = btnPrefab.Instantiate<ActionButton>();
                actionButtons[i] = btn;
                btn.SetIndex(i, SelectAction);
                AddChild(btn);
            }
        }
    }
    public void UpdateButtonActions(ActionData[] actions)
    {
        Initialize();
        for (int i = 0; i < actionButtons.Length; i++)
        {
            if (actions.Length > i)
            {
                actionButtons[i].SetAction(actions[i]);
                actionButtons[i].SetActive(true);
            }
            else
            {
                actionButtons[i].SetActive(false);
            }
        }
    }
    public void DisableButtons()
    {
        for (int i = 0; i < actionButtons.Length; i++)
        {
            actionButtons[i].SetActive(false);
        }
    }
    public void SelectAction(int actionIndex)
    {
        GD.Print($"Selected action: {actionIndex}");
        OnSelectAction.Invoke(actionIndex);
    }
}
