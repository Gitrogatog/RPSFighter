using Godot;
using System;

public partial class ClientTeamIconController : Control
{
    [Export] string fighterIconPrefabPath;
    FighterIcon[] fighters;
    public event Action<int> OnSelectSwap = delegate { };
    public void Init(ClientFighter[] team)
    {
        fighters = new FighterIcon[team.Length];
        var iconPrefab = GD.Load<PackedScene>(fighterIconPrefabPath);
        for (int i = 0; i < team.Length; i++)
        {
            FighterIcon icon = iconPrefab.Instantiate<FighterIcon>();
            icon.SetIndex(i, SelectSwap);
            icon.SetFighter(team[i]);
        }
    }
    public void SelectSwap(int index)
    {
        OnSelectSwap.Invoke(index);
    }
}
