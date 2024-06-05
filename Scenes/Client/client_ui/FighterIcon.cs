using Godot;
using System;

public partial class FighterIcon : Control
{
    int teamIndex;
    TextureRect sprite;
    RichTextLabel healthLabel;
    public Action<int> OnClickEvent;
    public override void _Ready()
    {
        sprite = GetNode<TextureRect>("FighterSprite");
        healthLabel = GetNode<RichTextLabel>("HealthLabel");
    }
    public void SetIndex(int index, Action<int> action)
    {
        teamIndex = index;
        OnClickEvent = action;
    }
    public void SetFighter(BaseFighter fighter)
    {
        sprite.Texture = fighter.Data.sprite;
        UpdateHealth(fighter.currentStats.health, fighter.baseStats.health);
    }

    public void UpdateHealth(int current, int max)
    {
        healthLabel.Text = $"{current} / {max}";
    }
    public void OnClick()
    {
        OnClickEvent.Invoke(teamIndex);
    }
}
