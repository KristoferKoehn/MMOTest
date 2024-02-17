using Godot;
using MMOTest.scripts.Managers;
using Newtonsoft.Json.Linq;
using scripts.server;
using System;

public partial class Fireball : AbstractAbility
{
    public override void Initialize(JObject obj)
    {
        Vector3 position = new Vector3((float)obj.Property("posx"), (float)obj.Property("posy"), (float)obj.Property("posz"));
        Vector3 velocity = new Vector3((float)obj.Property("velx"), (float)obj.Property("vely"), (float)obj.Property("velz"));
        this.Position = position + velocity * 2;
        this.LinearVelocity = velocity * 30;
    }

    public override void Initialize(float[] args, int CasterAuthority, Actor CasterOwner)
    {
        // in DB
        // "StartCasting" - 1
        // "StopCasting" - 2
        // ....
        
        Vector3 position = new Vector3(args[0], args[1], args[2]);
        Vector3 velocity = new Vector3(args[3], args[4], args[5]);
        this.Position = position + velocity * 2;
        this.LinearVelocity = velocity * 30;
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
        JObject m = new JObject
        {
            { "type", "cast"},
            { "spell", "FireballExplosion"},
            { "posx", this.Position.X },
            { "posy", this.Position.Y },
            { "posz", this.Position.Z },
        };
        MessageQueue.GetInstance().AddMessage(m);
        this.QueueFree();
    }

}
