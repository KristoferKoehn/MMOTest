using Godot;
using MMOTest.Backend;
using System.Collections.Generic;

public partial class Flag : RigidBody3D
{

    [Export]
    public Teams team { get; set; }
	public List<Actor> enemy = new List<Actor>();
    public List<Actor> ally = new List<Actor>();
	bool pickup = false;
	Actor carry = null;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if(Multiplayer.GetUniqueId() != 1)
		{
			return;
		}

		if(carry != null)
		{
			if(!carry.ClientModelReference.IsDead)
			{

			} else
			{
				pickup = false;
				
			}
		}
	}

	public void _on_ally_collide_body_entered(Node3D node)
	{
		//check if ally
		//	if ally, if not picked up, if clock isn't started start
			// start clock
	}

	public void _on_capture_point_collide_body_entered(Node3D node)
	{
		//if capture point is not this team
			//count team score
		//else do nothing
	}

	public void _on_enemy_collide_body_entered(Node3D node)
	{
		//if enemy and not pickup

	}
}
