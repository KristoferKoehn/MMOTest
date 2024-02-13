using Godot;
using System;

public partial class ActorManager : Node
{
	private bool host;

	public void ApplyHost(bool host)
	{
		this.host = host;
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
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
