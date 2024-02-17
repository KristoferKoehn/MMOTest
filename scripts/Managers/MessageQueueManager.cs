using Godot;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMOTest.scripts.Managers
{
    public class MessageQueueManager
    {
        Node SceneTreeRoot { get; set; }

        public MessageQueueManager(Node SceneTreeRoot) {
            this.SceneTreeRoot = SceneTreeRoot;
        }

        //get thing from queue
        //if spell, call spellcastmanager?
        public void ProcessMessages()
        {
            MessageQueue mq = MessageQueue.GetInstance();
            while (mq.Count() > 0)
            {
                JObject m = mq.PopMessage();
                if (m.Property("type").Value.ToString() == "cast")
                {
                    string variable = "";
                    GD.Print(m.Property("spell").Value);
                    AbstractAbility fb = GD.Load<PackedScene>($"res://scenes/abilities/{m.Property("spell").Value}.tscn").Instantiate<AbstractAbility>();

                    GD.Load<PackedScene>($"res://scenes/abilities/{variable}.tscn");

                    fb.SetMultiplayerAuthority(1);
                    fb.Initialize(m);
                    SceneTreeRoot.GetNode<Node>("GameLoop/TestLevel/AbilityModels").AddChild(fb, forceReadableName: true);
                }
                //do something here
            }
        }
    }
}