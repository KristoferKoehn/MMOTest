using Godot;


namespace MMOTest.scripts.Managers
{
    public partial class ConnectionManager : Node
    {

        private static ConnectionManager instance = null;

        ENetMultiplayerPeer EnetPeer;

        private ConnectionManager() { }
        public static ConnectionManager GetInstance()
        {
            if (instance == null)
            {
                instance = new ConnectionManager();
                GameLoop.Root.GetNode<MainLevel>("GameLoop").AddChild(instance);
                instance.Name = "ConnectionManager";
            }

            return instance;
        }

        //join

        //host

        //spawn player
            //establishActor

        //server stuff
            //subscribe to peerconnected
           
        //receive connection -> get actor data -> hand off to actormanager

        //getting actor data needs to be sourced from a manager that could have access to database

    }
}
