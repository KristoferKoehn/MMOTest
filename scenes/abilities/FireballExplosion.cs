using Godot;
using MMOTest.Backend;
using Newtonsoft.Json.Linq;
using System;


public partial class FireballExplosion : AbstractAbility
{

    [Export]
    private float ExplosionSpeed = 1.7f;
    public int SourceActorID = -1;
    bool host;

    public override void _Ready()
    {
        Tween t = CreateTween();
        Tween G = CreateTween();
        t.TweenProperty(this.GetNode<MeshInstance3D>("MeshInstance3D"), "scale", new Vector3(20,20,20), ExplosionSpeed);
        SphereShape3D sphereShape3D = (SphereShape3D)GetNode<CollisionShape3D>("Area3D/CollisionShape3D").Shape;
        G.TweenProperty(sphereShape3D, "radius", 6.06f, ExplosionSpeed);
        
        
        if (host)
        {
            t.Finished += QueueFree;
        }
        
        t.Play();
        G.Play();
    }

    public override void Initialize(JObject obj)
    {
        this.Position = new Vector3((float)obj.Property("posx"), (float)obj.Property("posy"), (float)obj.Property("posz"));
        if (obj.ContainsKey("SourceID"))
        {
            SourceActorID = (int)obj.Property("SourceID");
        }
    }

    public override void ApplyHost(bool Host)
    {
        this.GetNode<Area3D>("Area3D").Monitoring = true;
        this.GetNode<Area3D>("Area3D").Monitorable = true;
        host = Host;
    }

    public override void SetVisible(bool Visible)
    {
        this.Visible = Visible;
    }

    public void _on_area_3d_body_entered(Node3D node)
    {
        
        if(node is DefaultModel)
        {

            if (host)
            {
                // damage message
            }
            else
            {
                ((DefaultModel)node).ApplyImpulse((node.Position + new Vector3(0, 1.0f, 0) - this.Position).Normalized() * 60000);//+ new Vector3(0, 0.1f, 0)).Normalized() * 20;
            }
        }
    }
}
