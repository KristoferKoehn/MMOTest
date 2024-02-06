using Godot;
using System;

public partial class PuppetPlayer : Node3D
{
    [Export]
    public long TrackingPeerId { get; set; }
    public long SimulationPeerId { get; set; }

    public override void _Process(double delta)
    {

    }
}
