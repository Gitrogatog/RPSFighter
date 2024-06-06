using Godot;
using System;

public partial class DataGlobals : Node
{
    [Export] ActionDictionary actionDictionary;
    [Export] FighterDictionary fighterDictionary;
    public static ActionDictionary globalActionDictionary;
    public static FighterDictionary globalFighterDictionary;
    public static PackedScene baseFighterPrefab;
    public static PackedScene clientFighterPrefab;
    public static TeamJson defaultTeam;
    public override void _EnterTree()
    {
        globalActionDictionary = actionDictionary;
        globalFighterDictionary = fighterDictionary;
        baseFighterPrefab = GD.Load<PackedScene>("res://Scenes/Fighters/base_fighter.tscn");
        clientFighterPrefab = GD.Load<PackedScene>("res://Scenes/Fighters/client_fighter.tscn");
        FighterJson[] fighterSet = new FighterJson[3];
        fighterSet[0] = new FighterJson
        {
            Name = "rock man",
            actionNames = new string[] { "rock punch", "paper punch", "scissor punch" }
        };
        fighterSet[1] = new FighterJson
        {
            Name = "paper man",
            actionNames = new string[] { "paper punch", "scissor punch" }
        };
        fighterSet[2] = new FighterJson
        {
            Name = "scissor man",
            actionNames = new string[] { "scissor punch", "scissor slice" }
        };
        defaultTeam = new TeamJson
        {
            fighters = fighterSet
        };
    }
}
