﻿using Godot;
using MMOTest.Backend;
using System.Collections.Generic;

namespace MMOTest.scripts.Managers
{
    public partial class DeathManager : Node
    {
        private static DeathManager instance = null;
        Dictionary<int, Actor> DeadActors = new Dictionary<int, Actor>();

        private DeathManager() { }

        public static DeathManager GetInstance()
        {
            if (instance == null)
            {
                instance = new DeathManager();
                GameLoop.Root.GetNode<MainLevel>("GameLoop/MainLevel").AddChild(instance);
                instance.Name = "DeathManager";
            }
            return instance;
        }

        /// <summary>
        /// this kills the actor
        /// </summary>
        /// <param name="actor"></param>
        public void AddActor(Actor actor)
        {
            //kill the actor here
            //set dead = true or something. Lock up the controls. dig a grave
            DeadActors.Add(actor.ActorID, actor);
            ActorTimer at = GD.Load<PackedScene>("res://scenes/utility/ActorTimer.tscn").Instantiate<ActorTimer>();
            at.ActorID = actor.ActorID;
            AddChild(at);
            at.Start(5);
            at.ActorTimerTimeout += RespawnActor;
            at.Timeout += at.QueueFree;
        }

        public void RespawnActor(int ActorID)
        {
            GD.Print("respawning actor " + ActorID);
            DeadActors.Remove(ActorID);
            //bring back the boy.
        }
    }
}
