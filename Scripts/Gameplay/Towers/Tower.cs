using Godot;
using System;

public partial class Tower : Node2D
{
	//==========STATS============



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


public struct TowerStats {

	//Basic Stats
	public int AspectSlots = 3;
	public float FireRate = 1.5f; //fire rate in shots per second
	public float Damage = 5f;
	public float Range = 500; //range in pixels of distance
	public float ProjectileSpeed = 100; //speed in pixels per second
	
	//Advanced Stats - these won't be relevant all of the time, but will be semi-frequently
	public float CritChance = 0; //pure chance, so 1 is 100% and 0 is 0%
	public float CritMult = 2; //what final damage will be multiplied by when the crit triggers
	public float ShotSpread = 0; //angle, in degrees, from pure accuracy that shots can potentially spread
	public float ShotSpreadFalloff = 0; //a falloff for spread. At 0, all shots will be evenly distributed around the potential range. Higher numbers tighten spread, lower numbers cause more shots to go wider

	//Extra Stats - rarer, used in more specific use cases
	public int ChainTargets = 0;
	

	public TowerStats()
	{

	}



}
