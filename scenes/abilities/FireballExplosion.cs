using Godot;
using System;

public partial class FireballExplosion : AbstractAbility
{
    public override void _EnterTree()
    {
        Tween t = CreateTween();
        t.TweenProperty(this, "scale", new Vector3(10,10,10), 3);
        t.Finished += this.QueueFree;
        t.Play();
    }


    public override void ApplyHost(bool Host)
    {
        this.GetNode<Area3D>("Area3D").Monitoring = Host;
        this.GetNode<Area3D>("Area3D").Monitorable = Host;
    }

    public override void Initialize(float[] args, int CasterAuthority, Actor CasterOwner)
    {
        this.Position = new Vector3(args[0], args[1], args[2]);
    }

    public override void SetVisible(bool Visible)
    {
        this.Visible = Visible;
    }
}
