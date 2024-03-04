using Godot;
using MMOTest.Backend;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public partial class StatManager : Node
{
    public static StatManager instance;
    public Dictionary<long, List<int>> StatSubscription = new Dictionary<long, List<int>>();

    private StatManager() { }

    public static StatManager GetInstance() 
    {
        if (instance == null)
        {
            instance = new StatManager();
            GameLoop.Root.GetNode<MainLevel>("GameLoop/MainLevel").AddChild(instance);
            instance.Name = "StatManager";
        }
        return instance; 
    }

    public void SubscribePeerToActor(long PeerID, int ActorID)
    {
        if (StatSubscription.ContainsKey(PeerID))
        {
            if (!StatSubscription[PeerID].Contains(ActorID))
            {
                StatSubscription[PeerID].Add(ActorID);
            }
        } else
        {
            StatSubscription[PeerID] = new List<int>
            {
                ActorID
            };
        }
    }

    public void UnsubscribePeerToActor(int PeerID, int ActorID)
    {
        if (StatSubscription.ContainsKey(PeerID))
        {
            StatSubscription[PeerID].Remove(ActorID);
        }
    }

    public void UnsubscribeActorFromAllPeers(int ActorID)
    {
        foreach(List<int> lists in StatSubscription.Values)
        {
            lists.Remove(ActorID);
        }
    }

    public StatBlock GetStatBlock(int ActorID)
    {
        return ActorManager.GetInstance().GetActor(ActorID).stats;
    }

    public void CalculateAndNotify(long Source, long Target, Variant name)
    {
        //put huge function here. 
        // Damage = Intelligence * IntScalar + IntFlat + Strength * StrScalar + StrFlat + FlagScalar * FlagBool
        // and so on
    }

    /// <summary>
    /// this is called from the client and executed on the server. 
    /// This function calls AssignStatBlock as an RPC from the server to the client with the stat json and the peer that's associated with it.
    /// See AssignStatBlock
    /// </summary>
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void RequestStatBlock(int ActorID) 
    {
        long peerID = ActorManager.GetInstance().GetActor(ActorID).ActorMultiplayerAuthority;
        //call next RPC that sends the stat block to the user
        RpcId(peerID, "AssignStatBlock", GetStatBlock(ActorID).SerializeStatBlock(), ActorID);
    }

    /// <summary>
    /// Assigns a statblock to a client-side actor. This can be the client-player or another actor in a client's simulation.
    /// json must be a serialized Dictionary<StatType, float>
    /// </summary>
    /// <param name="jstr">must be a serialized Dictionary<StatType, float></param>
    /// <param name="peerID">PeerID of the requested actor.</param>
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void AssignStatBlock(string jstr, int ActorID)
    {
        if (ActorManager.GetInstance().GetActor(ActorID) == null)
        {
            ActorManager.GetInstance().CreateActor(null, null, 0, ActorID);
        }

        Dictionary<StatType, float> sb = JsonConvert.DeserializeObject< Dictionary<StatType, float>>(jstr);
        ActorManager.GetInstance().GetActor(ActorID).stats.SetStatBlock(sb);
    }

    /// <summary>
    /// This takes in a jstring of a list of StatProperties describing stat changes
    /// </summary>
    /// <param name="jstatchange">serialized Dictionary<int, Dictionary<StatType, float>>, describing an ActorID and a Dictionary<StatType, float> of stat changes</param>
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void UpdateClientStatChange(string jstatchange)
    {
        Dictionary<int, Dictionary<StatType, float>> StatChanges = JsonConvert.DeserializeObject<Dictionary<int, Dictionary<StatType, float>>>(jstatchange);
        ApplyAllStatChanges(StatChanges);
        GD.Print("Receive stat change");
    }

    public void NotifyStatChanges(Dictionary<int, Dictionary<StatType, float>> statchanges)
    {
        foreach (long peerID in StatSubscription.Keys)
        {
            Dictionary<int, Dictionary<StatType, float>> changes = new();
            foreach (int ActorID in StatSubscription[peerID])
            {
                if (statchanges.ContainsKey(ActorID))
                {
                    changes[ActorID] = statchanges[ActorID];
                }
            }
            RpcId(peerID, "UpdateClientStatChange", JsonConvert.SerializeObject(changes));
        }
    }


    public void ApplyAllStatChanges(Dictionary<int, Dictionary<StatType, float>> statchanges)
    {
        foreach (int ActorID in statchanges.Keys)
        {
            StatBlock sb = GetStatBlock(ActorID);
            sb.ApplyAllChanges(statchanges[ActorID]);
        }
    }

}
