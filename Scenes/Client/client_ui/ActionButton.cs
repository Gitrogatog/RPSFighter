using Godot;
using System;

public partial class ActionButton : Button
{
    int actionIndex;
    public Action<int> OnClickEvent;
    public void SetIndex(int index, Action<int> action)
    {
        actionIndex = index;
        OnClickEvent = action;
    }
    public void SetAction(ActionData actionData)
    {
        Text = actionData.Name;
    }
    public void OnClick()
    {
        OnClickEvent.Invoke(actionIndex);
    }
}
