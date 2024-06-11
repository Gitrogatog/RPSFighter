using Godot;
using System;

public partial class ClientTeamIconController : Control
{
    [Export] string fighterIconPrefabPath;
    public FighterIcon[] fighters;
    public event Action<int> OnSelectSwap = delegate { };
    public void Init(ClientFighter[] team)
    {
        fighters = new FighterIcon[team.Length];
        PackedScene iconPrefab = GD.Load<PackedScene>(fighterIconPrefabPath);
        for (int i = 0; i < team.Length; i++)
        {
            FighterIcon icon = iconPrefab.Instantiate<FighterIcon>();
            icon.SetIndex(i, SelectSwap);
            GD.Print($"index: {i}, revealed: {team[i].fighterRevealed} data: {team[i].Data != null}");
            AddChild(icon);
            fighters[i] = icon;
            if (team[i].Data != null)
            {

                icon.SetFighter(team[i]);
            }
            else
            {
                icon.SetBlank();
            }
        }
    }
    public void RevealIcon(int index, ClientFighter fighter)
    {
        fighters[index].SetFighter(fighter);
    }
    public void SelectSwap(int index)
    {
        OnSelectSwap.Invoke(index);
    }
}
