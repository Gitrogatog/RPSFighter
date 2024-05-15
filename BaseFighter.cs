using Godot;
using System;

public partial class BaseFighter : Node
{
    public FighterStats baseStats;
    public FighterStats currentStats;
    public RPSTyping rpsTyping;
    public StatusCondition status;
}

public class FighterStats
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