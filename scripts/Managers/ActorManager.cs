using Godot;
using System.Collections.Generic;
using scripts.server;
using Newtonsoft.Json.Linq;

public partial class ActorManager : Node
{
	private bool host;

	List<Actor> actors = new List<Actor>();
	static ActorManager instance = new ActorManager();

	private ActorManager() {

	}

	public ActorManager GetInstance()
	{
		return instance;
	}

	public void ApplyHost(bool host)
	{
		this.host = host;
	}

	//what makes an actor
	public void CreateActor(AbstractModel player, AbstractModel puppet, int ClientID) 
	{
		Actor actor = new Actor();
		actor.ClientModelReference = player;
		actor.PuppetModelReference = puppet;
		actor.ActorMultiplayerAuthority = ClientID;
		actors.Add(actor);
	}

	public void RemoveActor(Actor actor)
	{
		actors.Remove(actor);
	}

	public void GetActor(long PeerID)
	{

	}
}
