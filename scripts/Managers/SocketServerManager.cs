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
            GD.Print("web client connected from" + stream.GetConnectedHost());
        }
    }

}
