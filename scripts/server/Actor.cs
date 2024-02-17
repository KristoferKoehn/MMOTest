using Godot;
using System;
using Backend.StatBlock;

namespace scripts.server
{
    public class Actor
    {
        public Node3D ClientModelReference;
        public Node3D PuppetModelReference;
        public int ActorMultiplayerAuthority;
        public StatBlock stats;
    }
}