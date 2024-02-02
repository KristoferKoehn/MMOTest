using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;

public partial class GameLoop : Node
{
    public Stack<Node> sceneStack = new Stack<Node>();
    int PORT = 9001;

    public override void _Ready()
    {
        if (OS.HasFeature("dedicated_server"))
        {
            
            string ip = UpnpSetup();
            UniversalConnector connector = new UniversalConnector("50.47.173.115", PORT);
            connector.Host("DEDICATED SERVER", ip);
            //start host

            TestLevel tL = GD.Load<PackedScene>("res://scenes/levels/TestLevel.tscn").Instantiate<TestLevel>();
            tL.Connector = connector;
            tL.host = true;
            this.GetParent<GameLoop>().PushScene(tL);
        } else
        {
            PushScene(ResourceLoader.Load<PackedScene>("res://scenes/menu/MainMenu.tscn").Instantiate());
        }

        
        // put settings here
    }


    public void PushScene(Node node)
    {
        if (sceneStack.Count > 0)
        {
            this.RemoveChild(sceneStack.Peek());
        }
        this.sceneStack.Push(node);
        this.AddChild(node);
    }

    public void PopScene()
    {
        Node node = sceneStack.Pop();
        this.RemoveChild(node);
        node.QueueFree();
        this.AddChild(sceneStack.Peek());
    }

    public string UpnpSetup()
    {
        Upnp upnp = new Upnp();

        int result = upnp.Discover();

        Debug.Assert((Upnp.UpnpResult)result == Upnp.UpnpResult.Success, $"UPNP DISCOVER FAILED! ERROR {result}");

        Debug.Assert(upnp.GetGateway() != null && upnp.GetGateway().IsValidGateway(), "ESTABLISH GATEWAY FAILED");

        int MapResult = upnp.AddPortMapping(PORT);
        Debug.Assert(MapResult == 0, "INVALID PORT MAPPING");

        GD.Print($"SUCCESSFUL UPNP SETUP? map result: {MapResult} - valid gateway: {upnp.GetGateway().IsValidGateway()}");
        return upnp.QueryExternalAddress();

    }
}
