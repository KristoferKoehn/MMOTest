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
                    GD.Print(m.Property("spell").Value);
                    AbstractAbility ability = GD.Load<PackedScene>($"res://scenes/abilities/{m.Property("spell").Value}.tscn").Instantiate<AbstractAbility>();
                    ability.SetMultiplayerAuthority(1); //this will change to be pulled from json
                    ability.Initialize(m);
                    SceneTreeRoot.GetNode<Node>("GameLoop/MainLevel/AbilityModels").AddChild(ability, forceReadableName: true);
                }
                //do something here
            }
        }
    }
}