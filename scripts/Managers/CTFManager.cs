using Godot;
using Godot.Collections;
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
        JObject j = new JObject()
        {
            { "type", "CTF" },
            { "action", "return"},
            { }

        };
    }

    public void RegisterTeam(Teams team)
    {
        //add to team if doesn't exist
        score[team] = 0;
    }

    public void ConsumeMessage(JObject job)
    {
        string actionName = job.Property("action").Value.ToString();
        if(actionName == "return")
        {
            //report event
        } else if (actionName == "capture")
        {
            //increment score
            //
            Teams t = (Teams)(float)job.Property("team").Value;
            score[t]++;
        } else if (actionName == "pickup")
        {
            //report event 
        }
    }


    //function to subscribe flag and capture point
    //function to increment points
}
