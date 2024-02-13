using Godot;
using Newtonsoft.Json.Linq;
using scripts.server;
using System;


public partial class FireballExplosion : AbstractAbility
{

    [Export]
    private float ExplosionSpeed = 1.7f;

    public override void _EnterTree()
    {
        Tween t = CreateTween();
        t.TweenProperty(this, "scale", new Vector3(20,20,20), ExplosionSpeed);
        t.Finished += QueueFree;
        t.Play();
    }

    public override void Initialize(JObject obj)
    {
        this.Position = new Vector3((float)obj.Property("posx"), (float)obj.Property("posy"), (float)obj.Property("posz"));
    }

    public override void ApplyHost(bool Host)
    {
        this.GetNode<Area3D>("Area3D").Monitoring = !Host;
        this.GetNode<Area3D>("Area3D").Monitorable = !Host;
    }

    public override void Initialize(float[] args, int CasterAuthority, Actor CasterOwner)
    {
        this.Position = new Vector3(args[0], args[1], args[2]);
    }

    public override void SetVisible(bool Visible)
    {
        this.Visible = Visible;
    }

    public void _on_area_3d_body_entered(Node3D node)
    {
        if(node is Godot.CharacterBody3D)
        {
            GD.Print("WE DID IT");
        }
        //if node is player
        //velocity = (center + position + up).normalized() * 20
    }
}
