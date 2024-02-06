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
        if (Multiplayer.GetUniqueId() != 1)
        {
            return;
        }

        foreach (PuppetPlayer p in GetNode<Node>("PuppetModels").GetChildren())
        {
            p.GlobalPosition = GetNode<Node>("ClientModels").GetNode<Node3D>(p.PuppetId.ToString()).GlobalPosition;
        }


        foreach (Node3D n in GetNode<Node>("ClientModels").GetChildren())
        {
            GD.Print(n.GetMultiplayerAuthority() + " at " + n.GlobalPosition.ToString());
        }

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
        //Multiplayer.PeerConnected += PeerConnectedToServer;
        Multiplayer.PeerDisconnected += RemovePlayer;

        AddPlayer(Multiplayer.GetUniqueId());

    }

    public void PeerConnectedToServer(long numby)
    {
        GD.Print("Peer connected to server " + numby);
    }


    public void AddPlayer(long PeerId)
    {

        Node3D player = PlayerController.Instantiate<Node3D>();
        player.Position = new Vector3(0, 3, 0);
        player.Name = PeerId.ToString();
        player.SetMultiplayerAuthority((int)PeerId);
        //player.GetNode<MultiplayerSynchronizer>("MultiplayerSynchronizer").SetVisibilityFor(1, true);
        //player.GetNode<MultiplayerSynchronizer>("MultiplayerSynchronizer").PublicVisibility = false;
        this.GetNode<Node>(ClientNodePath).AddChild(player);

        PuppetPlayer puppet = PuppetPlayer.Instantiate<PuppetPlayer>();
        //puppet.GetNode<MultiplayerSynchronizer>("MultiplayerSynchronizer").SetVisibilityFor((int)PeerId, false);
        puppet.Name = puppet.Name + PeerId.ToString();
        puppet.Position = new Vector3(0, 3, 0);
        puppet.PuppetId = PeerId;
        puppet.SetMultiplayerAuthority(1);
        this.GetNode<Node>(PuppetNodePath).AddChild(puppet);

    }

    public void RemovePlayer(long PeerId) {
        var player = this.GetNode<Node>(ClientNodePath).GetNodeOrNull(PeerId.ToString());
        PuppetPlayer Puppet = null;
        foreach (PuppetPlayer p in this.GetNode<Node>(PuppetNodePath).GetChildren())
        {
            if (p.PuppetId == PeerId)
            {
                Puppet = p;

            }
        }

        player.QueueFree();
        if (Puppet != null)
        {
            Puppet.QueueFree();
        }
    }


    public void Join()
    {
        EnetPeer.CreateClient(ServerAddress, PORT);
        Multiplayer.MultiplayerPeer = EnetPeer;
    }

    public void _on_client_models_child_entered_tree(Node node)
    {
        GD.Print("client model added " + node.GetMultiplayerAuthority());
        if (node.GetMultiplayerAuthority() != this.Multiplayer.GetUniqueId())
        {
            ((Node3D)node).Visible = false;
        }
    }

}
