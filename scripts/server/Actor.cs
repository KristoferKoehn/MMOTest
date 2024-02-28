using Godot;
using System;
using Backend.StatBlock;

namespace scripts.server
{
    public class Actor
    {
        public AbstractModel ClientModelReference;
        public AbstractModel PuppetModelReference;
        public int ActorMultiplayerAuthority;
        public StatBlock stats;
    }
}