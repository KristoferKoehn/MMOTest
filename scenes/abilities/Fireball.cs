using Godot;
using System;

public partial class Fireball : AbstractAbility
{

    public override void Initialize(float[] args, int CasterAuthority, Actor CasterOwner)
    {
        Vector3 position = new Vector3(args[0], args[1], args[2]);
        Vector3 velocity = new Vector3(args[3], args[4], args[5]);
        this.Position = position + velocity * 2;
        this.LinearVelocity = velocity * 20;
    }

    public override void ApplyHost(bool Host)
    {
        this.GetNode<Area3D>("Area3D").Monitoring = Host;
        this.GetNode<Area3D>("Area3D").Monitorable = Host;
    }

    public override void SetVisible(bool Visible)
    {
        this.Visible = Visible;
    }

    public void _on_area_3d_body_entered(Node3D node)
    {
        GD.Print("Fireball Collided!!");
        float[] args = { this.Position.X, this.Position.Y, this.Position.Z };
        RpcId(1, "CastAbility", "FireballExplosion", args);
        //this.GetParent<Node>().GetParent<TestLevel>().CastAbility("FireballExplosion", args);
        this.QueueFree();
    }

}
