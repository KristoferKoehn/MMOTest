using Godot;
using System;

public partial class DefaultModel : AbstractModel
{
    [Export] public long TrackingPeerId { get; set; } = -1;
    [Export] public int ActorID { get; set; } = -1;
    public long SimulationPeerId { get; set; } = -2;
    AbstractController playerController { get; set; }

    public override void AttachController(AbstractController controller)
    {
        playerController = controller;
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        if (TrackingPeerId == SimulationPeerId)
        {
            this.Visible = false;
            this.GetNode<CollisionShape3D>("CollisionShape3D").Disabled = true;
        }
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

    public void ApplyImpulse(Vector3 vec)
    {
        //if not puppet, apply impulse. This prevents puppets from being pushed around/throwing errors because they don't have controllers
        if (playerController != null)
        {
            playerController.ApplyImpulse(vec);
        }
    }

    public override long GetTrackingPeerId()
    {
        return this.TrackingPeerId;
    }

    public override MultiplayerSynchronizer GetMultiplayerSynchronizer()
    {
        return this.GetNode<MultiplayerSynchronizer>("MultiplayerSynchronizer");
    }

    public override AnimationPlayer GetAnimationPlayer()
    {
        return this.GetNode<AnimationPlayer>("AnimationPlayer");
    }

    public override void SetTrackingPeerId(long peerId)
    {
        this.TrackingPeerId = peerId;
    }

    public override void SetActorID(int actorId)
    {
        ActorID = actorId;
    }
}
