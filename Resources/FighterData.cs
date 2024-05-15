using Godot;
using System;

public partial class FighterData : Resource
{
    [Export] public int Health;
    [Export] public int Attack;
    [Export] public int Defense;
    [Export] public int Speed;
    [Export] RPSTyping fighterTyping;
    public FighterData()
    {

    }
}
