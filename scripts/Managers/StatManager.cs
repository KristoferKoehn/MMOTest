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

    public StatBlock GetStatBlock(long PeerID)
    {
        return ActorManager.GetInstance().GetActor(PeerID).stats;
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

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void RequestStatblock() 
    {
        long peerID = Multiplayer.GetRemoteSenderId();
        //call next RPC that sends the stat block to the user


    }



}
