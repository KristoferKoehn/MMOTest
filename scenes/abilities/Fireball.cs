using Godot;
using System;

public partial class Fireball : RigidBody3D, IAbility
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
        this.GetNode<Area3D>("Area3D").Monitoring = host;
        this.GetNode<Area3D>("Area3D").Monitorable = host;
    }

    public void IsVisible(bool visible)
    {
        this.Visible = visible;
    }

    public void _on_area_3d_body_entered(Node node)
    {
        GD.Print("Fireball Collided!!");
        this.QueueFree();


    }

}
