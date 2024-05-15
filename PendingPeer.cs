using Godot;
using System;

public class PendingPeer
{
    public int connectTime;
    public StreamPeerTcp tcp;
    public StreamPeer connection;
    public WebSocketPeer webSocketPeer;
    public PendingPeer(StreamPeerTcp peerTCP)
    {
        tcp = peerTCP;
        connection = peerTCP;
        connectTime = (int)Time.GetTicksMsec();
    }
}