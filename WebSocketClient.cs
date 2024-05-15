using Godot;
using System;

public partial class WebSocketClient : Node2D
{
    [Signal] public delegate void ConnectedToServerEventHandler();
    [Signal] public delegate void ConnectionClosedEventHandler();
    [Signal] public delegate void MessageReceivedEventHandler(Variant message);


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

    public Error Send(Variant message)
    {
        if (message.VariantType == Variant.Type.String)
        {
            return socket.SendText((string)message);
        }
        return socket.Send(GD.VarToBytes(message));
    }

    public Error SendAction(int actionID)
    {
        return socket.SendText($"{{action: {actionID}}}");
    }

    public Variant GetMessage()
    {
        if (socket.GetAvailablePacketCount() < 1)
        {
            return default;
        }
        var packet = socket.GetPacket();
        if (socket.WasStringPacket())
        {
            return packet.GetStringFromUtf8();
        }
        return GD.BytesToVar(packet);
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
