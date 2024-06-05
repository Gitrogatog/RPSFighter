using Godot;
using System.Collections.Generic;
using System.Text.Json;

public partial class ChatClient : Control
{
    //     signal lobby_joined(lobby)
    // signal connected(id, use_mesh)
    // signal disconnected()
    // signal peer_connected(id)
    // signal peer_disconnected(id)
    // signal offer_received(id, offer)
    // signal answer_received(id, answer)
    // signal candidate_received(id, mid, index, sdp)
    // signal lobby_sealed()
    [Signal] public delegate void JoinedRoomMessageEventHandler(int roomID);
    [Signal] public delegate void ExitedRoomMessageEventHandler(string data);
    [Signal] public delegate void VerifiedTeamMessageEventHandler(string data);
    [Signal] public delegate void ErrorMessageEventHandler(string data);
    [Signal] public delegate void StartMatchMessageEventHandler(string data);
    [Signal] public delegate void TurnLogMessageEventHandler(); // array of turn actions, what response to expect
    [Signal] public delegate void EndMatchMessageEventHandler(string data);
    [Export] WebSocketClient _client;
    [Export] RichTextLabel _logDest;
    [Export] LineEdit _lineEdit;
    [Export] LineEdit _host;
    [Export] SpinBox _roomID;
    [Export] SpinBox _actionID;
    [Export] ClientBattleUI clientBattleUI;
    int clientID;


    public void Info(string message)
    {
        GD.Print(message);
        _logDest.AddText(message + "\n");
    }

    // client signals
    void OnWebSocketClientConnectionClosed()
    {
        var webSocket = _client.Socket;
        Info($"This client just disconnected");
        // Info($"This client just disconnected with code {webSocket.GetCloseCode()}, reason {webSocket.GetCloseReason()}");
    }
    void OnWebSocketClientConnectedToServer()
    {
        var webSocket = _client.Socket;
        Info($"Client just connected with protocol {webSocket.GetSelectedProtocol()}");
    }
    void OnWebSocketClientMessageReceived(string message)
    {
        Info(message);
        ProcessServerMessage(message);
    }

    void ProcessServerMessage(string message)
    {
        GD.Print($"client got message: {message}");
        ServerToClientMessage messageData = JsonSerializer.Deserialize<ServerToClientMessage>(message);
        GD.Print($"messageData type: {messageData.messageType}, messageData data: {messageData.data}");
        switch (messageData.messageType)
        {
            case ServerToClientMessageType.ConfirmJoin:
                int roomID = messageData.data.ToInt();
                Info($"Joined room {roomID}");
                break;
            case ServerToClientMessageType.ConfirmTeam:
                break;
            case ServerToClientMessageType.StartMatch:
                LoadGameUI();
                break;
            case ServerToClientMessageType.BattleLog:
                BattleLogMessage battleLogMessage = JsonSerializer.Deserialize<BattleLogMessage>(messageData.data);
                RunLogEffects(battleLogMessage.logs);
                ActionResponseUI(battleLogMessage.response);
                break;

        }
    }

    void RunLogEffects(BattleLogElement[] logs)
    {
        for (int i = 0; i < logs.Length; i++)
        {
            BattleLogElement log = logs[i];
            switch (log.type)
            { //Action, Swap, Damage, Death, StartTurn, EndTurn
                case BattleLogType.Action:
                    ActionLog actionLog = JsonSerializer.Deserialize<ActionLog>(log.data);
                    clientBattleUI.RunAction(actionLog.team == clientID, actionLog.actionID);
                    break;
                case BattleLogType.Swap:
                    SwapLog swapLog = JsonSerializer.Deserialize<SwapLog>(log.data);
                    clientBattleUI.RunSwap(swapLog.team == clientID, swapLog.swapToIndex, swapLog.fighterID);
                    break;
                case BattleLogType.Damage:
                    DamageLog damageLog = JsonSerializer.Deserialize<DamageLog>(log.data);
                    clientBattleUI.RunDamage(damageLog.team == clientID, damageLog.damage);
                    break;
                case BattleLogType.Death:
                    int team = log.data.ToInt();
                    clientBattleUI.RunDeath(clientID == team);
                    break;
                case BattleLogType.StartTurn:
                    clientBattleUI.RunStartTurn();
                    break;
                case BattleLogType.EndTurn:
                    clientBattleUI.RunEndTurn();
                    break;
            }
        }
    }

