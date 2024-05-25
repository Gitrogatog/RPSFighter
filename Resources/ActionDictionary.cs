using Godot;
using System;

public partial class ActionDictionary : Resource
{
    [Export]
    public Godot.Collections.Dictionary<string, ActionData> NameToActionData { get; set; }
}
