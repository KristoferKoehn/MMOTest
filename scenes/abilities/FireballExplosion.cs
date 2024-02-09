using Godot;
using System;

public partial class FireballExplosion : AbstractAbility
{
    private bool HostDelete = false;

    [Export]
    private float ExplosionSpeed = 1.7f;

    public override void _EnterTree()
    {
        Tween t = CreateTween();
        t.TweenProperty(this, "scale", new Vector3(20,20,20), ExplosionSpeed);
        t.Finished += QueueFree;
        t.Play();

    }


    public override void ApplyHost(bool Host)
    {
        this.GetNode<Area3D>("Area3D").Monitoring = Host;
        this.GetNode<Area3D>("Area3D").Monitorable = Host;
        HostDelete = Host;
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
