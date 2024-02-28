using Godot;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MMOTest.scripts.Managers
{
    public partial class MessageQueueManager : Node
    {

        static MessageQueueManager instance = null;



        private MessageQueueManager() { 
            GetTree().Root.GetNode<MainLevel>("GameLoop/MainLevel").AddChild(this);
        }

        public static MessageQueueManager GetInstance()
        {
            if (instance == null)
            {
                instance = new MessageQueueManager();
                instance.AttachSingleton();
            }
            return instance;
        }

        private void AttachSingleton()
        {
            GetTree().Root.GetNode<MainLevel>("GameLoop/MainLevel").AddChild(instance);
        }

        //get thing from queue
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
                    GetTree().Root.GetNode<Node>("GameLoop/MainLevel/AbilityModels").AddChild(ability, forceReadableName: true);
                }
                //if type == statchange do that
                //if type == pickup
                //if type == equip
                //if type == interact
                //if type == ???
                //do something here
            }
        }
    }
}