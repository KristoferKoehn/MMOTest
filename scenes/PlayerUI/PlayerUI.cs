using Godot;
using MMOTest.Backend;
using MMOTest.scripts.Managers;

public partial class PlayerUI : CanvasLayer
{

	ProgressBar progressBar;
	int ActorID;
	bool initialized = false;


	public void initialize(int ActorID)
	{
		this.ActorID = ActorID;
        initialized = true;
		GetNode<Label>("ActorID").Text = "ActorID: " + ActorID;
        GetNode<Label>("Team").Text = "Team: " + ((Teams)StatManager.GetInstance().GetStatBlock(ActorID).GetStat(StatType.CTF_TEAM)).ToString();
		this.Name = ActorID.ToString();
    }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		progressBar = this.GetNode<ProgressBar>("ProgressBar");
		UIManager.GetInstance().RegisterUI(ActorID, this);
		
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
