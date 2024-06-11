using Godot;
using System;

public partial class ClientFighter : BaseFighter
{
    public bool fighterRevealed = false;
    public override void Initialize(FighterData data, ActionData[] actionDatas)
    {
        base.Initialize(data, actionDatas);
        fighterRevealed = true;
    }
    public void PartialInit(FighterData data) //doesn't set the held actions
    {
        baseStats = new FighterStats
        {
            health = data.Health,
            attack = data.Attack,
            defense = data.Defense,
            speed = data.Speed
        };
        currentStats = baseStats;
        actions = new ActionData[3];
        rpsTyping = data.fighterTyping;
        status = StatusCondition.Normal;
        name = data.Name;
        Data = data;
        fighterRevealed = true;
    }
}
