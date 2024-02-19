using Godot;
using System;

public partial class DefaultModel : CharacterBody3D
{
    [Export]
    public long TrackingPeerId { get; set; }
    public long SimulationPeerId { get; set; }

    public override void _EnterTree()
    {
        if (TrackingPeerId == SimulationPeerId)
        {
            this.Visible = false;
        }
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
