using Godot;
using System.Collections.Generic;
using scripts.server;
using Newtonsoft.Json.Linq;

public class ActorManager
{
	private bool host;

	List<Actor> actors = new List<Actor>();

	public void ApplyHost(bool host)
	{
		this.host = host;
	}

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void UpdateClientStats(string json)
	{
		JObject dict = new JObject(json);
		// update local stat block with this dict
		// 
	}

	//what makes an actor
	public void CreateActor(Node3D player, Node3D puppet, int ClientID) 
	{
		Actor actor = new Actor();
		actor.ClientModelReference = player;
		actor.PuppetModelReference = puppet;
		actor.ActorMultiplayerAuthority = ClientID;
		actors.Add(actor);
	}
}
