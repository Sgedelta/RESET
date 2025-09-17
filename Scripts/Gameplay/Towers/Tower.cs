using Godot;
using System;

public partial class Tower : Node2D
{
	[Export] public float BaseDamage = 5f;
	[Export] public float BaseFireRate = 1.5f; // shots per second

	public TargetingComponent Targeting { get; private set; }
	public ShooterComponent Shooter { get; private set; }

	public override void _Ready()
	{
		Targeting = GetNode<TargetingComponent>("TargetingComponent");
		Shooter   = GetNode<ShooterComponent>("ShooterComponent");
		Shooter.SetStats(BaseFireRate, BaseDamage);
	}
}
