using Godot;
using System;

namespace MMOTest.Backend
{
    public class Actor
    {
        public AbstractModel ClientModelReference;
        public AbstractModel PuppetModelReference;
        public long ActorMultiplayerAuthority;
        public StatBlock stats;
    }
}