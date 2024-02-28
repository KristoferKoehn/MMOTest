using Godot;
using System.Collections.Generic;
using scripts.server;
using Newtonsoft.Json.Linq;

public partial class ActorManager : Node
{
	private bool host;

	Dictionary<long, Actor> actors = new Dictionary<long, Actor>();
	static ActorManager instance = null;

	private ActorManager() {
		
	}

	private void AttachSingleton() {
        GetTree().Root.GetNode<MainLevel>("GameLoop/MainLevel").AddChild(instance);
    }

	public static ActorManager GetInstance()
	{
		if (instance == null) {
			instance = new ActorManager();
			instance.AttachSingleton();
        }
		return instance;
	}

	public void ApplyHost(bool host)
	{
		this.host = host;
	}

	//what makes an actor
	public void CreateActor(AbstractModel player, AbstractModel puppet, long PeerID) 
	{
		Actor actor = new Actor();
		actor.ClientModelReference = player;
		actor.PuppetModelReference = puppet;
		actor.ActorMultiplayerAuthority = PeerID;
		actors.Add(PeerID, actor);
	}

	public void RemoveActor(long PeerID)
	{
		actors.Remove(PeerID);
	}

	public Actor GetActor(long PeerID)
	{
		if (actors.TryGetValue(PeerID, out Actor actor))
		{
			return actor;
		}
		return null;
	}
}