    void ActionResponseUI(ExpectedActionResponse response)
    {
        clientBattleUI.SetExpectedResponse(response);
    }

    void LoadGameUI()
    {

    }

    // UI Signals
    void OnSendPressed()
    {

        if (_lineEdit.Text == "") return;
        Info($"Sending message: {_lineEdit.Text}");
        ClientToServerMessage message = new ClientToServerMessage(ClientToServerMessageType.JoinRoom, _lineEdit.Text);
        // string sendContent = @$"{""type"": 0, ""id"": 1, ""data"": ""{}""}";
        string sendContent = JsonSerializer.Serialize(message);
        GD.Print($"client is sending message: {sendContent}");
        _client.Send(sendContent);
        _lineEdit.Text = "";
    }
    void OnSelectActionButton()
    {
        int actionID = (int)_actionID.Value;
        OnSelectAction(actionID);
    }
    void OnSelectAction(int actionID)
    {
        Info($"Sending action with ID: {actionID}");
        // _client.Send($"!act{actionID}");
        SendMessage(ClientToServerMessageType.Action, actionID.ToString());
    }
    void OnSelectSwapButton()
    {
        int swapID = (int)_actionID.Value;
        OnSelectSwap(swapID);
    }
    void OnSelectSwap(int swapID)
    {
        Info($"Sending swap with ID: {swapID}");
        // _client.Send($"!act{actionID}");
        SendMessage(ClientToServerMessageType.Swap, swapID.ToString());
    }
    void OnSendTeam()
    {
        Info("trying to start match");
        SendMessage(ClientToServerMessageType.TeamData);
    }

    void OnCreateRoomPressed()
    {
        Info($"Sending request to create a room");
        SendMessage(ClientToServerMessageType.CreateRoom);
    }
    void OnJoinRoomPressed()
    {
        int roomID = (int)_roomID.Value;
        Info($"Sending request to join room: {roomID}");
        // _client.Send($"!joi{roomID}");
        SendMessage(ClientToServerMessageType.JoinRoom, roomID.ToString());
    }

    void OnExitRoomPressed()
    {
        Info("Sending request to exit current room");
        SendMessage(ClientToServerMessageType.ExitRoom);
    }

    void OnConnectToggled(bool pressed)
    {
        if (!pressed)
        {
            _client.Close();
            return;
        }
        if (_host.Text == "")
        {
            return;
        }
        Info($"Connecting to host: {_host.Text}");
        Error error = _client.ConnectToURL(_host.Text);
        if (error != Error.Ok)
        {
            Info("Error connecting to host {_host.Text}");
        }
    }
    void SendMessage(ClientToServerMessageType messageType, string data = "")
    {
        ClientToServerMessage message = new ClientToServerMessage(messageType, data);
        string messageText = JsonSerializer.Serialize(message);
        GD.Print($"Client is sending {messageText}");
        _client.Send(messageText);
    }
}

public struct ClientToServerMessage
{
    public ClientToServerMessageType messageType { get; set; }
    // public int id;
    public string data { get; set; }
    public ClientToServerMessage(ClientToServerMessageType messageType, string data)
    {
        this.messageType = messageType;
        // this.id = id;
        this.data = data;
    }
}

public struct ServerToClientMessage
{
    public ServerToClientMessageType messageType { get; set; }
    // public int id;
    public string data { get; set; }
    public ServerToClientMessage(ServerToClientMessageType messageType, string data)
    {
        this.messageType = messageType;
        // this.id = id;
        this.data = data;
    }
}

public enum ClientToServerMessageType
{
    JoinRoom, CreateRoom, ExitRoom, TeamData, Action, Swap, Alert
}
public enum ServerToClientMessageType
{
    ConfirmJoin, ConfirmTeam, Error, StartMatch, BattleLog, MatchResult, MatchState, Alert
}
// list of possible messages:
// sent from client to server:
// join room, create room, exit room, send team data, send action 

// sent from server to client:
// confirm part of room, verified team data, error, start match, send turn action data, 
//   send what command client should choose, send match result, send that another user joined/exited the room