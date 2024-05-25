using Godot;
using System;
using System.Collections.Generic;

public partial class WebSocketServer : Node2D
{
    // signals
    // signal message_received(peer_id: int, message)
    // signal client_connected(peer_id: int)
    // signal client_disconnected(peer_id: int)
    [Signal] public delegate void MessageReceivedEventHandler(int peerId, string message);
    [Signal] public delegate void ClientConnectedEventHandler(int peerId);
    [Signal] public delegate void ClientDisconnectedEventHandler(int peerId);


    [Export] string[] handshakeHeaders;
    [Export] public string[] supportedProtocols;
    [Export] int handshakeTimeout = 3000;
    [Export] bool useTLS = false;
    [Export] X509Certificate tlsCert;
    [Export] CryptoKey tlsKey;
    [Export]
    bool RefuseNewConnections
    {
        get
        {
            return refuse;
        }
        set
        {
            refuse = value;
            if (refuse)
            {
                pendingPeers.Clear();
            }
        }
    }
    bool refuse = false;
    // TcpServer tcpServer = new TcpServer();

    const int PORT = 9080;
    TcpServer tcpServer = new TcpServer();
    WebSocketPeer socket = new WebSocketPeer();
    // list of peers currently attempting to connect
    List<PendingPeer> pendingPeers = new List<PendingPeer>();
    // the currently connected peers (int is id, PacketPeer is peer)
    public Dictionary<int, WebSocketPeer> peers = new Dictionary<int, WebSocketPeer>();

    // starts getting the tcp server to listen on that port
    public Error Listen(ushort port)
    {
        LogMessage($"listening on port {port}");
        if (tcpServer.IsListening())
        {
            LogMessage("failed to start server");
        }
        return tcpServer.Listen(port);
    }
    //stops running the servers and stuff
    public void Stop()
    {
        tcpServer.Stop();
        pendingPeers.Clear();
        peers.Clear();
    }

    // send message to given peer id
    public Error Send(int peerId, string message)
    {
        // message.VariantType == Variant.Type.String
        // var type = message.VariantType;
        if (peerId <= 0)
        {
            //send message to multiple peers:
            // zero = broadcast, negative = exclude one
            foreach (int id in peers.Keys)
            {
                if (id == -peerId)
                {
                    continue;
                }
                peers[id].SendText(message);
                // if (type == Variant.Type.String)
                // {
                //     peers[id].SendText((string)message);
                // }
                // else
                // {
                //     // peers[id].PutVar(message);
                //     peers[id].PutPacket((byte[])message);
                // }
            }
            return Error.Ok;
        }
        if (!peers.ContainsKey(peerId))
        {
            LogMessage("lacking peer ID");
        }
        WebSocketPeer socket = peers[peerId];
        return socket.SendText((string)message);
        // if (type == Variant.Type.String)
        // {
        //     return socket.SendText((string)message);
        // }
        // return socket.Send(GD.VarToBytes(message));
    }

    // gets message sent from websocket with given peer ID
    // and returns the most recently sent packet converted to Variant
    string GetMessage(int peerId)
    {
        if (!peers.ContainsKey(peerId))
        {
            LogMessage("lacking peer ID");
        }
        WebSocketPeer socket = peers[peerId];
        if (socket.GetAvailablePacketCount() < 1)
        {
            return "";
        }
        var pkt = socket.GetPacket();
        return pkt.GetStringFromUtf8();
        // if (socket.WasStringPacket())
        // {
        //     return pkt.GetStringFromUtf8();
        // }
        // return GD.BytesToVar(pkt);
    }

    // returns true if the websocket with the passed peerId
    // has any more packets sent that we haven't received yet
    public bool HasMessage(int peerId)
    {
        if (!peers.ContainsKey(peerId))
        {
            LogMessage("lacking peer ID");
        }
        return peers[peerId].GetAvailablePacketCount() > 0;
    }

    public WebSocketPeer CreatePeer()
    {
        WebSocketPeer webSocketPeer = new()
        {
            SupportedProtocols = supportedProtocols,
            HandshakeHeaders = handshakeHeaders
        };
        return webSocketPeer;
    }

