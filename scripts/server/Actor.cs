using Godot;
using System;
using Backend.StatBlock;

namespace scripts.server
{
    public class Actor
    {
        public AbstractModel ClientModelReference;
        public AbstractModel PuppetModelReference;
        public long ActorMultiplayerAuthority;
        public StatBlock stats;
    }
}