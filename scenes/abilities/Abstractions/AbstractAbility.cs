using Godot;
using Newtonsoft.Json.Linq;
using scripts.server;
using System;

public abstract partial class AbstractAbility : RigidBody3D
{
    public abstract void Initialize(float[] args, int CasterAuthority, Actor CasterOwner);
    public abstract void Initialize(JObject obj);
    public abstract void ApplyHost(bool Host);
    public abstract void SetVisible(bool Visible);
}
