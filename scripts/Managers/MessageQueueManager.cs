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
            /*
            {
                {
                    "ActorID" : 0000,
                    {
                        "HEALTH" : -10,
                    }
                } 

            }


            */
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
                if (m.Property("type").Value.ToString() == "statchange")
                {
                    


                    /*
                    {
                      "type": "statchange",
                      "TargetID": 1000,
                      "SourceID": -1,
                      "stats": {
                        "HEALTH": -209
                      }
                    }
                    */

                    if(StatChangeDictionary.ContainsKey(m.Property("TargetID").ToString()))
                    {

                        StatChangeDictionary[m["TargetID"].ToString()].AddAfterSelf(m["stats"]);

                    } else
                    {

                        StatChangeDictionary[m["TargetID"].ToString()] = m["stats"];
                    }

                    
                    // we change stats

                    // we gotta put ActorID as well as the stat that is changing.



                }


                GD.Print(StatChangeDictionary.ToString());
                //if type == pickup
                //if type == equip
                //if type == interact
                //if type == ???
                //do something here
            }
        }
    }
}