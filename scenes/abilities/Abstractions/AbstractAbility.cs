using Godot;
using System;

public abstract partial class AbstractAbility : RigidBody3D
{
    public abstract void Initialize(float[] args, int CasterAuthority, Actor CasterOwner);
    public abstract void ApplyHost(bool Host);
    public abstract void SetVisible(bool Visible);
}
