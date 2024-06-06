using Godot;
using System;

public partial class FighterIcon : Control
{
    int teamIndex;
    Button button;
    RichTextLabel healthLabel;
    public Action<int> OnClickEvent;
    public bool isAlive = true;
    public bool isSelectable = true;
    public bool isRevealed = false;
    bool initialized = false;
    public override void _EnterTree()
    {
        Initialize();
    }
    void Initialize()
    {
        if (!initialized)
        {
            initialized = true;
            // sprite = GetNode<TextureRect>("FighterSprite");
            button = GetNode<Button>("Button");
            healthLabel = GetNode<RichTextLabel>("HealthLabel");
        }
    }
    public void SetIndex(int index, Action<int> action)
    {
        teamIndex = index;
        OnClickEvent = action;
    }
    public void SetFighter(BaseFighter fighter)
    {
        Initialize();
        button.Icon = fighter.Data.sprite;
        UpdateHealth(fighter.currentStats.health, fighter.baseStats.health);
    }
    public void SetBlank()
    {
        button.Icon = null;
        healthLabel.Text = "";
    }

    public void UpdateHealth(int current, int max)
    {
        healthLabel.Text = $"{current} / {max}";
    }
    public void OnClick()
    {
        if (isAlive && isSelectable)
        {
            OnClickEvent.Invoke(teamIndex);
        }

    }
}
