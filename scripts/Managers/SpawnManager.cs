using Godot;
using MMOTest.Backend;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MMOTest.scripts.Managers
{
    public partial class SpawnManager : Node
    {

        List<SpawnArea> spawnAreas = new List<SpawnArea>();
        private static SpawnManager instance = null;

        private SpawnManager() { }

        public static SpawnManager GetInstance()
        {
            if (instance == null)
            {
                instance = new SpawnManager();
                GameLoop.Root.GetNode<MainLevel>("GameLoop/MainLevel").AddChild(instance);
                instance.Name = "SpawnManager";
            }
            return instance;
        }

        public void AddSpawnArea(SpawnArea spawnArea)
        {
            spawnAreas.Add(spawnArea);
        }

        public void RemoveSpawnArea(SpawnArea spawnArea)
        {
            spawnAreas.Remove(spawnArea);
        }

        public void SpawnActor(int ActorID)
        {
            Actor actor = ActorManager.GetInstance().GetActor(ActorID);
            StatBlock sb = actor.stats;
            AbstractModel model = actor.PuppetModelReference;

            RandomNumberGenerator rng = new RandomNumberGenerator();
            Teams t = (Teams)sb.GetStat(StatType.CTF_TEAM);
            List<SpawnArea> validAreas = spawnAreas.Where(x => x.Team == t).ToList();
            int index = rng.RandiRange(0, validAreas.Count);

            Vector3 spawnPosition = validAreas[index].GetValidSpawnPoint();

            //move model to position, controller should follow automatically
        }
    }
}
