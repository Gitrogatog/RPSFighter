using Godot;
using System;

public partial class BaseFighter : Node
{
    public FighterStats baseStats;
    public FighterStats currentStats;
    [Export] public ActionData[] actions;
    [Export] public RPSTyping rpsTyping;
    [Export] public StatusCondition status;
    [Export] public string name;
    public virtual void Initialize(FighterData data)
    {
        baseStats = new FighterStats
        {
            health = data.Health,
            attack = data.Attack,
            defense = data.Defense,
            speed = data.Speed
        };
        currentStats = baseStats;
        int actionLength = data.actions.Length;
        actions = new ActionData[actionLength];
        for (int i = 0; i < actionLength; i++)
        {
            actions[i] = data.actions[i];
        }
        rpsTyping = data.fighterTyping;
        status = StatusCondition.Normal;
        name = data.Name;
        // spriteNode = GetNodeOrNull<Sprite2D>("Sprite2D");
        // if (spriteNode != null)
        // {
        //     spriteNode.Texture = data.sprite;
        // }
    }
    public void PrintState()
    {
        GD.Print($"Name: {name}");
        GD.Print($"Base stats: hp: {baseStats.health}, att: {baseStats.attack}, def: {baseStats.defense}, spd: {baseStats.speed}");
        GD.Print($"Current stats: hp: {currentStats.health}, att: {currentStats.attack}, def: {currentStats.defense}, spd: {currentStats.speed}");

    }
}

public struct FighterStats
{
    public int health, attack, defense, speed;
}

public enum RPSTyping
{
    Rock, Paper, Scissors
}

public enum StatusCondition
{
    Normal, Dead
}