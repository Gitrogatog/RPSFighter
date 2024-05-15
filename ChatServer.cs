using System.Collections.Generic;
using Godot;

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
        if (message[0] == '!')
        {
            ProcessClientRequest(peerId, message);
            return;
        }
        Info($"Server received data from peer {peerId}: {message}");
        _server.Send(-peerId, $"{peerId} Says: {message}");
    }

    void ProcessClientRequest(int peerID, string message)
    {

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
}