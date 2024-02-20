using Godot;
using System;

public partial class DefaultModel : AbstractModel
{
    [Export]
    public long TrackingPeerId { get; set; } = -1;
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
        playerController.ApplyImpulse(vec);
    }
}
