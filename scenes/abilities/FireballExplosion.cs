using Godot;
using Newtonsoft.Json.Linq;
using scripts.server;
using System;


public partial class FireballExplosion : AbstractAbility
{

    [Export]
    private float ExplosionSpeed = 1.7f;
    bool host;

    public override void _EnterTree()
    {
        Tween t = CreateTween();
        Tween G = CreateTween();
        t.TweenProperty(this.GetNode<MeshInstance3D>("MeshInstance3D"), "scale", new Vector3(20,20,20), ExplosionSpeed);
        SphereShape3D sphereShape3D = (SphereShape3D)GetNode<CollisionShape3D>("Area3D/CollisionShape3D").Shape;
        G.TweenProperty(sphereShape3D, "radius", 6.06f, ExplosionSpeed);
        if (!host)
        {
            Timer timer = new Timer();
            timer.Timeout += QueueFree;
            this.AddChild(timer);
            timer.Start(ExplosionSpeed + 0.1);
        }else
        {
            t.Finished += QueueFree;
        }
        t.Play();
        G.Play();
    }

    public override void Initialize(JObject obj)
    {
        this.Position = new Vector3((float)obj.Property("posx"), (float)obj.Property("posy"), (float)obj.Property("posz"));
    }

    public override void ApplyHost(bool Host)
    {
        this.GetNode<Area3D>("Area3D").Monitoring = true;
        this.GetNode<Area3D>("Area3D").Monitorable = true;
        host = Host;
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

            GD.Print("We get here");
            if (host)
            {
                // damage message
            }
            else
            {
                ((CharacterBody3D)node).Velocity += ((this.Position - node.Position).Normalized() + new Vector3(0, 0.1f, 0)).Normalized() * 20;
                GD.Print("WE GET IT");
            }
        }
        
    }
}
