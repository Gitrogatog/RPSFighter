using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;

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
    public bool ReadyForMatchStart
    {
        get
        {
            return p1ID != -1 && p2ID != -1 && p1Team != null && p2Team != null;
        }

    }
    public static ServerRoom CreateRoom()
    {
        lastRoomID++;
        ServerRoom room = new ServerRoom(lastRoomID);
        roomIDTable.Add(lastRoomID, room);
        return room;
    }
    public ServerRoom(int roomID)
    {
        this.roomID = roomID;
        p1ID = -1;
        p2ID = -1;

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
        roomIDTable.Remove(roomID);

    }

    public bool LoadTeam(int playerID, string teamData)
    {
        TeamJson teamJson = JsonSerializer.Deserialize<TeamJson>(teamData);
        if (teamJson == null)
        {
            GD.PrintErr("Failed to read team data");
            return false;
        }
        return true;
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
}