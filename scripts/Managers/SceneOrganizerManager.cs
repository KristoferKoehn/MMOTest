using Godot;
using MMOTest.scripts.Managers;
using System;
using System.Collections.Generic;

public partial class SceneOrganizerManager : Node
{

    private static SceneOrganizerManager instance = null;
    public Node3D CurrentLevel = null;
    Dictionary<string, PackedScene> scenes = new Dictionary<string, PackedScene>();
    List<string> sceneNames = new List<string>()
    {
        "res://scenes/actorScenes/Models/MageModel.tscn",
        "res://scenes/actorScenes/Models/NecromancerModel.tscn",
        "res://scenes/abilities/Fireball.tscn",
        "res://scenes/abilities/FireballExplosion.tscn"
    };

    private SceneOrganizerManager() { }
    public static SceneOrganizerManager GetInstance()
    {
        if (instance == null)
        {
            instance = new SceneOrganizerManager();
            GameLoop.Root.GetNode<MainLevel>("GameLoop").AddChild(instance);
            instance.Name = "SceneOrganizerManager";
        }

        return instance;
    }

    public override void _Ready()
    {
        foreach (string sceneName in sceneNames)
        {
            scenes[sceneName] = ResourceLoader.Load<PackedScene>(sceneName, cacheMode: ResourceLoader.CacheMode.Reuse);
        }
    }


    public void SetCurrentLevel(Node3D MainLevel)
    {
        CurrentLevel = MainLevel;
    }

    public Node3D GetCurrentLevel()
    {
        return CurrentLevel;
    }

    public PackedScene GetPackedScene(string sceneName)
    {
        if(scenes.ContainsKey(sceneName))
        {
            return scenes[sceneName];
        } else
        {
            return null;
        }
    }

}
