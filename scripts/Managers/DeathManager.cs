using Godot;
using MMOTest.Backend;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMOTest.scripts.Managers
{
    public partial class DeathManager : Node
    {
        private static DeathManager instance = null;
        Dictionary<int, Actor> DeadActors = new Dictionary<int, Actor>();

        private DeathManager()
        {

        }

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

        //tells things they are dead

        /// <summary>
        /// this kills the actor
        /// </summary>
        /// <param name="actor"></param>
        public void AddActor(Actor actor)
        {
            DeadActors.Add(actor.ActorID, actor);
            ActorTimer at = GD.Load<PackedScene>("res://scenes/utility/ActorTimer.tscn").Instantiate<ActorTimer>();
            at.ActorID = actor.ActorID;
            AddChild(at);
        }


    }
}
