using Godot;
using System;

public partial class ClientFighter : BaseFighter
{
    Sprite2D spriteNode;
    public override void Initialize(FighterData data, ActionData[] actionDatas)
    {
        base.Initialize(data, actionDatas);
        spriteNode = GetNode<Sprite2D>("Sprite2D");
        spriteNode.Texture = data.sprite;
    }
    public void PartialInit(FighterData data)
    {
        // baseStats = new FighterStats
        // {
        //     health = data.Health,
        //     attack = data.Attack,
        //     defense = data.Defense,
        //     speed = data.Speed
        // };
        // currentStats = baseStats;
        // int actionLength = data.actions.Length;
        // actions = new ActionData[actionLength];
        // for (int i = 0; i < actionLength; i++)
        // {
        //     actions[i] = some temp action
        // }
        // rpsTyping = data.fighterTyping;
        // status = StatusCondition.Normal;
        // name = data.Name;
    }
}
