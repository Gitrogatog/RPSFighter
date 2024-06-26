using System.Collections.Generic;
using Godot;
using System.Text.Json;

public partial class ChatServer : Control
{
    [Export] WebSocketServer _server;
    [Export] RichTextLabel _logDest;
    [Export] LineEdit _lineEdit;
    [Export] SpinBox _port;
    TeamJson defaultTeam;
    [Export] public FighterDictionary fighterDictionary;
    [Export] public ActionDictionary actionDictionary;
    public static FighterDictionary globalFighterDictionary;
    public static ActionDictionary globalActionDictionary;
    public static PackedScene baseFighterPrefab;
    public static PackedScene serverBattleInstancePrefab;

    public override void _Ready()
    {
        globalFighterDictionary = fighterDictionary;
        globalActionDictionary = actionDictionary;
        baseFighterPrefab = GD.Load<PackedScene>("res://Scenes/Fighters/base_fighter.tscn");
        serverBattleInstancePrefab = GD.Load<PackedScene>("res://Scenes/Server/server_battle_scene.tscn");
        FighterJson[] fighterSet = new FighterJson[3];
        fighterSet[0] = new FighterJson
        {
            Name = "rock man",
            actionNames = new string[] { "rock punch", "paper punch", "scissor punch" }
        };
        fighterSet[1] = new FighterJson
        {
            Name = "paper man",
            actionNames = new string[] { "paper punch", "scissor punch" }
        };
        fighterSet[2] = new FighterJson
        {
            Name = "scissor man",
            actionNames = new string[] { "scissor punch", "scissor slice" }
        };
        defaultTeam = new TeamJson
        {
            fighters = fighterSet
        };
    }

    public void Info(string message)
    {
        GD.Print(message);
        _logDest.AddText(message + "\n");
    }

    // Server signals
    void OnWebSocketServerClientConnected(int peerId)
    {
        WebSocketPeer peer = _server.peers[peerId];
        Info($"Remote client connected {peerId}, protocol {peer.GetSelectedProtocol()}");
        _server.Send(-peerId, $"{peerId} connected");
    }
    void OnWebSocketServerClientDisconnected(int peerId)
    {
        WebSocketPeer peer = _server.peers[peerId];
        Info($"Remote client {peerId} disconnected");
        _server.Send(-peerId, $"{peerId} disconnected");
    }
    void OnWebSocketServerMessageReceived(int peerId, string message)
    {
        ProcessClientRequest(peerId, message);
        Info($"Server received data from peer {peerId}: {message}");
    }

    void ProcessClientRequest(int peerID, string message)
    {
        GD.Print($"server got message: {message}");
        ClientToServerMessage messageData = JsonSerializer.Deserialize<ClientToServerMessage>(message);
        GD.Print($"messageData type: {messageData.messageType}, sender id: {peerID}, messageData data: {messageData.data}");
        switch (messageData.messageType)
        {
            case ClientToServerMessageType.CreateRoom:
                CreateRoomRequest(peerID);
                break;
            case ClientToServerMessageType.JoinRoom:
                int roomID = messageData.data.ToInt();
                JoinRoomRequest(peerID, roomID);
                break;
            case ClientToServerMessageType.ExitRoom:
                ExitRoomRequest(peerID);
                break;
            case ClientToServerMessageType.TeamData:
                GetTeamRequest(peerID, messageData.data);
                break;
            case ClientToServerMessageType.Action:
                SelectAction(peerID, messageData.data.ToInt());
                break;
            case ClientToServerMessageType.Swap:
                SelectSwap(peerID, messageData.data.ToInt());
                break;
                // case ClientToServerMessageType.:
                //     break;
        }
    }

    ServerRoom GetPeerRoom(int peerID)
    {
        if (!ServerRoom.pIDToRoomID.ContainsKey(peerID))
        {
            SendMessage(peerID, ServerToClientMessageType.Error, "You're not in a room!");
            return null;
        }
        return ServerRoom.roomIDTable[ServerRoom.pIDToRoomID[peerID]];
    }

    void SelectAction(int peerID, int actionID)
    {
        ServerRoom room = GetPeerRoom(peerID);
        if (room == null) return;
        // int actionID = actionData.ToInt();
        if (!room.SelectAction(peerID, actionID))
        {
            SendMessage(peerID, ServerToClientMessageType.Error, "action input failed!");
        }
    }
    void SelectSwap(int peerID, int swapID)
    {
        ServerRoom room = GetPeerRoom(peerID);
        if (room == null) return;
        // int swapID = swapData.ToInt();
        if (!room.SelectSwap(peerID, swapID))
        {
            SendMessage(peerID, ServerToClientMessageType.Error, "swap input failed!");
        }
    }

    public void ReportEndOfTurn(ServerRoom room)
    {
        var logs = room.battleInstance.logManager.GetLog();
        BattleLogMessage logMessage = new BattleLogMessage(ExpectedActionResponse.Any, logs);
        string messageJson = JsonSerializer.Serialize(logMessage);
        SendMessage(room.p1ID, ServerToClientMessageType.BattleLog, messageJson);
        SendMessage(room.p2ID, ServerToClientMessageType.BattleLog, messageJson);
    }
    public void ReportDeathSwap(ServerRoom room, bool p1Swap, bool p2Swap)
    {
        var logs = room.battleInstance.logManager.GetLog();
        BattleLogMessage p1LogMessage = new BattleLogMessage(p1Swap ? ExpectedActionResponse.Swap : ExpectedActionResponse.None, logs);
        BattleLogMessage p2LogMessage = new BattleLogMessage(p2Swap ? ExpectedActionResponse.Swap : ExpectedActionResponse.None, logs);
        string p1MessageJson = JsonSerializer.Serialize(p1LogMessage);
        string p2MessageJson = JsonSerializer.Serialize(p2LogMessage);
        SendMessage(room.p1ID, ServerToClientMessageType.BattleLog, p1MessageJson);
        SendMessage(room.p2ID, ServerToClientMessageType.BattleLog, p2MessageJson);
    }

