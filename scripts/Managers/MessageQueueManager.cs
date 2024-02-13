﻿using Godot;
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
            if(mq == null)
            {
                GD.Print("NPDE");
            }
            

            while (mq.Count() > 0)
            {
                JObject m = mq.PopMessage();

                if (m.Property("Type").ToString() == "Cast")
                {
                    AbstractAbility fb = GD.Load<PackedScene>($"res://scenes/abilities/{m.Property("spell")}.tscn").Instantiate<AbstractAbility>();
                    fb.Initialize(m);
                    SceneTreeRoot.GetNode<Node>("GameLoop/TestLevel/AbilityModels").AddChild(fb, forceReadableName: true);
                }
                //do something here
            }
        }
    }
}