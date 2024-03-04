using Godot;
using MMOTest.Backend;
using MMOTest.scripts.Managers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Windows.Markup;

public partial class Fireball : AbstractAbility
{

    int SourceActorID = -1;
    public override void Initialize(JObject obj)
    {
        Vector3 position = new Vector3((float)obj.Property("posx"), (float)obj.Property("posy"), (float)obj.Property("posz"));
        Vector3 velocity = new Vector3((float)obj.Property("velx"), (float)obj.Property("vely"), (float)obj.Property("velz"));
        if (obj.ContainsKey("SourceID"))
        {
            SourceActorID = (int)obj.Property("SourceID");
        }
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
        this.QueueFree();
        //this will dispose once this function is done

        JObject m = new JObject
        {
            { "type", "cast"},
            { "spell", "FireballExplosion"},
            { "posx", this.Position.X },
            { "posy", this.Position.Y },
            { "posz", this.Position.Z },
            { "SourceID", SourceActorID}
        };
        MessageQueue.GetInstance().AddMessage(m);

        //if model/actor whatever

        //testing only!!
        JObject b = new JObject
            {
                { "type", "statchange" },
                { "TargetID", 1000 },
                { "SourceID", SourceActorID },
            };

        List<StatProperty> values = new List<StatProperty>
        {
            new StatProperty(StatType.HEALTH, 20)
        };

        b["stats"] = JsonConvert.SerializeObject(values);

        //GD.Print(b.ToString());
        MessageQueue.GetInstance().AddMessage(b);

        //testing only!!!

        AbstractModel target = node as AbstractModel;
        if(target != null)
        {
            StatBlock sourceBlock = StatManager.GetInstance().GetStatBlock(SourceActorID);
            StatBlock targetBlock = StatManager.GetInstance().GetStatBlock(target.GetActorID());

            //has base damage, and scales off intelligence
            //going to calculate the message here, ONLY SEND DELTA DATA

            float nextHealth = targetBlock.GetStat(StatType.HEALTH) - sourceBlock.GetStat(StatType.ABILITY_POINTS) - 5;
            float delta = nextHealth - targetBlock.GetStat(StatType.HEALTH);
            JObject s = new JObject
            {
                { "type", "statchange" },
                { "TargetID", target.GetActorID() },
                { "SourceID", SourceActorID },
                { "stats", new JObject
                    {
                        { "HEALTH", delta }
                    }
                }
            };
            GD.Print(s.ToString());
            MessageQueue.GetInstance().AddMessage(s);
        }

        // get actor ID from model
        // 

    }

}
