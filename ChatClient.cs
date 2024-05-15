using Godot;

public partial class ChatClient : Control
{
    [Export] WebSocketClient _client;
    [Export] RichTextLabel _logDest;
    [Export] LineEdit _lineEdit;
    [Export] LineEdit _host;
    [Export] SpinBox _roomID;

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
    }

    // UI Signals
    void OnSendPressed()
    {
        if (_lineEdit.Text == "") return;
        Info($"Sending message: {_lineEdit.Text}");
        _client.Send(_lineEdit.Text);
        _lineEdit.Text = "";
    }
    void OnSelectAction(int actionID)
    {
        Info($"Sending action with ID: {actionID}");
        _client.Send($"!act{actionID}");
    }

    void OnCreateRoomPressed()
    {
        Info($"Sending request to create a room");
        _client.Send("!cre");
    }
    void OnJoinRoomPressed()
    {
        int roomID = (int)_roomID.Value;
        Info($"Sending request to join room: {roomID}");
        _client.Send($"!joi{roomID}");
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
}