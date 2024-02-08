using Godot;
using System;

public partial class TestAbility : RigidBody3D
{

    public bool host = false;

    public override void _EnterTree()
    {
        this.Visible = false;
    }


    public override void _Ready()
    {

    }

    public void ApplyHost(bool host)
    {
        if (host)
        {
            this.GetNode<CollisionShape3D>("MeshInstance3D/Area3D/CollisionShape3D").Disabled = false;
        }
    }

    public void ApplyVisibility(bool visible)
    {
        if (visible)
        {
            this.Visible = true;
        }
    }

}
