using Godot;
using System;

public partial class HealthBar : Control
{
    Control health;
    RichTextLabel label;
    int maxHealth;
    int minHealth;
    public override void _Ready()
    {
        health = GetNode<Control>("Health");
        label = GetNode<RichTextLabel>("HealthText");
    }
    public void SetMaxHealth(int maxHealth)
    {
        this.maxHealth = maxHealth;
        minHealth = maxHealth;
        label.Text = $"{minHealth} / {maxHealth}";
    }
    public void UpdateHealth(int newHealth)
    {
        minHealth = newHealth;
        health.Scale = new Vector2((float)minHealth / maxHealth, health.Scale.Y);
        label.Text = $"{minHealth} / {maxHealth}";
    }
}

