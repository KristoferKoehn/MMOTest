using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

public partial class FlagCapturePoint : Area3D
{
	[Export]
	public Teams team { get; set; }
	[Export]
	public Array<Flag> AcceptedFlags { get; set; } = new Array<Flag>();
	[Export]
	public Array<Flag> RequiredFlagsForValidCapture { get; set; } = new Array<Flag>();
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
	}

	public void _on_area_entered(Area3D area)
	{
		if (Multiplayer.GetUniqueId() != 1)
		{
			return;
		}

        Flag flag = area.GetParent<Node>() as Flag;
		bool validCap = true;
		if (flag != null)
		{
			GD.Print("Is Flag");
			if(flag.Carried)
			{
                GD.Print("Flag is carried");
                if (AcceptedFlags.Contains(flag))
				{
                    GD.Print("Flag is accepted flag");
                    foreach (Flag req in RequiredFlagsForValidCapture)
					{
						if (!req.AtBase)
						{
							validCap = false;
						}
					}

					if (validCap)
					{
						GD.Print((flag.team).ToString() + " flag captured");
					}
				} 
			} 
		}

		return;
    }

}
