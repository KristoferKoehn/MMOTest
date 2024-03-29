using Godot;
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
    PackedScene PuppetPlayer = ResourceLoader.Load<PackedScene>("res://scenes/actorScenes/Models/MageModel.tscn", cacheMode: ResourceLoader.CacheMode.Reuse);


    public override void _EnterTree()
    {
        //gonna make sure these are instantiated on client and host
        ActorManager.GetInstance();
        ConnectionManager.GetInstance();
        SceneOrganizerManager.GetInstance();
        CTFManager.GetInstance();
        DeathManager.GetInstance();
        MessageQueue.GetInstance();
        MessageQueueManager.GetInstance();
        SpawnManager.GetInstance();
        StatManager.GetInstance();
        UIManager.GetInstance();
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
        SceneOrganizerManager.GetInstance().SetCurrentLevel(this);

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

    /*
    public void HeadlessHost()
    {
        EnetPeer.CreateServer(PORT);
        Multiplayer.MultiplayerPeer = EnetPeer;
        Multiplayer.PeerConnected += EstablishActor;
        Multiplayer.PeerDisconnected += RemoveActor;
        Timer t = new Timer();
        t.Timeout += Connector.HostRefresh;
        this.AddChild(t);
        t.Start(5);
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
        AbstractModel puppet = PuppetPlayer.Instantiate<MageModel>();
        puppet.SetTrackingPeerId(PeerId);
        puppet.SetActorID(ActorID);
        client.SetTrackingPeerId(PeerId);
        client.SetActorID(ActorID);
        puppet.SetMultiplayerAuthority(1);
        RpcId(PeerId, "SpawnClientModel", PeerId, ActorID);
        this.GetNode<Node>(PuppetNodePath).AddChild(puppet, forceReadableName: true);
        ActorManager.GetInstance().CreateActor(client, puppet, PeerId, ActorID);
        SpawnManager.GetInstance().SpawnActor(ActorID);
    }


    public void RemoveActor(long PeerId)
    {
        List<Actor> actors = ActorManager.GetInstance().GetActorsFromPeerID(PeerId);

        foreach (Actor actor in actors)
        {
            ActorManager.GetInstance().RemoveActor(actor.ActorID);
            actor.ClientModelReference.QueueFree();
            actor.PuppetModelReference.QueueFree();
            UIManager.GetInstance().UnregisterActor(actor.ActorID);
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

        //DefaultModel PlayerModel = GD.Load<PackedScene>("res://scenes/actorScenes/Models/DefaultModel.tscn").Instantiate<DefaultModel>();
        MageModel PlayerModel = ResourceLoader.Load<PackedScene>("res://scenes/actorScenes/Models/MageModel.tscn", cacheMode: ResourceLoader.CacheMode.Reuse).Instantiate<MageModel>();
        PlayerModel.GetMultiplayerSynchronizer().SetVisibilityFor(0, false);
        PlayerModel.GetMultiplayerSynchronizer().SetVisibilityFor(1, true);
        PlayerModel.GetMultiplayerSynchronizer().SetVisibilityFor((int)PeerId, true);
        PlayerModel.SetMultiplayerAuthority((int)PeerId);
        this.GetNode<Node>(ClientNodePath).AddChild(PlayerModel);
        PlayerModel.Name = PeerId.ToString();
        PlayerModel.ActorID = ActorID;
        if (!host)
        {
            ActorManager.GetInstance().CreateActor(PlayerModel, null, PeerId, ActorID);
            StatManager.GetInstance().RpcId(1, "RequestStatBlock", ActorID);
            GetNode<PlayerController>("PlayerController").AttachModel(PlayerModel);
        }
        return PlayerModel;
    }
    */

    //when a puppetmodel enters scene tree
    public void _on_puppet_models_child_entered_tree(Node node)
    {
        AbstractModel dm = (AbstractModel)node;
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
