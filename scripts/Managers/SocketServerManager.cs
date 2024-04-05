using Godot;
using System;

public partial class SocketServerManager : Node
{
    int inport = 9002;
    int outport = 9003;


    private SocketServerManager() { }
    private static SocketServerManager instance = null;
    TcpServer TCPin = null;
    WebSocketPeer wsp = null;

    public static SocketServerManager GetInstance() { 
        if (instance == null)
        {
            instance = new SocketServerManager();
            instance.Name = "SocketServerManager";
            GameLoop.Root.GetNode<MainLevel>("GameLoop/MainLevel").AddChild(instance);
        }

        return instance;
    }

    public override void _Ready()
    {
        wsp = new WebSocketPeer();
        TCPin = new TcpServer();
        TCPin.Listen(9002);
    }

    public override void _Process(double delta)
    {

        if (TCPin.IsConnectionAvailable())
        {
            StreamPeerTcp stream = TCPin.TakeConnection();
            wsp.AcceptStream(stream);
            GD.Print("web client connected from" + stream.GetConnectedHost());
        }

        if(wsp.GetReadyState() == WebSocketPeer.State.Open)
        {
            while(wsp.GetAvailablePacketCount() > 0)
            {
                byte[] msg = wsp.GetPacket();
                GD.Print(msg.ToString());
            }
        }
    }
}
