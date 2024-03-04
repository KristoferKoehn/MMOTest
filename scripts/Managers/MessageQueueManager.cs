using Godot;
using MMOTest.Backend;
using Newtonsoft.Json;
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

            Dictionary<int, Dictionary<StatType, float>> StatChanges = null;
            
            while (mq.Count() > 0)
            {
                JObject m = mq.PopMessage();
                if (m.Property("type").Value.ToString() == "cast")
                {
                    AbstractAbility ability = GD.Load<PackedScene>($"res://scenes/abilities/{m.Property("spell").Value}.tscn").Instantiate<AbstractAbility>();
                    ability.SetMultiplayerAuthority(1); //this will change to be pulled from json
                    ability.Initialize(m);
                    GetTree().Root.GetNode<Node>("GameLoop/MainLevel/AbilityModels").AddChild(ability, forceReadableName: true);
                }
                //if type == statchange do that
                if (m.Property("type").Value.ToString() == "statchange")
                {
                    if (StatChanges == null)
                    {
                        StatChanges = new Dictionary<int, Dictionary<StatType, float>>();
                    }


                    List<StatProperty> mstats = JsonConvert.DeserializeObject<List<StatProperty>>(m["stats"].ToString());
                    int targetID = (int)m["TargetID"];
                    //GD.Print(m.ToString());
                    if (StatChanges.ContainsKey(targetID))
                    {
                        foreach(StatProperty stat in mstats)
                        {
                            if (StatChanges[targetID].ContainsKey(stat.StatType))
                            {
                                StatChanges[targetID][stat.StatType] += stat.Value;
                            } else
                            {
                                StatChanges[targetID][stat.StatType] = stat.Value;
                            }
                        }
                    } else
                    {

                        Dictionary<StatType, float> statDeltas = new Dictionary<StatType, float>();
                        StatChanges[targetID] = statDeltas;

                    }

                    // we change stats

                    // we gotta put ActorID as well as the stat that is changing.

                }
                
                
                //if type == pickup
                //if type == equip
                //if type == interact
                //if type == ???
                //do something here

                //rectify all stat changes in dictionary
                if (StatChanges != null)
                {
                    StatManager.GetInstance().ApplyAllStatChanges(StatChanges);
                    StatManager.GetInstance().NotifyStatChanges(StatChanges);
                }
            }
        }
    }
}