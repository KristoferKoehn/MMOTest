using Godot;
using System;

public abstract partial class AbstractModel : CharacterBody3D
{

	// Called when the node enters the scene tree for the first time.
	public abstract void AttachController(AbstractController controller);
	public abstract void SetTrackingPeerId(long peerId);
	public abstract long GetTrackingPeerId();
	public abstract MultiplayerSynchronizer GetMultiplayerSynchronizer();
	public abstract AnimationPlayer GetAnimationPlayer();
}
