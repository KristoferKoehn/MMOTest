using Godot;
using MMOTest.scripts.Managers;
using Newtonsoft.Json.Linq;
using System;
using System.Runtime.CompilerServices;

public partial class TestLevel : Node3D
{

    const int PORT = 9001;
    public string ServerAddress = "localhost";
    public UniversalConnector Connector;
    public bool host = false;
    public bool headless = false;
    string PuppetNodePath = "PuppetModels";
    string ClientNodePath = "ClientModels";
    MessageQueueManager messageQueueManager;
    

    ENetMultiplayerPeer EnetPeer;
    PackedScene PuppetPlayer = GD.Load<PackedScene>("res://scenes/actorScenes/PuppetPlayer.tscn");
    PackedScene PlayerController = GD.Load<PackedScene>("res://scenes/actorScenes/player.tscn");
    public override void _EnterTree()
    {
        messageQueueManager = new MessageQueueManager(this.GetTree().Root);
    }


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
            p.GlobalPosition = GetNode<Node>("ClientModels").GetNode<Node3D>(p.TrackingPeerId.ToString()).GlobalPosition;
        }

        // message queue manager processing
        // 
        messageQueueManager.ProcessMessages();

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
        player.Position = new Vector3(3, 3, 0);
        player.Name = PeerId.ToString();
        player.GetNode<MultiplayerSynchronizer>("MultiplayerSynchronizer").PublicVisibility = false;
        player.GetNode<MultiplayerSynchronizer>("MultiplayerSynchronizer").SetVisibilityFor(1, true);
        player.SetMultiplayerAuthority((int)PeerId);
        this.GetNode<Node>(ClientNodePath).AddChild(player);

        PuppetPlayer puppet = PuppetPlayer.Instantiate<PuppetPlayer>();
        //puppet.GetNode<MultiplayerSynchronizer>("MultiplayerSynchronizer").SetVisibilityFor((int)PeerId, false);
        puppet.Name = puppet.Name + PeerId.ToString();
        puppet.Position = new Vector3(3, 3, 0);
        puppet.TrackingPeerId = PeerId;
        puppet.SetMultiplayerAuthority(1);
        this.GetNode<Node>(PuppetNodePath).AddChild(puppet);
        //GD.Print((int)PeerId + " " + Multiplayer.GetUniqueId());
        
    }

    public void RemovePlayer(long PeerId) {
        var player = this.GetNode<Node>(ClientNodePath).GetNodeOrNull(PeerId.ToString());
        PuppetPlayer Puppet = null;
        foreach (PuppetPlayer p in this.GetNode<Node>(PuppetNodePath).GetChildren())
        {
            if (p.TrackingPeerId == PeerId)
            {
                Puppet = p;
            }
        }

        player.QueueFree();
        if (Puppet != null)
        {
            Puppet.QueueFree();
        }
        GD.Print("Peer left " +  PeerId);
    }


    public void Join()
    {
        EnetPeer.CreateClient(ServerAddress, PORT);
        Multiplayer.MultiplayerPeer = EnetPeer;
    }


    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void CastAbility(string SceneName, float[] args)
    {
        if(host)
        {
            AbstractAbility fb = GD.Load<PackedScene>($"res://scenes/abilities/{SceneName}.tscn").Instantiate<AbstractAbility>();
            fb.Initialize(args, CasterAuthority:0, CasterOwner:null);
            fb.SetMultiplayerAuthority(1);
            GetNode<Node>("AbilityModels").AddChild(fb, forceReadableName: true);
        }
    }


    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void SendMessage(string message)
    {
        JObject jsonMessagePayload = JObject.Parse(message);
        MessageQueue.GetInstance().AddMessage(jsonMessagePayload);
    }
    /* 
    Quest Manager stuff
    public void Listener(SpellEvent ddd)
    {
        // userID
        // spellID
        // targetUserID

        // Fireball is associated with questId 1

        // each player will have a dict of quests called Quests
        
        // get Quests for UserId
        // if Quests contains quests associated with spellId
        // if emprty do nothing
        // if exists then change stored Quest state
    }
    */


    public void CastAbilityCall(string SceneName, float[] args)
    {
        RpcId(1, "CastAbility", SceneName, args);
    }

    public void SendMessageCall(string message)
    {
        RpcId(1, "SendMessage", message);
    }


    public void _on_client_models_child_entered_tree(Node node)
    {
        if (node.GetMultiplayerAuthority() != this.Multiplayer.GetUniqueId())
        {
            ((Node3D)node).Visible = false;
            ((Node3D)node).Position = new Vector3(0, -20, 0);
        } else
        {
            RandomNumberGenerator rng = new RandomNumberGenerator();
            ((Node3D)node).Position = new Vector3(rng.RandfRange(-20, -10), 3, rng.RandfRange(10, 20));
        }
    }

    public void _on_puppet_models_child_entered_tree(Node node)
    {
        PuppetPlayer p = ((PuppetPlayer)node);
        p.SimulationPeerId = this.Multiplayer.GetUniqueId();
    }

    public void _on_ability_models_child_entered_tree(Node node)
    {
        GD.Print("CHild entered tree. Host: " + host);
        AbstractAbility t = (AbstractAbility)node;
        t.ApplyHost(host);
    }

    public void _on_ability_spawner_despawned(Node node)
    {

    }

}
