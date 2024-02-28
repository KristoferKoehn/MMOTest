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

	private ActorManager() {
		
	}

	public static ActorManager GetInstance()
	{
		if (instance == null) {
			instance = new ActorManager();
            GameLoop.Root.GetNode<MainLevel>("GameLoop/MainLevel").AddChild(instance);
        }
		return instance;
	}

	public void ApplyHost(bool host)
	{
		this.host = host;
	}

	public void CreateActor(AbstractModel player, AbstractModel puppet, long PeerID)
	{
		Actor actor = new Actor();
		actor.ClientModelReference = player;
		actor.PuppetModelReference = puppet;
		actor.ActorMultiplayerAuthority = PeerID;
        // generic stat block
        JObject stat = new JObject()
		{
			{ Enum.GetName(typeof(StatType), StatType.HEALTH) , 100 },
            { Enum.GetName(typeof(StatType), StatType.MANA) , 100 },
			{ Enum.GetName(typeof(StatType), StatType.MAGIC_RESIST), 13},
			{ Enum.GetName(typeof(StatType), StatType.ARMOR), 11},
			{ Enum.GetName(typeof(StatType), StatType.ABILITY_POINTS), 14},
			{ Enum.GetName(typeof(StatType), StatType.CASTING_SPEED), 12},
			{ Enum.GetName(typeof(StatType), StatType.PHYSICAL_DAMAGE), 16},
        };
		StatBlock statBlock = new StatBlock();
		statBlock.SetStatBlock(stat);
		actor.stats = statBlock;

        actors.Add(PeerID, actor);
	}

	public void RemoveActor(long PeerID)
	{
		actors.Remove(PeerID);
	}

	public Actor GetActor(long PeerID)
	{
		if (actors.TryGetValue(PeerID, out Actor actor))
		{
			return actor;
		}
		return null;
	}

    public override void _Process(double delta)
    {
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
