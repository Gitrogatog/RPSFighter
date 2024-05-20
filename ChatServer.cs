using System.Collections.Generic;
using Godot;
using System.Text.Json;

public partial class ChatServer : Control
{
    [Export] WebSocketServer _server;
    [Export] RichTextLabel _logDest;
    [Export] LineEdit _lineEdit;
    [Export] SpinBox _port;

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
        // Info($"Remote client disconnected {peerId}. Code {peer.GetCloseCode()}, Reason: {peer.GetCloseReason()}");
        Info($"Remote client {peerId} disconnected");
        _server.Send(-peerId, $"{peerId} disconnected");
    }
    void OnWebSocketServerMessageReceived(int peerId, string message)
    {
        // if (message[0] == '!')
        // {

        //     return;
        // }
        ProcessClientRequest(peerId, message);
        Info($"Server received data from peer {peerId}: {message}");
        _server.Send(-peerId, $"{peerId} Says: {message}");
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
                // case ClientToServerMessageType.:
                //     break;
        }
        // Variant parsed = Json.ParseString(message);
        // GD.Print($"received type: {parsed.VariantType}");
        // // GD.Print($"received as string: {parsed}");
        // GD.Print($"received contents: {parsed}");
    }

    void CreateRoomRequest(int peerID)
    {
        if (ServerRoom.pIDToRoomID.ContainsKey(peerID))
        {
            SendMessage(peerID, ServerToClientMessageType.Error, "You're already in a room!");
        }
        else
        {

            ServerRoom room = ServerRoom.CreateRoom();
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