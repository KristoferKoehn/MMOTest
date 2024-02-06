using Godot;
using System;

public partial class TestLevel : Node3D
{

    const int PORT = 9001;
    public string ServerAddress = "localhost";
    public UniversalConnector Connector;
    public bool host = false;
    public bool headless = false;
    string PuppetNodePath = "PuppetModels";
    string ClientNodePath = "ClientModels";

    ENetMultiplayerPeer EnetPeer;
    PackedScene PuppetPlayer = GD.Load<PackedScene>("res://scenes/actorScenes/PuppetPlayer.tscn");
    PackedScene PlayerController = GD.Load<PackedScene>("res://scenes/actorScenes/player.tscn");

    public override void _Ready()
    {
        EnetPeer = new ENetMultiplayerPeer();
        if (OS.HasFeature("dedicated_server"))
        {
            headless = true;
        }

        if (host && !headless)
        {
            PeerHost();
        } 
        else if (host && headless)
        {
            HeadlessHost();
        }
        else
        {
            Join();
        }



    }

    public override void _Process(double delta)
    {
        
    }

    public void HeadlessHost()
    {
        EnetPeer.CreateServer(PORT);
        Multiplayer.MultiplayerPeer = EnetPeer;
        Multiplayer.PeerConnected += AddPlayer;
        Multiplayer.PeerConnected += PeerConnectedToServer;
        Multiplayer.PeerDisconnected += RemovePlayer;
        Timer t = new Timer();
        this.AddChild(t);
        t.Start(5);
        t.Timeout += Connector.HostRefresh;

    }


    public void PeerHost()
    {
        EnetPeer.CreateServer(PORT);

        Multiplayer.MultiplayerPeer = EnetPeer;
        Multiplayer.PeerConnected += AddPlayer;
        Multiplayer.PeerConnected += PeerConnectedToServer;
        Multiplayer.PeerDisconnected += RemovePlayer;


        AddPlayer(Multiplayer.GetUniqueId());

    }

    public void PeerConnectedToServer(long numby)
    {
        GD.Print("Peer connected to server " + numby);
    }


    public void AddPlayer(long PeerId)
    {
        var player = PlayerController.Instantiate<Node3D>();
        player.Name = PeerId.ToString();
        this.GetNode<Node>(ClientNodePath).AddChild(player);
        player.Position = new Vector3(0, 3, 0);
        Node3D puppet = PuppetPlayer.Instantiate<Node3D>();
        puppet.Name = PeerId.ToString();
        this.GetNode<Node>(PuppetNodePath).AddChild(puppet);
        puppet.Position = new Vector3(0, 3, 0);
    }

    public void RemovePlayer(long PeerId) {
        var player = this.GetNode<Node>(ClientNodePath).GetNodeOrNull(PeerId.ToString());
        var puppet = this.GetNode<Node>(PuppetNodePath).GetNodeOrNull(PeerId.ToString());
        player.QueueFree();
        puppet.QueueFree();
    }


    public void Join()
    {
        
        GD.Print( "Joining on Address: " + ServerAddress);
        EnetPeer.CreateClient(ServerAddress, PORT);
        Multiplayer.MultiplayerPeer = EnetPeer;
    }
}
