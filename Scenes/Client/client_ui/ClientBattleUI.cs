using Godot;
using System;

public partial class ClientBattleUI : Control
{
    public event Action<int> OnSelectAction;
    public event Action<int> OnSelectSwap;
    [Export] HealthBar playerHealthBar;
    [Export] HealthBar enemyHealthBar;
    [Export] ActionButtonController actionController;
    [Export]
    ClientTeamIconController playerIcons;
    [Export]
    ClientTeamIconController enemyIcons;
    [Export] RichTextLabel textLog;
    ClientFighter[] playerFighters;
    ClientFighter[] enemyFighters;
    int activePlayerIndex = 0;
    int activeEnemyIndex = 0;
    ActionButton[] actionButtons;

    public void InitBattle(ClientFighter[] playerFighters, ClientFighter[] enemyFighters)
    {
        this.playerFighters = playerFighters;
        this.enemyFighters = enemyFighters;
        actionController.OnSelectAction += SelectAction;
        playerIcons.Init(playerFighters);
        playerIcons.OnSelectSwap += SelectSwap;
        enemyIcons.Init(enemyFighters);
    }
    public void SelectAction(int index)
    {
        // EmitSignal(SignalName.OnSelectAction, index);
        OnSelectAction.Invoke(index);
    }
    public void SelectSwap(int index)
    {
        // EmitSignal(SignalName.OnSelectSwap, index);
        OnSelectSwap.Invoke(index);
    }
    // all possible battle updates
    // change health
    // swap out
    // use action (?)
    // change stats
    // die
    public void SetExpectedResponse(ExpectedActionResponse response)
    {
        switch (response)
        {
            case ExpectedActionResponse.Any:
                break;
            case ExpectedActionResponse.Swap:
                break;
            case ExpectedActionResponse.None:
                break;
        }
    }
    public void RunAction(bool myTeam, string actionID)
    {
        textLog.AddText($"{(myTeam ? "Enemy" : "You")} used action {actionID}\n");
    }
    public void RunSwap(bool myTeam, int swapToIndex, string fighterID)
    {
        if (myTeam)
        {

        }
    }
    public void RunDamage(bool myTeam, int damage)
    {

    }
    public void RunDeath(bool myTeam)
    {

    }
    public void RunStartTurn()
    {

    }
    public void RunEndTurn()
    {

    }
}
