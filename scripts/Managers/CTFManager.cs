using Godot;
using Godot.Collections;
using MMOTest.scripts.Managers;
using Newtonsoft.Json.Linq;
using System;

public partial class CTFManager : Node
{

    private static CTFManager instance = null;
    private Dictionary<Teams, int> score = new Dictionary<Teams, int>();


    private CTFManager()
    {
        
    }

    public static CTFManager GetInstance()
    {
        if (instance == null)
        {
            instance = new CTFManager();
            GameLoop.Root.GetNode<MainLevel>("GameLoop/MainLevel").AddChild(instance);
            instance.Name = "CTFManager";
        }
        
        return instance;
    }

    public void RegisterTeam(Teams team)
    {
        //add to team if doesn't exist
        score[team] = 0;
    }

    public void ConsumeMessage(JObject job)
    {
        string actionName = job.Property("action").Value.ToString();
        GD.Print("CTF MANAGER: " + job.Property("team").Value.ToString() + " flag " + actionName + " by " + job.Property("ActorID").Value.ToString());
        UIManager.GetInstance().NotifyAll(job.Property("team").Value.ToString() + " flag " + actionName + " by " + job.Property("ActorID").Value.ToString());
        if (actionName == "return")
        {
            //report event
        } else if (actionName == "capture")
        {
            Teams t = (Teams)Enum.Parse(typeof(Teams),job.Property("team").Value.ToString());
            score[t]++;

        } else if (actionName == "pickup")
        {
            //report event
        } 
    }


    //function to subscribe flag and capture point
    //function to increment points
}
