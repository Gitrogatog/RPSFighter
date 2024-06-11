using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.RegularExpressions;

public class ServerRoom
{
    public static int lastRoomID = 0;
    public static Dictionary<int, ServerRoom> roomIDTable = new Dictionary<int, ServerRoom>();
    public static Dictionary<int, int> pIDToRoomID = new Dictionary<int, int>();
    // [Signal] public delegate void AddPlayerToRoomEventHandler(int playerID, int roomID);
    public static event Action<int, int> AddPlayerToRoom = delegate { };
    public static event Action<int> RemovePlayerFromRoom = delegate { };
    public int roomID;
    public int p1ID;
    public int p2ID;
    public ServerBattleManager battleInstance;
    public ChatServer serverHead;
    BaseFighter[] p1Team;
    BaseFighter[] p2Team;
    public TeamJson p1Json;
    public TeamJson p2Json;
    public bool ReadyForMatchStart
    {
        get
        {
            return p1ID != -1 && p2ID != -1 && p1Team != null && p2Team != null;
        }

    }
    public static ServerRoom CreateRoom(ServerBattleManager battleManager)
    {
        lastRoomID++;
        ServerRoom room = new ServerRoom(lastRoomID, battleManager);
        roomIDTable.Add(lastRoomID, room);
        return room;
    }
    public ServerRoom(int roomID, ServerBattleManager battleManager)
    {
        battleInstance = battleManager;
        this.roomID = roomID;
        p1ID = -1;
        p2ID = -1;
        battleInstance.turnManager.TurnEndEvent += EndOfTurn;
        battleInstance.turnManager.DeathSwapEvent += DeathSwap;
        battleInstance.turnManager.MatchEndedEvent += MatchEnd;
    }
    public void CloseRoom()
    {
        if (p1ID != -1)
        {
            RemovePlayerFromRoom.Invoke(p1ID);
            pIDToRoomID.Remove(p1ID);
        }
        if (p2ID != -1)
        {
            RemovePlayerFromRoom.Invoke(p2ID);
            pIDToRoomID.Remove(p2ID);
        }
        battleInstance.QueueFree();
        roomIDTable.Remove(roomID);

    }
    public bool StartMatch()
    {
        if (ReadyForMatchStart)
        {
            battleInstance.StartBattle();
            return true;
        }
        return false;
    }
    public void MatchEnd(int loser)
    {
        serverHead.ReportBattleEnd(this, loser != 0);
    }
    public bool SelectAction(int playerID, int actionIndex)
    {
        if (playerID == p1ID)
        {
            return battleInstance.inputManager.RegisterAction(0, actionIndex);

        }
        else if (playerID == p2ID)
        {
            return battleInstance.inputManager.RegisterAction(1, actionIndex);
        }
        else
        {
            GD.PrintErr($"player {playerID} is not in room!");
        }
        return false;
    }
    public bool SelectSwap(int playerID, int swapIndex)
    {
        if (playerID == p1ID)
        {
            return battleInstance.inputManager.RegisterSwap(0, swapIndex);
        }
        else if (playerID == p2ID)
        {
            return battleInstance.inputManager.RegisterSwap(1, swapIndex);
        }
        else
        {
            GD.PrintErr($"player {playerID} is not in room!");
        }
        return false;
    }

    public bool LoadTeam(int playerID, TeamJson teamData)
    {
        if (p1ID == playerID)
        {
            p1Json = teamData;
            p1Team = CreateTeamFromJson(teamData);
            battleInstance.LoadPlayerTeam(0, p1Team);
            return true;
        }
        else if (p2ID == playerID)
        {
            p2Json = teamData;
            p2Team = CreateTeamFromJson(teamData);
            battleInstance.LoadPlayerTeam(1, p2Team);
            return true;
        }
        return false;
    }
    BaseFighter[] CreateTeamFromJson(TeamJson teamJson)
    {
        BaseFighter[] team = new BaseFighter[teamJson.fighters.Length];
        for (int i = 0; i < team.Length; i++)
        {
            BaseFighter fighter = CreateFighterFromJson(teamJson.fighters[i]);
            team[i] = fighter;
        }
        return team;
    }
    BaseFighter CreateFighterFromJson(FighterJson fighterJson)
    {
        GD.Print($"trying to load fighter with name {fighterJson.Name}");
        FighterData data = ChatServer.globalFighterDictionary.NameToFighterData[fighterJson.Name];
        if (data == null)
        {
            GD.PrintErr($"couldn't find FighterData with name {fighterJson.Name}");
            return null;
        }
        ActionData[] actions = new ActionData[fighterJson.actionNames.Length];
        for (int i = 0; i < actions.Length; i++)
        {
            actions[i] = LoadAction(fighterJson.actionNames[i]);
        }
        return LoadFighter(data, actions);
    }
    ActionData LoadAction(string actionName)
    {
        ActionData data = ChatServer.globalActionDictionary.NameToActionData[actionName];
        if (data == null)
        {
            GD.PrintErr($"couldn't find ActionData with name {actionName}");
            return null;
        }
        return data;
    }
    BaseFighter LoadFighter(FighterData data, ActionData[] actions)
    {
        BaseFighter fighterNode = ChatServer.baseFighterPrefab.Instantiate<BaseFighter>();
        fighterNode.Initialize(data, actions);
        return fighterNode;
    }
    public bool AddPlayer(int playerID)
    {
        GD.Print($"Attempt to add player {playerID} to room {roomID}");
        if (p1ID == -1)
        {
            p1ID = playerID;
            pIDToRoomID.Add(playerID, roomID);
            AddPlayerToRoom.Invoke(playerID, roomID);
            return true;
        }
        else if (p2ID == -1)
        {
            p2ID = playerID;
            pIDToRoomID.Add(playerID, roomID);
            AddPlayerToRoom.Invoke(playerID, roomID);
            return true;
        }
        GD.PrintErr($"Failed to add player {playerID} to room {roomID}");
        return false;
    }
    public bool RemovePlayer(int playerID)
    {
        if (p1ID == playerID)
        {
            p1ID = -1;
            pIDToRoomID.Remove(playerID);
            return true;
        }
        else if (p2ID == playerID)
        {
            p2ID = -1;
            pIDToRoomID.Remove(playerID);
            return true;
        }
        return false;
    }
    public void EndOfTurn()
    {
        serverHead.ReportEndOfTurn(this);
    }
    public void DeathSwap(bool player1Swap, bool player2Swap)
    {
        serverHead.ReportDeathSwap(this, player1Swap, player2Swap);
    }
}