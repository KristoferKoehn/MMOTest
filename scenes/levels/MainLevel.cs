using Godot;
using Godot.Collections;
using MMOTest.Backend;
using MMOTest.scripts.Managers;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

public partial class MainLevel : Node3D
{

    const int PORT = 9001;
    public string ServerAddress = "localhost";
    public UniversalConnector Connector;
    public bool host = false;
    public bool headless = false;
    string PuppetNodePath = "PuppetModels";
    string ClientNodePath = "ClientModels";

    ENetMultiplayerPeer EnetPeer;
    PackedScene PuppetPlayer = GD.Load<PackedScene>("res://scenes/actorScenes/Models/DefaultModel.tscn");
    PackedScene PlayerController = GD.Load<PackedScene>("res://scenes/actorScenes/player.tscn");
    public override void _EnterTree()
    {
        //gonna make sure these are instantiated on client and host
        ActorManager.GetInstance();
        MessageQueue.GetInstance();
        StatManager.GetInstance();
        MessageQueueManager.GetInstance();
    }


    public void PrintStatus()
    {
        GD.Print("Peers Connected: " + Multiplayer.GetPeers().Length);
        foreach (int p in Multiplayer.GetPeers())
        {
            GD.Print("Peer ID " + p);
        }
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

        //this is effectively the server tick. All the events more or less  are processed in MessageQueueManager
        MessageQueueManager.GetInstance().ProcessMessages();
    }

    public void HeadlessHost()
    {
        EnetPeer.CreateServer(PORT);
        Multiplayer.MultiplayerPeer = EnetPeer;
        Multiplayer.PeerConnected += EstablishActor;
        Multiplayer.PeerDisconnected += RemoveActor;
        Timer t = new Timer();
        this.AddChild(t);
        t.Start(5);
        t.Timeout += Connector.HostRefresh;
        t.Timeout += PrintStatus;
    }
    

    //this will break
    public void PeerHost()
    {
        GD.Print("Peerhost functionality effectively deprecated");
        EnetPeer.CreateServer(PORT);
        Multiplayer.MultiplayerPeer = EnetPeer;
        Multiplayer.PeerConnected += EstablishActor;
        Multiplayer.PeerDisconnected += RemoveActor;
        EstablishActor(Multiplayer.GetUniqueId());
    }


    public void EstablishActor(long PeerId)
    {
        GD.Print("Establishing Actor for connecting client: " + PeerId);

        RandomNumberGenerator rng = new RandomNumberGenerator();
        int ActorID = (int)rng.Randi();
        while (ActorManager.GetInstance().GetActor(ActorID) != null)
        {
            ActorID = (int)rng.Randi();
        }

        AbstractModel client = SpawnClientModel(PeerId, ActorID);
        AbstractModel puppet = PuppetPlayer.Instantiate<DefaultModel>();
        puppet.SetTrackingPeerId(PeerId);
        puppet.SetActorID(ActorID);
        client.SetTrackingPeerId(PeerId);
        client.SetActorID(ActorID);
        puppet.SetMultiplayerAuthority(1);
        RpcId(PeerId, "SpawnClientModel", PeerId, ActorID);
        this.GetNode<Node>(PuppetNodePath).AddChild(puppet, forceReadableName: true);
        ActorManager.GetInstance().CreateActor(client, puppet, PeerId, ActorID);
    }


    public void RemoveActor(long PeerId)
    {
        List<Actor> actors = ActorManager.GetInstance().GetActorsFromPeerID(PeerId);

        foreach (Actor actor in actors)
        {
            ActorManager.GetInstance().RemoveActor(actor.ActorID);
            actor.ClientModelReference.QueueFree();
            actor.PuppetModelReference.QueueFree();
            GD.Print("Actor Left: " + actor.ActorID);
        }
    }


    public void Join()
    {
        EnetPeer.CreateClient(ServerAddress, PORT);
        Multiplayer.MultiplayerPeer = EnetPeer;
    }


    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public AbstractModel SpawnClientModel(long PeerId, int ActorID)
    {
        
        DefaultModel PlayerModel = GD.Load<PackedScene>("res://scenes/actorScenes/Models/DefaultModel.tscn").Instantiate<DefaultModel>();
        PlayerModel.GetMultiplayerSynchronizer().SetVisibilityFor(0, false);
        PlayerModel.GetMultiplayerSynchronizer().SetVisibilityFor(1, true);
        PlayerModel.SetMultiplayerAuthority((int)PeerId);
        this.GetNode<Node>(ClientNodePath).AddChild(PlayerModel);
        PlayerModel.Name = PeerId.ToString();

        if (!host)
        {
            GetNode<PlayerController>("PlayerController").AttachModel(PlayerModel);
            ActorManager.GetInstance().CreateActor(PlayerModel, null, PeerId, ActorID);
        }
        return PlayerModel;
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void SendMessage(string message)
    {
        JObject jsonMessagePayload = JObject.Parse(message);
        MessageQueue.GetInstance().AddMessage(jsonMessagePayload);
    }

    //when a clientmodel enters scene tree
    public void _on_client_models_child_entered_tree(Node node)
    {
        RandomNumberGenerator rng = new RandomNumberGenerator();
        ((Node3D)node).Position = new Vector3(rng.RandfRange(-20, -10), 3, rng.RandfRange(10, 20));
    }

    //when a puppetmodel enters scene tree
    public void _on_puppet_models_child_entered_tree(Node node)
    {
        DefaultModel dm = (DefaultModel)node;
        dm.SimulationPeerId = this.Multiplayer.GetUniqueId();
    }

    //when an ability enters the tree
    public void _on_ability_models_child_entered_tree(Node node)
    {
        AbstractAbility t = (AbstractAbility)node;
        t.ApplyHost(host);
    }

    
    //get info from database 
    /*
    string connectionString = "Data Source=your_database_file_path.db";
    using (SqliteConnection connection = new SqliteConnection(connectionString))
    {
        connection.Open();
        using (SqliteCommand command = connection.CreateCommand())
        {
           //figure out connection stuff
            command.CommandText = connectionString;
            command.ExecuteReader();
        }
        connection.Close();
    }
    */

}
