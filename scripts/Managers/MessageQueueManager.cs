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
            
        }

        public static MessageQueueManager GetInstance()
        {
            if (instance == null)
            {
                instance = new MessageQueueManager();
                GameLoop.Root.GetNode<MainLevel>("GameLoop/MainLevel").AddChild(instance);
            }
            return instance;
        }

        public void ProcessMessages()
        {
            MessageQueue mq = MessageQueue.GetInstance();

            JObject StatChangeDictionary = new JObject();


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
                if (m.Property("type").Value.ToString() == "schange")
                {
                    // we change stats

                    // we gotta put ActorID and PeerID in the thing, as well as the stat that is changing.

                    // here, we strip the PeerID, use that as a key to a list of ActorID, which is a key to a list of stat changes. 

                }
                //if type == pickup
                //if type == equip
                //if type == interact
                //if type == ???
                //do something here
            }
        }
    }
}