using Godot;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using MMOTest.Backend;

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
		actor.stats = new StatBlock();
		//TODO: define generic stat block better
		actor.stats.SetStat(StatType.HEALTH, 100);
        actor.stats.SetStat(StatType.MANA, 100);
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
