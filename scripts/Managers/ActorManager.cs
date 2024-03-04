using Godot;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using MMOTest.Backend;
using System;

public partial class ActorManager : Node
{
	private bool host;

	Dictionary<long, Actor> actors = new Dictionary<long, Actor>();
	static ActorManager instance = null;

	private ActorManager() 
	{
		
	}

	public static ActorManager GetInstance()
	{
		if (instance == null) {
			instance = new ActorManager();
            GameLoop.Root.GetNode<MainLevel>("GameLoop/MainLevel").AddChild(instance);
            instance.Name = "StatManager";
        }
		return instance;
	}

	public void ApplyHost(bool host)
	{
		this.host = host;
	}

	public void CreateActor(AbstractModel player, AbstractModel puppet, long PeerID, int ActorID)
	{
		Actor actor = new Actor();
		actor.ClientModelReference = player;
		actor.PuppetModelReference = puppet;
		actor.ActorMultiplayerAuthority = PeerID;
		actor.ActorID = ActorID;
        // generic stat block
		StatBlock statBlock = new StatBlock();
        Dictionary<StatType, float> statsDict = new()
        {
            [StatType.HEALTH] = 100,
            [StatType.MANA] = 100,
            [StatType.MAGIC_RESIST] = 13,
            [StatType.ARMOR] = 11,
            [StatType.ABILITY_POINTS] = 14,
            [StatType.CASTING_SPEED] = 12,
            [StatType.PHYSICAL_DAMAGE] = 15
        };

        statBlock.SetStatBlock(statsDict);
		actor.stats = statBlock;

        actors.Add(ActorID, actor);
		StatManager.GetInstance().SubscribePeerToActor(PeerID, ActorID);

	}

	public void RemoveActor(int ActorID)
	{
		actors.Remove(ActorID);
		StatManager.GetInstance().UnsubscribeActorFromAllPeers(ActorID);

	}

	public Actor GetActor(int ActorID)
	{
		if (actors.TryGetValue(ActorID, out Actor actor))
		{
			return actor;
		}
		return null;
	}

    public List<Actor> GetActorsFromPeerID(long PeerID)
    {
		List<Actor> actorsList = new List<Actor>();

        foreach (Actor actor in this.actors.Values)
		{
			if (actor.ActorMultiplayerAuthority == PeerID)
			{
				actorsList.Add(actor);
			}
		}

		return actorsList;
    }


    public override void _Process(double delta)
    {
		if (Multiplayer.GetUniqueId() != 1)
		{
			//do client-only stuff here
			return;
		}


        foreach (Actor actor in actors.Values)
		{
			//makes all the stuff line up. Assign all synced variables from client to puppet
			actor.PuppetModelReference.GlobalPosition = actor.ClientModelReference.GlobalPosition;
            actor.PuppetModelReference.GlobalRotation = actor.ClientModelReference.GlobalRotation;
            if (actor.PuppetModelReference.GetAnimationPlayer().CurrentAnimation != actor.ClientModelReference.GetAnimationPlayer().CurrentAnimation)
			{
				actor.PuppetModelReference.GetAnimationPlayer().CurrentAnimation = actor.ClientModelReference.GetAnimationPlayer().CurrentAnimation;
            }
        }
    }
}
