using Godot;
using System;
using System.Collections.Generic;

public partial class GameLoop : Node
{
    public Stack<Node> sceneStack = new Stack<Node>();


    public override void _Ready()
    {
        PushScene(ResourceLoader.Load<PackedScene>("res://scenes/menu/MainMenu.tscn").Instantiate());
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
}
