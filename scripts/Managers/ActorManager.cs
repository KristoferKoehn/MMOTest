using Godot;
using System;
using System.Collections.Generic;
using scripts.server;
using Newtonsoft.Json.Linq;

public partial class ActorManager : Node
{
	private bool host;

	List<Actor> actors = new List<Actor>();

	public void ApplyHost(bool host)
	{
		this.host = host;
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

	}

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void UpdateClientStats(string json)
	{
		JObject dict = new JObject(json);
		//update local stat block with this dict
	}

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
		if(host)
		{
			
		} 
		else
		{

		}

	}
}
