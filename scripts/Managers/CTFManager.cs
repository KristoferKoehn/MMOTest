using Godot;
using System;

public partial class CTFManager : Node
{

    private static CTFManager instance = null;

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


    //function to subscribe flag and capture point
    //function to increment points
}
