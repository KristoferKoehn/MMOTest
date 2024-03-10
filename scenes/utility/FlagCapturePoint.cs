using Godot;
using Godot.Collections;
using MMOTest.scripts.Managers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

[Tool]
public partial class FlagCapturePoint : Area3D
{
	[Export]
	public Teams team { get; set; }
	[Export]
	public Array<RigidBody3D> AcceptedFlags { get; set; } = new Array<RigidBody3D>();
	[Export]
	public Array<RigidBody3D> RequiredFlagsForValidCapture { get; set; } = new Array<RigidBody3D>();
	// Called when the node enters the scene tree for the first time.
	private List<MeshInstance3D> LineMeshes = new List<MeshInstance3D>();
	public override void _Ready()
	{
        CTFManager.GetInstance().RegisterTeam(team);
    }

    public override void _Process(double delta)
    {
        if (Engine.IsEditorHint())
		{
			foreach(MeshInstance3D lineMesh in LineMeshes) {
				lineMesh.QueueFree();
			}

			LineMeshes.Clear();


			foreach(RigidBody3D f in AcceptedFlags)
			{
				if (f == null) continue;

				MeshInstance3D mesh = new MeshInstance3D();
                ImmediateMesh line = new ImmediateMesh();
                OrmMaterial3D material = new OrmMaterial3D();


				material.ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded;
				material.AlbedoColor = Colors.Chartreuse;

                line.SurfaceBegin(Mesh.PrimitiveType.Lines, material);
				line.SurfaceAddVertex(this.GlobalPosition);
				line.SurfaceAddVertex(f.GlobalPosition);
				line.SurfaceEnd();

				mesh.Mesh = line;

				LineMeshes.Add(mesh);
				mesh.TopLevel = true;
				GetTree().EditedSceneRoot.AddChild(mesh);

				//this.AddChild(mesh);
            }

			foreach(RigidBody3D r in RequiredFlagsForValidCapture)
			{
				if (r == null) continue;	

                MeshInstance3D mesh = new MeshInstance3D();
                ImmediateMesh line = new ImmediateMesh();
                OrmMaterial3D material = new OrmMaterial3D();


                material.ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded;
                material.AlbedoColor = Colors.MediumPurple;

                line.SurfaceBegin(Mesh.PrimitiveType.Lines, material);
                line.SurfaceAddVertex(this.GlobalPosition);
                line.SurfaceAddVertex(r.GlobalPosition);
                line.SurfaceEnd();

                mesh.Mesh = line;

                LineMeshes.Add(mesh);
                mesh.TopLevel = true;
                GetTree().EditedSceneRoot.AddChild(mesh);
            }
		}
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
			if(flag.Carried)
			{
                if (AcceptedFlags.Contains(flag))
				{
                    foreach (Flag req in RequiredFlagsForValidCapture)
					{
						if (!req.AtBase)
						{
							validCap = false;
						}
					}

					if (validCap)
					{
                        JObject j = new JObject()
						{
							{ "type", "CTF" },
							{ "action", "capture"},
							{ "ActorID", flag.carry.ActorID},
							{ "team", team.ToString()}
						};
                        MessageQueue.GetInstance().AddMessage(j);

                        flag.ReturnHome();
                    }
				} 
			} 
		}

		return;
    }

}
