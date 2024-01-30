using Godot;
using System;
using System.Collections.Generic;

public partial class ServerMenu : Node2D
{
	UniversalConnector connector;
	Tree tree;
	LineEdit serverName;
	Timer timer;
	[Export]
	string IPAdress = "0.0.0.0";

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		connector = new UniversalConnector(IPAdress, 9999);
		tree = this.GetNode<Tree>("Control/Panel/Tree");
        serverName = this.GetNode<LineEdit>("Control/Panel/ServerName");
        tree.CreateItem();
		tree.HideRoot = true;
		timer = new Timer();
		this.AddChild(timer);
		timer.Timeout += updateTree;
		timer.Start(2);
		updateTree();

    }


	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
		
	}

	public void _on_host_pressed()
	{
		if (serverName.Text.Length > 0)
		{
			string ip = UpnpSetup();
			connector.Host(serverName.Text, ip);
            //start host

            TestLevel tL = GD.Load<PackedScene>("res://scenes/levels/TestLevel.tscn").Instantiate<TestLevel>();
			tL.host = true;
            this.GetParent<GameLoop>().PushScene(tL);
        }
	}

	public void updateTree()
	{
		tree.Clear();
		tree.CreateItem();

        List<string> serverList = connector.Browse();
		if (serverList.Count < 1)
		{
			return;
		} 
        foreach (string server in serverList)
        {
            string[] strings = server.Split(new char[] { ' ' }, 2);
            TreeItem ti = tree.CreateItem();
            ti.SetText(0, strings[0]);
            ti.SetText(1, strings[1]);
        }
    }

	public void _on_join_pressed()
	{
		if (tree.GetSelected() is not null)
		{
			TreeItem selection = tree.GetSelected();
			string ip = connector.Join(selection.GetText(0));
            TestLevel tL = GD.Load<PackedScene>("res://scenes/levels/TestLevel.tscn").Instantiate<TestLevel>();
			tL.ServerAdress = ip;
            this.GetParent<GameLoop>().PushScene(tL);
        } else if (this.GetNode<TextEdit>("Control/TextEdit").Text.Length > 0)
		{
            string ip = this.GetNode<TextEdit>("Control/TextEdit").Text;
            TestLevel tL = GD.Load<PackedScene>("res://scenes/levels/TestLevel.tscn").Instantiate<TestLevel>();
            tL.ServerAdress = ip;
            this.GetParent<GameLoop>().PushScene(tL);
        }
	}

	public void _on_back_pressed()
	{
        this.GetParent<GameLoop>().PushScene(ResourceLoader.Load<PackedScene>("res://scenes/menu/MainMenu.tscn").Instantiate());
    }


    public string UpnpSetup()
    {
        Upnp upnp = new Upnp();

        int result = upnp.Discover();
        if ((Upnp.UpnpResult)result != Upnp.UpnpResult.Success)
        {
            GD.Print($"UPNP DISCOVER FAILED! ERROR {result}");
        }

        if (upnp.GetGateway() == null && !upnp.GetGateway().IsValidGateway())
        {
            GD.Print("ESTABLISH GATEWAY FAILEDD");
        }

        int MapResult = upnp.AddPortMapping(9001);
        if (MapResult != 0)
        {
            GD.Print("INVALID PORT MAPPING");
        }

        return upnp.QueryExternalAddress();

    }



}
