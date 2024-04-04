using Godot;
using MMOTest.Backend;
using System.Collections.Generic;

namespace MMOTest.scripts.Managers
{
    public partial class ConnectionManager : Node
    {

        private static ConnectionManager instance = null;

        ENetMultiplayerPeer EnetPeer;
        const int PORT = 9001;
        public string ServerAddress = "localhost";
        public UniversalConnector Connector;
        public bool host = false;
        public bool headless = false;
        string PuppetNodePath = "PuppetModels";
        string ClientNodePath = "ClientModels";

        PackedScene PuppetPlayer = ResourceLoader.Load<PackedScene>("res://scenes/actorScenes/Models/MageModel.tscn", cacheMode: ResourceLoader.CacheMode.Reuse);

        private ConnectionManager() { }
        public static ConnectionManager GetInstance()
        {
            if (instance == null)
            {
                instance = new ConnectionManager();
                GameLoop.Root.GetNode<GameLoop>("GameLoop").AddChild(instance);
                instance.Name = "ConnectionManager";
            }

            return instance;
        }

        public void InitializeConnection()
        {
            EnetPeer = new ENetMultiplayerPeer();
            if (OS.HasFeature("dedicated_server"))
            {
                headless = true;
            }
            if (host && headless)
            {
                HeadlessHost();
            }
            else
            {
                Join();
            }

            GD.Print("headless: " + headless + " host: " + host);
        }

        public void HeadlessHost()
        {
            SocketServerManager.GetInstance();
            EnetPeer.CreateServer(PORT);
            Multiplayer.MultiplayerPeer = EnetPeer;
            Multiplayer.PeerConnected += EstablishActor;
            Multiplayer.PeerDisconnected += RemoveActor;
            Timer t = new Timer();
            t.Timeout += Connector.HostRefresh;
            this.AddChild(t);
            t.Start(5);

        }

        public void Join()
        {
            EnetPeer.CreateClient(ServerAddress, PORT);
            Multiplayer.MultiplayerPeer = EnetPeer;
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

            //stop here, wait for spawn signal

            //establish actor across both simulations
            ActorManager.GetInstance().CreateActor(PeerId, ActorID);
            ActorManager.GetInstance().RpcId(PeerId, "CreateActor", PeerId, ActorID);


            //I think that's it?



            //the following must be moved to SpawnManager or something. 
            /*
            AbstractModel client = SpawnClientModel(PeerId, ActorID);
            AbstractModel puppet = PuppetPlayer.Instantiate<MageModel>();
            RpcId(PeerId, "SpawnClientModel", PeerId, ActorID);
            puppet.SetTrackingPeerId(PeerId);
            puppet.SetActorID(ActorID);
            client.SetTrackingPeerId(PeerId);
            client.SetActorID(ActorID);
            puppet.SetMultiplayerAuthority(1);

            Node level = SceneOrganizerManager.GetInstance().GetCurrentLevel();
            level.GetNode<Node>(PuppetNodePath).AddChild(puppet, forceReadableName: true);
            ActorManager.GetInstance().CreateActor(client, puppet, PeerId, ActorID);
            SpawnManager.GetInstance().SpawnActor(ActorID);
            */
            //end move

        }

        public void RemoveActor(long PeerId)
        {
            List<Actor> actors = ActorManager.GetInstance().GetActorsFromPeerID(PeerId);

            foreach (Actor actor in actors)
            {
                ActorManager.GetInstance().RemoveActor(actor.ActorID);
                if (actor.ClientModelReference != null)
                {
                    actor.ClientModelReference.QueueFree();
                }
                if (actor.PuppetModelReference != null)
                {
                    actor.PuppetModelReference.QueueFree();
                }
                UIManager.GetInstance().UnregisterActor(actor.ActorID);
                GD.Print("Actor Left: " + actor.ActorID);
            }

        }

        [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
        public AbstractModel SpawnClientModel(long PeerId, int ActorID)
        {

            MageModel PlayerModel = ResourceLoader.Load<PackedScene>("res://scenes/actorScenes/Models/MageModel.tscn", cacheMode: ResourceLoader.CacheMode.Reuse).Instantiate<MageModel>();
            PlayerModel.GetMultiplayerSynchronizer().SetVisibilityFor(0, false);
            PlayerModel.GetMultiplayerSynchronizer().SetVisibilityFor(1, true);
            PlayerModel.GetMultiplayerSynchronizer().SetVisibilityFor((int)PeerId, true);
            PlayerModel.SetMultiplayerAuthority((int)PeerId);

            Node level = SceneOrganizerManager.GetInstance().GetCurrentLevel();
            level.GetNode<Node>(ClientNodePath).AddChild(PlayerModel);

            PlayerModel.Name = PeerId.ToString();
            PlayerModel.ActorID = ActorID;
            if (!host)
            {
                ActorManager.GetInstance().CreateActor(PlayerModel, null, PeerId, ActorID);
                StatManager.GetInstance().RpcId(1, "RequestStatBlock", ActorID);
                level.GetNode<PlayerController>("PlayerController").AttachModel(PlayerModel);
                GD.Print("attaching playercontroller");
            }
            return PlayerModel;
        }
    }
}
