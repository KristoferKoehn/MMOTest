using Godot;
using System;

public partial class TestAbility : RigidBody3D
{

    public bool host = false;

    public override void _EnterTree()
    {
        //this.Visible = false;
    }


    public override void _Ready()
    {

    }

    public void ApplyHost(bool host)
    {
        GD.Print("Collision: " + host);
        if (host)
        {
            this.GetNode<CollisionShape3D>("CollisionShape3D").Disabled = false;
        } else
        {
            this.GetNode<CollisionShape3D>("CollisionShape3D").Disabled = true;
        }
    }

    public void ApplyVisibility(bool visible)
    {
        if (visible)
        {
            this.Visible = true;
        }
    }

    public void _on_area_3d_body_entered(Node node)
    {
        GD.Print("Fireball Collided!!");
        this.QueueFree();
    }


}
