using Godot;
using System;

public partial class ActionButton : Button
{
    int actionIndex;
    public Action<int> OnClickEvent;
    public bool isActive = false;
    public void SetActive(bool active)
    {
        isActive = active;
        Visible = active;
    }
    public void SetIndex(int index, Action<int> action)
    {
        isActive = true;
        actionIndex = index;
        OnClickEvent = action;
    }
    public void SetAction(ActionData actionData)
    {
        Text = actionData.Name;
    }
    public void OnClick()
    {
        GD.Print("button registered click!");

        if (isActive)
        {
            OnClickEvent.Invoke(actionIndex);
        }

    }
}
