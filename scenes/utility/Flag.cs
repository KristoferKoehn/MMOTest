using Godot;
using MMOTest.Backend;
using MMOTest.scripts.Managers;
using System.Collections.Generic;

public partial class Flag : RigidBody3D
{

	[Export]
	public Teams team { get; set; }
	[Export]
	public bool Carried = false;
	public bool AtBase = true;
	Vector3 ReturnPosition { get;set; }
	Actor carry = null;

    public override void _EnterTree()
    {
		this.SetMultiplayerAuthority(1);
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
		ReturnPosition = this.GlobalPosition;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

        if (Multiplayer.GetUniqueId() != 1)
        {
            return;
        }

        if (carry != null && !DeathManager.GetInstance().IsActorDead(carry) && !carry.PuppetModelReference.IsQueuedForDeletion()) 
		{
			this.GlobalPosition = carry.PuppetModelReference.GlobalPosition;
			this.LinearVelocity = carry.ClientModelReference.Velocity;
		} else
		{
			carry = null;
			Carried = false;
        }

	}

	public void _on_actor_collide_body_entered(Node3D node)
	{

        if (Multiplayer.GetUniqueId() != 1)
        {
            return;
        }

        if (Carried)
		{
			return;
		}

		AbstractModel model = node as AbstractModel;
		if (model != null) {
			StatBlock sb = StatManager.GetInstance().GetStatBlock(model.GetActorID());
			if ((Teams)sb.GetStat(StatType.CTF_TEAM) != this.team)
			{
                GD.Print("ENEMYPICKUP");
                carry = ActorManager.GetInstance().GetActor(model.GetActorID());
				Carried = true;
				AtBase = false;
			} else if (!Carried && !AtBase)
			{
				GD.Print("INSTANT RETURN");
				this.GlobalPosition = ReturnPosition;
				AtBase = true;
			}
		}
	}
}
