using Godot;
using System;

public abstract partial class AbstractAbility : RigidBody3D
{
    public abstract void Initialize(float[] args, int CasterAuthority, Actor CasterOwner);
    public abstract void ApplyHost();
    public abstract void SetVisible();
}
