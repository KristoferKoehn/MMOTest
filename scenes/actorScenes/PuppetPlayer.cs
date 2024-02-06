using Godot;
using System;

public partial class PuppetPlayer : Node3D
{
    [Export]
    public long PuppetId { get; set; }

    public override void _Process(double delta)
    {

    }
}
