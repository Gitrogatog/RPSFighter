using Godot;
using System;

public partial class AttackActionData : ActionData
{
    [Export] public int Damage;
    [Export] public int Hits = 1;
    [Export] RPSTyping typing;
    public override void UseAction(int playerIndex, ServerTurnManager turnManager)
    {
        turnManager.DealDamage(1 - playerIndex, Damage, Hits, typing);
    }
    public AttackActionData()
    {

    }
}