using Godot;
using System;

public partial class TestAbility : Node3D
{

    public override void _Ready()
    {
        Timer t = new Timer();
        this.AddChild(t);
        t.Start(3);
        t.Timeout += this.QueueFree;
    }



}
