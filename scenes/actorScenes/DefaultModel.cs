using Godot;
using System;

public partial class DefaultModel : CharacterBody3D
{
    [Export]
    public long TrackingPeerId { get; set; } = -1;
    public long SimulationPeerId { get; set; } = -2;

    public override void _EnterTree()
    {

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
}
