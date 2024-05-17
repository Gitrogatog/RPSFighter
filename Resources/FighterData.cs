using Godot;
using System;

public partial class FighterData : Resource
{
    [Export] public int Health;
    [Export] public int Attack;
    [Export] public int Defense;
    [Export] public int Speed;
    [Export] public RPSTyping fighterTyping;
    [Export] public ActionData[] actions;
    [Export] public string Name;
    [Export] public CompressedTexture2D sprite;
    public FighterData()
    {

    }
}
