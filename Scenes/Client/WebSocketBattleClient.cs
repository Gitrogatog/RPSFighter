using Godot;
using System;

public partial class WebSocketBattleClient : Node2D
{
    [Signal] public delegate void ConnectedToServerEventHandler();
    [Signal] public delegate void ConnectionClosedEventHandler();
    [Signal] public delegate void MessageReceivedEventHandler(string message);


    [Export] string[] handshakeHeaders;
    [Export] string[] supportedProtocols;
    TlsOptions tlsOptions = null;

    WebSocketPeer socket = new WebSocketPeer();
    public WebSocketPeer Socket => socket;
    WebSocketPeer.State lastState = WebSocketPeer.State.Closed;

    public Error ConnectToURL(string url)
    {
        socket.SupportedProtocols = supportedProtocols;
        socket.HandshakeHeaders = handshakeHeaders;
        Error error = socket.ConnectToUrl(url, tlsOptions);
        if (error != Error.Ok)
        {
            return error;
        }
        lastState = socket.GetReadyState();
        return Error.Ok;
    }

    public Error Send(string message)
    {
        return socket.SendText((string)message);
        // if (message.VariantType == Variant.Type.String)
        // {

        // }
        // return socket.Send(GD.VarToBytes(message));
    }

    public Error SendAction(int actionID)
    {
        return socket.SendText($"{{action: {actionID}}}");
    }

    public string GetMessage()
    {
        if (socket.GetAvailablePacketCount() < 1)
        {
            return "";
        }
        var packet = socket.GetPacket();
        return packet.GetStringFromUtf8();
        // JSON
        // Json.ParseString()
        // if (socket.WasStringPacket())
        // {

        // }
        // return GD.BytesToVar(packet);
    }

    public void Close(int code = 1000, string reason = "")
    {
        socket.Close(code, reason);
        lastState = socket.GetReadyState();
    }

    void Poll()
    {
        if (socket.GetReadyState() != WebSocketPeer.State.Closed)
        {
            socket.Poll();
        }
        var state = socket.GetReadyState();
        if (lastState != state)
        {
            lastState = state;
            if (state == WebSocketPeer.State.Open)
            {
                EmitSignal(SignalName.ConnectedToServer);
            }
            else if (state == WebSocketPeer.State.Closed)
            {
                EmitSignal(SignalName.ConnectionClosed);
            }
        }
        while (socket.GetReadyState() == WebSocketPeer.State.Open && socket.GetAvailablePacketCount() > 0)
        {
            EmitSignal(SignalName.MessageReceived, GetMessage());
        }
    }

    public override void _Process(double delta)
    {
        Poll();
    }
}