    public void ReportBattleEnd(ServerRoom room, bool p1Win)
    {
        int winnerID = p1Win ? room.p1ID : room.p2ID;
        int loserID = p1Win ? room.p2ID : room.p1ID;
        SendMessage(winnerID, ServerToClientMessageType.MatchResult, "1");
        SendMessage(loserID, ServerToClientMessageType.MatchResult, "0");
    }

    void GetTeamRequest(int peerID, string teamData)
    {
        ServerRoom room = GetPeerRoom(peerID);
        if (room == null) return;
        // TeamJson teamJson = JsonSerializer.Deserialize<TeamJson>(teamData);
        TeamJson teamJson = defaultTeam;
        if (teamJson == null)
        {
            GD.PrintErr("Failed to read team data");
            SendMessage(peerID, ServerToClientMessageType.Error, "Reading team data failed!");
            return;
        }
        bool teamLoadSuccess = room.LoadTeam(peerID, teamJson);
        if (!teamLoadSuccess)
        {
            SendMessage(peerID, ServerToClientMessageType.Error, "Could not verify team");
            return;
        }
        SendMessage(peerID, ServerToClientMessageType.ConfirmTeam, "Team was accepted!");
        if (room.ReadyForMatchStart)
        {
            StartMatch(room);
            GD.Print($"Room {room.roomID} has begun its match");
        }
    }

    void StartMatch(ServerRoom room)
    {
        room.serverHead = this;
        room.StartMatch();
        StartMatchInfo info1 = new StartMatchInfo(0, room.p1Json.fighters, room.p2Json.fighters[0].Name);
        string json1 = JsonSerializer.Serialize(info1);
        GD.Print("s1:");
        GD.Print(json1);
        GD.Print($"serialized 1: {json1}, length: {room.p1Json.fighters.Length}");
        SendMessage(room.p1ID, ServerToClientMessageType.StartMatch, json1);
        StartMatchInfo info2 = new StartMatchInfo(1, room.p2Json.fighters, room.p1Json.fighters[0].Name);
        string json2 = JsonSerializer.Serialize(info2);
        GD.Print("s2:");
        GD.Print(json2);
        GD.Print($"serialized 2: {json2}, length: {room.p2Json.fighters.Length}");
        SendMessage(room.p2ID, ServerToClientMessageType.StartMatch, json2);
    }

    void SendMatchState(ServerRoom room)
    {
        int p1ID = room.p1ID;
        int p2ID = room.p2ID;
    }

    void CreateRoomRequest(int peerID)
    {
        if (ServerRoom.pIDToRoomID.ContainsKey(peerID))
        {
            SendMessage(peerID, ServerToClientMessageType.Error, "You're already in a room!");
        }
        else
        {
            ServerBattleManager battleManager = serverBattleInstancePrefab.Instantiate<ServerBattleManager>();
            AddChild(battleManager);
            ServerRoom room = ServerRoom.CreateRoom(battleManager);
            room.AddPlayer(peerID);
            int roomID = room.roomID;
            GD.Print($"Adding player {peerID} to {roomID}");
            SendMessage(peerID, ServerToClientMessageType.ConfirmJoin, roomID.ToString());
        }
    }

    void JoinRoomRequest(int peerID, int roomID)
    {
        if (ServerRoom.roomIDTable.ContainsKey(roomID))
        {
            if (ServerRoom.roomIDTable[roomID].AddPlayer(peerID))
            {
                SendMessage(peerID, ServerToClientMessageType.ConfirmJoin, roomID.ToString());
            }
            else
            {
                SendMessage(peerID, ServerToClientMessageType.Error, $"Room with id {roomID} is full");
            }
        }
        else
        {
            SendMessage(peerID, ServerToClientMessageType.Error, $"No room with id {roomID}");
        }
    }

    void ExitRoomRequest(int peerID)
    {
        if (ServerRoom.pIDToRoomID.ContainsKey(peerID))
        {
            int room = ServerRoom.pIDToRoomID[peerID];
            if (ServerRoom.roomIDTable[room].RemovePlayer(peerID))
            {
                SendMessage(peerID, ServerToClientMessageType.Alert, "You successfully left your room");
            }
            else
            {
                SendMessage(peerID, ServerToClientMessageType.Alert, "You are in a room, but failed to leave");
            }
        }
        SendMessage(peerID, ServerToClientMessageType.Alert, "You are NOT in a room");
    }


    // UI signals
    void OnSendPressed()
    {
        if (_lineEdit.Text == "") return;
        Info($"Sending message: {_lineEdit.Text}");
        _server.Send(0, $"Server says: {_lineEdit.Text}");
        _lineEdit.Text = "";
    }


    void OnListenToggled(bool pressed)
    {
        if (!pressed)
        {
            _server.Stop();
            Info("Server stopped");
            return;
        }
        ushort port = (ushort)_port.Value;
        Error error = _server.Listen(port);
        if (error != Error.Ok)
        {
            Info($"Error listing on port {port}");
            return;
        }
        Info($"Listing on port {port}, supported protocols {_server.supportedProtocols[0]}");
    }
    void SendMessage(int peerID, ServerToClientMessageType messageType, string data = "")
    {
        ServerToClientMessage message = new ServerToClientMessage(messageType, data);
        _server.Send(peerID, JsonSerializer.Serialize(message));
    }
}

public struct MatchState
{

}

