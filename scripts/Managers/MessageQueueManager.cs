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
                instance.Name = "MessageQueueManager";
            }
            return instance;
        }

        public void ProcessMessages()
        {
            MessageQueue mq = MessageQueue.GetInstance();

            Dictionary<int, Dictionary<StatType, float>> StatChanges = null;
            //check time
            while (mq.Count() > 0)
            {
                JObject m = mq.PopMessage();
                string MessageType = m["Type"].ToString();
                if (MessageType == "cast")
                {
                    //check mana, check if valid target?
                    AbstractAbility ability = GD.Load<PackedScene>($"res://scenes/abilities/{m.Property("spell").Value}.tscn").Instantiate<AbstractAbility>();
                    ability.SetMultiplayerAuthority(1); //this will change to be pulled from json
                    ability.Initialize(m);
                    GetTree().Root.GetNode<Node>("GameLoop/MainLevel/AbilityModels").AddChild(ability, forceReadableName: true);
                }
                //if type == statchange do that
                if (MessageType == "statchange")
                {
                    List<StatProperty> mstats = JsonConvert.DeserializeObject<List<StatProperty>>(m["stats"].ToString());
                    int targetID = (int)m["TargetID"];
                    StatManager.GetInstance().ApplyStatChange(mstats, targetID);
                }
                if(MessageType == "death")
                {
                    //do the death stuff

                }
                
                
                //if type == pickup
                //if type == equip
                //if type == interact
                //if type == ???
                //do something here

                //rectify all stat changes in dictionary
            } //end of while loop

            StatManager.GetInstance().SendCachedStatData();
        }
    }
}