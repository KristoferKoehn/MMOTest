using Godot;
using MMOTest.Backend;
using MMOTest.scripts.Managers;
using System.Collections.Generic;

public partial class Flag : RigidBody3D
{

    [Export]
    public Teams team { get; set; }
	[Export]
	public float ReturnTime { get; set; } = 7;
    public List<Actor> ally = new List<Actor>();
	bool pickup = false;
	Actor carry = null;
	Timer ReturnTimer = null;
	ProgressBar ProgressBar = null;

	float TimeRemaining = float.MaxValue;

    public override void _EnterTree()
    {
		this.SetMultiplayerAuthority(1);
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
		ReturnTimer = GetNode<Timer>("ReturnTimer");
		ProgressBar = GetNode<ProgressBar>("SubViewport/ProgressBar");
		ReturnTimer.Start(ReturnTime);
		ReturnTimer.Paused = true;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		ProgressBar.Value = TimeRemaining - ReturnTime;


        if (Multiplayer.GetUniqueId() != 1)
        {
            return;
        }

		TimeRemaining = (float)ReturnTimer.TimeLeft;

        ally.RemoveAll(item => DeathManager.GetInstance().IsActorDead(item));


		if(ally.Count < 1 )
		{
			ReturnTimer.Paused = true;
		} else
		{
            ReturnTimer.Paused = false;
        }

        if (carry != null && !DeathManager.GetInstance().IsActorDead(carry) && !carry.PuppetModelReference.IsQueuedForDeletion()) 
		{
			this.GlobalPosition = carry.PuppetModelReference.GlobalPosition;
			this.LinearVelocity = carry.ClientModelReference.Velocity;
		} else
		{
			carry = null;
			pickup = false;
		}
	}

	public void _on_ally_collide_body_entered(Node3D node)
	{
        if (Multiplayer.GetUniqueId() != 1)
        {
            return;
        }

        AbstractModel model = node as AbstractModel;
        if (model != null)
        {
            StatBlock sb = StatManager.GetInstance().GetStatBlock(model.GetActorID());
            if ((Teams)sb.GetStat(StatType.CTF_TEAM) == this.team)
            {
                ally.Add(ActorManager.GetInstance().GetActor(model.GetActorID()));
				if(!pickup)
				{
					ReturnTimer.Paused = false;
				}
            }
        }
    }

	public void _on_capture_point_collide_body_entered(Node3D node)
	{
		//if capture point is not this team
			//count team score
		//else do nothing
	}

	public void _on_enemy_collide_body_entered(Node3D node)
	{

        if (Multiplayer.GetUniqueId() != 1)
        {
            return;
        }

        if (pickup)
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
				pickup = true;
			}
		}
	}
}
