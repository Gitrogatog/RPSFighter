using Godot;
using System;
using System.ComponentModel.DataAnnotations.Schema;

public partial class ClientBattleUI : Control
{
    public event Action<int> OnSelectAction = delegate { };
    public event Action<int> OnSelectSwap = delegate { };
    [Export] HealthBar playerHealthBar;
    [Export] HealthBar enemyHealthBar;
    [Export] ActionButtonController actionController;
    [Export]
    ClientTeamIconController playerIcons;
    [Export]
    ClientTeamIconController enemyIcons;
    [Export] RichTextLabel textLog;
    [Export] Sprite2D playerSprite;
    [Export] Sprite2D enemySprite;
    ClientFighter[] playerFighters;
    ClientFighter[] enemyFighters;
    int activePlayerIndex = 0;
    int activeEnemyIndex = 0;
    ExpectedActionResponse currentResponse = ExpectedActionResponse.Any;
    public void InitBattle(ClientFighter[] playerFighters, ClientFighter[] enemyFighters)
    {
        this.playerFighters = playerFighters;
        this.enemyFighters = enemyFighters;
        actionController.OnSelectAction += SelectAction;
        actionController.UpdateButtonActions(playerFighters[0].actions);
        GD.Print("Initing player icons");
        playerIcons.Init(playerFighters);
        playerIcons.OnSelectSwap += SelectSwap;
        GD.Print("Initing enemy icons");
        enemyIcons.Init(enemyFighters);

        playerSprite.Texture = playerFighters[0].Data.sprite;
        enemySprite.Texture = enemyFighters[0].Data.sprite;
    }
    public void SelectAction(int index)
    {
        // EmitSignal(SignalName.OnSelectAction, index);
        OnSelectAction.Invoke(index);
    }
    public void SelectSwap(int index)
    {
        // EmitSignal(SignalName.OnSelectSwap, index);
        if (activePlayerIndex != index)
        {
            OnSelectSwap.Invoke(index);
        }

    }
    // all possible battle updates
    // change health
    // swap out
    // use action (?)
    // change stats
    // die
    public void SetExpectedResponse(ExpectedActionResponse response)
    {
        currentResponse = response;
        switch (response)
        {
            case ExpectedActionResponse.Any:
                GD.Print("Expecting any action!");
                AddLog("Expecting any action!");
                ClientFighter fighter = playerFighters[activePlayerIndex];
                actionController.UpdateButtonActions(fighter.actions);
                break;
            case ExpectedActionResponse.Swap:
                GD.Print("Expecting swap");
                AddLog("Expecting swap");
                actionController.DisableButtons();
                break;
            case ExpectedActionResponse.None:
                GD.Print("Wait for response");
                AddLog("Wait for response");
                actionController.DisableButtons();
                break;
        }
    }
    public void RunAction(bool myTeam, string actionID)
    {
        AddLog($"{(myTeam ? "Enemy" : "You")} used action {actionID}");
    }
    public void RunSwap(bool myTeam, int swapToIndex, string fighterID)
    {
        if (myTeam)
        {
            SwapActivePlayer(swapToIndex, fighterID);
        }
        else
        {
            SwapActiveEnemy(swapToIndex, fighterID);
        }
    }
    void SwapActivePlayer(int player, string fighterID)
    {
        AddLog($"You swapped to {fighterID}");
        ClientFighter fighter = playerFighters[player];
        playerSprite.Texture = fighter.Data.sprite;
        UpdatePlayerHealth(fighter.currentStats.health, fighter.baseStats.health);

        activePlayerIndex = player;

    }
    void SwapActiveEnemy(int enemy, string fighterID)
    {
        AddLog($"Enemy swapped to {fighterID}");
        ClientFighter fighter = enemyFighters[enemy];
        if (!fighter.fighterRevealed)
        {
            FighterData fData = DataGlobals.globalFighterDictionary.NameToFighterData[fighterID];
            fighter.PartialInit(fData);
            enemyIcons.RevealIcon(enemy, fighter);
        }
        enemySprite.Texture = fighter.Data.sprite;
        UpdateEnemyHealth(fighter.currentStats.health, fighter.baseStats.health);

        activeEnemyIndex = enemy;
    }
    void UpdatePlayerHealth(int current, int max)
    {
        // playerHealthBar.SetMaxHealth(max);
        playerHealthBar.UpdateHealth(current, max);
    }
    void UpdateEnemyHealth(int current, int max)
    {
        // enemyHealthBar.SetMaxHealth(max);
        enemyHealthBar.UpdateHealth(current, max);
    }
    public void RunDamage(bool myTeam, int damage)
    {
        if (myTeam)
        {
            ClientFighter fighter = playerFighters[activePlayerIndex];
            AddLog($"Your {fighter.name} took {damage} damage");
            fighter.currentStats.health = Math.Clamp(fighter.currentStats.health - damage, 0, fighter.baseStats.health);
            playerHealthBar.UpdateHealth(fighter.currentStats.health, fighter.baseStats.health);
            playerIcons.fighters[activePlayerIndex].UpdateHealth(fighter.currentStats.health, fighter.baseStats.health);
        }
        else
        {
            ClientFighter fighter = enemyFighters[activeEnemyIndex];
            AddLog($"Enemy {fighter.name} took {damage} damage");
            fighter.currentStats.health = Math.Clamp(fighter.currentStats.health - damage, 0, fighter.baseStats.health);
            enemyHealthBar.UpdateHealth(fighter.currentStats.health, fighter.baseStats.health);
            enemyIcons.fighters[activeEnemyIndex].UpdateHealth(fighter.currentStats.health, fighter.baseStats.health);
        }
    }
    public void RunDeath(bool myTeam)
    {
        if (myTeam)
        {
            ClientFighter fighter = playerFighters[activePlayerIndex];
            fighter.status = StatusCondition.Dead;
            playerIcons.fighters[activePlayerIndex].isAlive = false;
            playerSprite.Texture = null;
            AddLog($"Your {fighter.name} died!");
        }
        else
        {
            ClientFighter fighter = enemyFighters[activeEnemyIndex];
            fighter.status = StatusCondition.Dead;
            enemyIcons.fighters[activeEnemyIndex].isAlive = false;
            enemySprite.Texture = null;
            AddLog($"Enemy {fighter.name} died!");
        }
    }
    public void RunStartTurn()
    {
        AddLog("Turn Start");
    }
    public void RunEndTurn()
    {
        AddLog("Turn End");
    }
    public void AddLog(string log)
    {
        textLog.AddText($"{log}\n");
    }
}
