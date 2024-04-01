using Godot;
using MMOTest.Backend;
using MMOTest.scripts.Managers;
using System;

public partial class ModelManager : Node
{

    static ModelManager instance = null;


    private ModelManager()
    {

    }

    public static ModelManager GetInstance()
    {
        if (instance == null)
        {
            instance = new ModelManager();
            GameLoop.Root.GetNode<MainLevel>("GameLoop/MainLevel").AddChild(instance);
            instance.Name = "ActorManager";
        }
        return instance;
    }


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void ClientModelChange(int ActorID, string classname)
    {
        //changing the client model clientside

        Actor a = ActorManager.GetInstance().GetActor(ActorID);

        if (a != null)
        {
            a.ClientModelReference = ResourceLoader.Load<AbstractModel>("res://scenes/actorScenes/Models/" + classname + "Model");
            a.ClientModelReference.SetMultiplayerAuthority(Multiplayer.GetUniqueId());
            SceneOrganizerManager.GetInstance().GetCurrentLevel().GetNode<PlayerController>("PlayerController").AttachModel(a.ClientModelReference);

        }
    }


    // the route to think about is this:

    // when the player dies, the spawn panel pops up after a few seconds.
    // the player selects a class, and what needs to happen is the models get reassigned.
    // the puppet is rather easy because of the spawner node, from the server just make it and assign it to an actor
    // the client model needs to be made serverside AND RPC'd into the client.
    // then in basically the same function call, the client model gets moved to the spawn position. 

    // the only issues I forsee is that the player controller snaps to the new model in a weird way.
    // 



}
