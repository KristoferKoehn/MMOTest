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
	public Array<Flag> RequiredFlagsForValidCapture { get; set; }
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
	}

	public void _on_area_entered(Node3D body)
	{
		if (Multiplayer.GetUniqueId() != 1)
		{
			return;
		}

        Flag flag = body as Flag;
		bool validCap = true;
		if (flag != null)
		{
			if(flag.Carried)
			{
				if(AcceptedFlags.Contains(flag))
				{
					foreach(Flag req in RequiredFlagsForValidCapture)
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
