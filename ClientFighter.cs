using Godot;
using System;

public partial class ClientFighter : BaseFighter
{
    // Sprite2D spriteNode;
    // Texture2D 
    public bool fighterRevealed = false;
    public override void Initialize(FighterData data, ActionData[] actionDatas)
    {
        base.Initialize(data, actionDatas);
        fighterRevealed = true;
        // spriteNode = GetNode<Sprite2D>("Sprite2D");
        // spriteNode.Texture = data.sprite;
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
        // int actionLength = data.actions.Length;
        // actions = new ActionData[actionLength];
        // for (int i = 0; i < actionLength; i++)
        // {
        //     actions[i] = some temp action
        // }
        actions = new ActionData[3];
        rpsTyping = data.fighterTyping;
        status = StatusCondition.Normal;
        name = data.Name;
        Data = data;
        fighterRevealed = true;
    }
}
