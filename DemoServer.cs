using Godot;
using System;

public partial class DemoServer : Node2D
{
    [Export] string[] handshakeHeaders;
    [Export] string[] supportedProtocols;
    [Export] int handshakeTimeout = 3000;
    [Export] bool useTLS = false;
    [Export] X509Certificate tlsCert;
    [Export] CryptoKey tlsKey;

    const int PORT = 9080;
    TcpServer tcpServer = new TcpServer();
    WebSocketPeer socket = new WebSocketPeer();

    void LogMessage(string message)
    {
        string time = Time.GetTimeStringFromSystem();
        GD.Print(message);
    }
    // WebSocket 
    public override void _Ready()
    {
        base._Ready();
        if (tcpServer.Listen(PORT) != Error.Ok)
        {
            LogMessage("failed to start server");
            SetProcess(false);
        }
    }
    public override void _Process(double delta)
    {
        while (tcpServer.IsConnectionAvailable())
        {
            StreamPeerTcp conn = tcpServer.TakeConnection();
            if (conn == null)
            {
                GD.PrintErr("connection is null");
                return;
            }
            socket.AcceptStream(conn);
        }
        socket.Poll();
    }
}