using Godot;
using MMOTest.Backend;


public partial class PlayerUI : CanvasLayer
{

	ProgressBar progressBar;
	int ActorID;
	bool initialized = false;


	public void initialize(int ActorID)
	{
		this.ActorID = ActorID;
        initialized = true;
    }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		progressBar = this.GetNode<ProgressBar>("ProgressBar");
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (!initialized)
		{
			return;
		}
		StatBlock sb = StatManager.GetInstance().GetStatBlock(ActorID);
		float health = sb.GetStat(StatType.HEALTH);
        progressBar.Value = health;
	}
}
