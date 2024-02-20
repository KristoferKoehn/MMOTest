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
    PackedScene PuppetPlayer = GD.Load<PackedScene>("res://scenes/actorScenes/Models/DefaultModel.tscn");
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

        foreach (CharacterBody3D p in GetNode<Node>("PuppetModels").GetChildren())
        {
            Node3D clientModel = GetNode<Node>("ClientModels").GetNode<Node3D>(p.Name.ToString());
            p.GlobalPosition = clientModel.GlobalPosition;
            p.Rotation = clientModel.Rotation;
            AnimationPlayer puppetAnim = p.GetNode<AnimationPlayer>("AnimationPlayer");
            AnimationPlayer clientAnim = clientModel.GetNode<AnimationPlayer>("AnimationPlayer");
            if (puppetAnim.CurrentAnimation != clientAnim.CurrentAnimation)
            {
                puppetAnim.Play(clientAnim.CurrentAnimation);
            }
            
        }

        messageQueueManager.ProcessMessages();

    }

    public void HeadlessHost()
    {
        EnetPeer.CreateServer(PORT);
        Multiplayer.MultiplayerPeer = EnetPeer;
        Multiplayer.PeerConnected += AddPlayer;
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
        Multiplayer.PeerDisconnected += RemovePlayer;
        AddPlayer(Multiplayer.GetUniqueId());
    }



    public void AddPlayer(long PeerId)
    {

        RpcId(PeerId, "SpawnClientModel", PeerId);
        SpawnClientModel(PeerId);
        DefaultModel puppet = PuppetPlayer.Instantiate<DefaultModel>();
        puppet.GetNode<MultiplayerSynchronizer>("MultiplayerSynchronizer").SetVisibilityFor(0, true);
        puppet.GetNode<MultiplayerSynchronizer>("MultiplayerSynchronizer").SetVisibilityFor((int)PeerId, false);
        puppet.Name = PeerId.ToString();
        puppet.TrackingPeerId = PeerId;
        puppet.Position = new Vector3(3, 3, 0);
        puppet.SetMultiplayerAuthority(1);
        
        this.GetNode<Node>(PuppetNodePath).AddChild(puppet);
        //GD.Print((int)PeerId + " " + Multiplayer.GetUniqueId());
        
    }

    public void RemovePlayer(long PeerId) {
        var player = this.GetNode<Node>(ClientNodePath).GetNodeOrNull(PeerId.ToString());
        CharacterBody3D Puppet = null;
        foreach (CharacterBody3D p in this.GetNode<Node>(PuppetNodePath).GetChildren())
        {
            if (long.Parse(p.Name) == PeerId)
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
    public void SpawnClientModel(long PeerId)
    {
        
        GD.Print("Spawning Client Model");
        /*
        Node3D player = PlayerController.Instantiate<Node3D>();
        player.GetNode<MultiplayerSynchronizer>("MultiplayerSynchronizer").SetVisibilityFor(1, true);
        player.Position = new Vector3(3, 3, 0);
        player.Name = PeerId.ToString();
        player.SetMultiplayerAuthority((int)PeerId);
        this.GetNode<Node>(ClientNodePath).AddChild(player);
        */

        DefaultModel PlayerModel = GD.Load<PackedScene>("res://scenes/actorScenes/Models/DefaultModel.tscn").Instantiate<DefaultModel>();
        PlayerModel.GetNode<MultiplayerSynchronizer>("MultiplayerSynchronizer").SetVisibilityFor(1, true);
        PlayerModel.SetMultiplayerAuthority((int)PeerId);
        this.GetNode<Node>(ClientNodePath).AddChild(PlayerModel);
        PlayerModel.Name = PeerId.ToString();

        if (!host)
        {
            //attach controller ????
            GetNode<PlayerController>("PlayerController").AttachModel(PlayerModel);
        }

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
        } 
        else
        {
            RandomNumberGenerator rng = new RandomNumberGenerator();
            ((Node3D)node).Position = new Vector3(rng.RandfRange(-20, -10), 3, rng.RandfRange(10, 20));
        }
    }

    public void _on_puppet_models_child_entered_tree(Node node)
    {
        DefaultModel dm = (DefaultModel)node;
        dm.SimulationPeerId = this.Multiplayer.GetUniqueId();
    }

    public void _on_ability_models_child_entered_tree(Node node)
    {
        AbstractAbility t = (AbstractAbility)node;
        t.ApplyHost(host);
    }

    public void _on_ability_spawner_despawned(Node node)
    {
        if (node.IsQueuedForDeletion()) { return; }
        node.QueueFree();
    }

}
