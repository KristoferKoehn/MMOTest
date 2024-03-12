using Godot;
using MMOTest.Backend;
using MMOTest.scripts.Managers;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

public partial class Flag : RigidBody3D
{

	[Export]
	public Teams team { get; set; }
	[Export]
	public bool Carried = false;
	public bool AtBase = true;
	Vector3 ReturnPosition { get;set; }
	public Actor carry = null;

    public override void _EnterTree()
    {
		this.SetMultiplayerAuthority(1);
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
		ReturnPosition = this.GlobalPosition;
		CTFManager.GetInstance().RegisterTeam(team);
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
			this.LinearVelocity = carry.PuppetModelReference.Velocity;
		} else
		{
			carry = null;
			Carried = false;
        }

	}


	public void ReturnHome()
	{
		this.GlobalPosition = ReturnPosition;
		carry = null;
		Carried = false;
		AtBase = true;
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
                carry = ActorManager.GetInstance().GetActor(model.GetActorID());
				Carried = true;
				AtBase = false;
                JObject j = new JObject()
				{
					{ "type", "CTF" },
					{ "action", "pickup"},
					{ "ActorID", model.GetActorID()},
					{ "team", team.ToString()}
				};
                MessageQueue.GetInstance().AddMessage(j);


            } else if (!Carried && !AtBase)
			{
				ReturnHome();
                JObject j = new JObject()
				{
					{ "type", "CTF" },
					{ "action", "return"},
					{ "ActorID", model.GetActorID()},
                    { "team", team.ToString()}
                };
                MessageQueue.GetInstance().AddMessage(j);
			}
		}
	}
}
