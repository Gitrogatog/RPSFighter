using Godot;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System.Text.Json;

public partial class ChatClient : Control
{
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
    ClientFighter[] playerTeam;

    public override void _Ready()
    {
        clientBattleUI.OnSelectAction += OnSelectAction;
        clientBattleUI.OnSelectSwap += OnSelectSwap;
    }

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
    }
    void OnWebSocketClientConnectedToServer()
    {
        var webSocket = _client.Socket;
        Info($"Client just connected with protocol {webSocket.GetSelectedProtocol()}");
    }
    void OnWebSocketClientMessageReceived(string message)
    {
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
                Info("Team was confirmed!");
                break;
            case ServerToClientMessageType.StartMatch:
                Info("Match begun");
                StartMatchInfo info = JsonSerializer.Deserialize<StartMatchInfo>(messageData.data);
                StartMatch(info);

                break;
            case ServerToClientMessageType.BattleLog:
                BattleLogMessage battleLogMessage = JsonSerializer.Deserialize<BattleLogMessage>(messageData.data);
                RunLogEffects(battleLogMessage.logs);
                ActionResponseUI(battleLogMessage.response);
                break;
            case ServerToClientMessageType.MatchResult:
                bool didiwin = messageData.data.ToInt() == 1;
                clientBattleUI.SetMatchEnd(didiwin);
                break;

        }
    }

    void StartMatch(StartMatchInfo startMatchInfo)
    {
        clientID = startMatchInfo.teamID;
        GD.Print(startMatchInfo.playerTeam);
        FighterJson[] teamJson = startMatchInfo.playerTeam;
        foreach (FighterJson fighterJson in teamJson)
        {
            GD.Print($"fighter: {fighterJson.Name}");
        }

        playerTeam = CreateFighter.CreateTeamFromJson(teamJson);
        for (int i = 0; i < playerTeam.Length; i++)
        {
            AddChild(playerTeam[i]);
        }
        ClientFighter[] enemyTeam = CreateFighter.CreateBlankTeam(playerTeam.Length);
        for (int i = 0; i < enemyTeam.Length; i++)
        {
            AddChild(enemyTeam[i]);
        }
        FighterData enemyData = DataGlobals.globalFighterDictionary.NameToFighterData[startMatchInfo.enemyFighter];
        enemyTeam[0].PartialInit(enemyData);
        clientBattleUI.InitBattle(playerTeam, enemyTeam);
        Info($"You sent out {playerTeam[0].name}");
        Info($"Opponent sent out {enemyTeam[0].name}");
    }

    void RunLogEffects(BattleLogElement[] logs)
    {
        for (int i = 0; i < logs.Length; i++)
        {
            BattleLogElement log = logs[i];
            GD.Print($"Action log: {log.type} data: {log.data}");
            switch (log.type)
            { //Action, Swap, Damage, Death, StartTurn, EndTurn
                case BattleLogType.Action:
                    ActionLog actionLog = JsonSerializer.Deserialize<ActionLog>(log.data);
                    GD.Print($"Performed action: {actionLog.actionID} from team: {actionLog.team}");
                    Info($"{(actionLog.team == clientID ? "You" : "Opponent")} used {actionLog.actionID}");
                    clientBattleUI.RunAction(actionLog.team == clientID, actionLog.actionID);
                    break;
                case BattleLogType.Swap:
                    SwapLog swapLog = JsonSerializer.Deserialize<SwapLog>(log.data);
                    Info($"{(swapLog.team == clientID ? "You" : "Opponent")} switched into {swapLog.fighterID}");
                    clientBattleUI.RunSwap(swapLog.team == clientID, swapLog.swapToIndex, swapLog.fighterID);
                    break;
                case BattleLogType.Damage:
                    DamageLog damageLog = JsonSerializer.Deserialize<DamageLog>(log.data);
                    Info($"{(damageLog.team == clientID ? "You" : "Opponent")} took {damageLog.damage} damage");
                    clientBattleUI.RunDamage(damageLog.team == clientID, damageLog.damage);
                    break;
                case BattleLogType.Death:
                    int team = log.data.ToInt();
                    Info($"{(team == clientID ? "Your" : "Opponent's")} fighter died!");
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
        GD.Print("Action response from client");
        clientBattleUI.SetExpectedResponse(response);
    }

    void OnSendPressed()
    {

        if (_lineEdit.Text == "") return;
        Info($"Sending message: {_lineEdit.Text}");
        ClientToServerMessage message = new ClientToServerMessage(ClientToServerMessageType.JoinRoom, _lineEdit.Text);
        string sendContent = JsonSerializer.Serialize(message);
        GD.Print($"client is sending message: {sendContent}");
        _client.Send(sendContent);
        _lineEdit.Text = "";
    }
    void OnSelectAction(int actionID)
    {
        Info($"Sending action with ID: {actionID}");
        SendMessage(ClientToServerMessageType.Action, actionID.ToString());
    }
    void OnSelectSwap(int swapID)
    {
        Info($"Sending swap with ID: {swapID}");
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
    public string data { get; set; }
    public ClientToServerMessage(ClientToServerMessageType messageType, string data)
    {
        this.messageType = messageType;
        this.data = data;
    }
}

public struct ServerToClientMessage
{
    public ServerToClientMessageType messageType { get; set; }
    public string data { get; set; }
    public ServerToClientMessage(ServerToClientMessageType messageType, string data)
    {
        this.messageType = messageType;
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

public struct StartMatchInfo
{
    public int teamID { get; set; }
    public FighterJson[] playerTeam { get; set; }
    public string enemyFighter { get; set; }
    public StartMatchInfo(int teamID, FighterJson[] playerTeam, string enemyFighter)
    {
        this.teamID = teamID;
        this.playerTeam = playerTeam;
        this.enemyFighter = enemyFighter;
    }
}
// list of possible messages:
// sent from client to server:
// join room, create room, exit room, send team data, send action 

// sent from server to client:
// confirm part of room, verified team data, error, start match, send turn action data, 
//   send what command client should choose, send match result, send that another user joined/exited the room