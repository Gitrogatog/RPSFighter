using Godot;
using System;

public partial class FighterDictionary : Resource
{
    [Export]
    public Godot.Collections.Dictionary<string, FighterData> NameToFighterData { get; set; }
}
