using Godot;
using MMOTest.Backend;
using Newtonsoft.Json.Linq;
using System;

public partial class StatManager : Node
{
    public static StatManager instance;

    private StatManager() { }

    public static StatManager GetInstance() 
    {
        if (instance == null)
        {
            instance = new StatManager();
            GameLoop.Root.GetNode<MainLevel>("GameLoop/MainLevel").AddChild(instance);
        }
        return instance; 
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

    public void NotifyStatChange(JObject set)
    {

    }


    /// <summary>
    /// this is called from the client and executed on the server. 
    /// This function calls AssignStatBlock as an RPC from the server to the client with the stat json and the peer that's associated with it.
    /// See AssignStatBlock
    /// </summary>
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void RequestStatblock(int ActorID) 
    {
        long peerID = ActorManager.GetInstance().GetActor(ActorID).ActorMultiplayerAuthority;
        //call next RPC that sends the stat block to the user
        RpcId(peerID, "AssignStatBlock", GetStatBlock(ActorID).GetStatBlockString(), ActorID);
    }

    /// <summary>
    /// Assigns a statblock to a client-side actor. This can be the client-player or another actor in a client's simulation.
    /// </summary>
    /// <param name="jstr">stat block json string (sourced from server)</param>
    /// <param name="peerID">PeerID of the requested actor.</param>
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void AssignStatBlock(string jstr, int ActorID)
    {
        if (ActorManager.GetInstance().GetActor(ActorID) == null)
        {
            ActorManager.GetInstance().CreateActor(null, null, 0, ActorID);
        }

        ActorManager.GetInstance().GetActor(ActorID).stats.SetStatBlock(jstr);
        //if this actor doesn't exist on client
        //    make actor on client, leave out clientmodel or something
        //    client makes decisions based on some metric to subscribe or unsubscribe to "stat updates" for a given actor
        //    gotta do all that somewhere else

    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void UpdateClientStatChange(string jstatchange)
    {
        /*
        ActorID1 : {
            health: 30,
            mana: 23
        },

        ActorID2 : {
            health: 99,
        }
        */

        JObject changes = new JObject(jstatchange);
        foreach (JProperty actorchange in changes.Properties())
        {
            ActorManager.GetInstance().GetActor(int.Parse(actorchange.Name)).stats.SetStatFromChangeList(actorchange.Value.ToString());
        }
    }

}