    void Poll()
    {
        if (!tcpServer.IsListening())
        {
            // GD.Print("tcp isn't listening!");
            return;
        }
        while (tcpServer.IsConnectionAvailable()) //!RefuseNewConnections && tcpServer.IsConnectionAvailable()
        {
            var conn = tcpServer.TakeConnection();
            // assert conn != null
            pendingPeers.Add(new PendingPeer(conn));
        }

        // remove pending peers
        List<PendingPeer> toRemovePeers = new List<PendingPeer>();
        foreach (PendingPeer peer in pendingPeers)
        {
            if (!ConnectPending(peer))
            {
                if (peer.connectTime + handshakeTimeout < (int)Time.GetTicksMsec())
                {
                    // took too long, timeout this pending peer
                    toRemovePeers.Add(peer);
                }
                continue; // otherwise we're still pending for this peer
            }
            toRemovePeers.Add(peer);
        }
        foreach (PendingPeer removePeer in toRemovePeers)
        {
            pendingPeers.Remove(removePeer);
        }

        List<int> toRemoveIDs = new List<int>();
        foreach (int id in peers.Keys)
        {
            WebSocketPeer peer = peers[id];
            int packetCount = peer.GetAvailablePacketCount();
            peer.Poll();
            if (peer.GetReadyState() != WebSocketPeer.State.Open)
            {
                // emit disconnect client signal with peer id?
                EmitSignal(SignalName.ClientDisconnected, id);
                toRemoveIDs.Add(id);
                continue;
            }
            while (peer.GetAvailablePacketCount() > 0)
            {
                // emit message receibed signal
                EmitSignal(SignalName.MessageReceived, id, GetMessage(id));
            }
        }
        foreach (int removeID in toRemoveIDs)
        {
            peers.Remove(removeID);
        }
        toRemoveIDs.Clear();
    }

    bool ConnectPending(PendingPeer peer)
    {
        if (peer.webSocketPeer != null)
        {
            // poll websocket client if doing handshake
            peer.webSocketPeer.Poll();
            var state = peer.webSocketPeer.GetReadyState();
            if (state == WebSocketPeer.State.Open)
            {
                int id = GD.RandRange(2, 1 << 30);
                peers[id] = peer.webSocketPeer;
                EmitSignal(SignalName.ClientConnected, id);
                return true; //successful connection
            }
            else if (state != WebSocketPeer.State.Connecting)
            {
                return true; // failure connecting
            }
            return false; // still connecting
        }
        else if (peer.tcp.GetStatus() != StreamPeerTcp.Status.Connected)
        {
            return true; // TCP disconnected
        }
        else if (!useTLS)
        {
            // TCP is ready, create WS peer
            peer.webSocketPeer = CreatePeer();
            peer.webSocketPeer.AcceptStream(peer.tcp);
            return false; // WebSocketPeer connection is pending
        }
        else
        {
            if (peer.connection == peer.tcp)
            {
                if (tlsKey == null || tlsCert == null)
                {
                    LogMessage("lacking peer ID");
                }
                StreamPeerTls tls = new StreamPeerTls();
                tls.AcceptStream(peer.tcp, TlsOptions.Server(tlsKey, tlsCert));
                peer.connection = tls;
            }
            StreamPeerTls conn = (StreamPeerTls)peer.connection;
            conn.Poll();
            var status = conn.GetStatus();
            if (status == StreamPeerTls.Status.Connected)
            {
                peer.webSocketPeer = CreatePeer();
                peer.webSocketPeer.AcceptStream(peer.connection);
                return false; // WebSocketPeer connection is still pending
            }
            if (status != StreamPeerTls.Status.Handshaking)
            {
                return true; // failure
            }
            return false;
        }
    }

    void LogMessage(string message)
    {
        string time = Time.GetTimeStringFromSystem();
        GD.PrintErr(message);
    }
    // WebSocket 
    // public override void _Ready()
    // {
    //     base._Ready();
    //     if (tcpServer.Listen(PORT) != Error.Ok)
    //     {
    //         LogMessage("failed to start server");
    //         SetProcess(false);
    //     }
    // }
    public override void _Process(double delta)
    {
        Poll();
    }
}
