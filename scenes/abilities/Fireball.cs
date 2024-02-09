using Godot;
using System;

public partial class Fireball : AbstractAbility
{

    public bool host = false;

    public override void _EnterTree()
    {
        //this.Visible = false;
    }


    public override void _Ready()
    {

    }

    public override void Initialize(float[] args, int CasterAuthority, Actor CasterOwner)
    {
        Vector3 position = new Vector3(args[0], args[1], args[2]);
        Vector3 velocity = new Vector3(args[3], args[4], args[5]);
        this.Position = position + velocity * 2;
        this.LinearVelocity = velocity * 20;
    }

    public override void ApplyHost(bool Host)
    {
        GD.Print("Applying Host");
        //this.GetNode<Area3D>("Area3D").Monitoring = host;
        //this.GetNode<Area3D>("Area3D").Monitorable = host;
    }

    public override void SetVisible(bool Visible)
    {
        this.Visible = Visible;
    }

    public void _on_area_3d_body_entered(Node3D node)
    {
        GD.Print("Fireball Collided!!");
        this.QueueFree();
    }

}
