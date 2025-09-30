using Godot;
using System;
using System.Collections.Generic;

public struct TowerStats
{
	// Basic Stats
	public int   AspectSlots;
	public float FireRate;
	public float Damage;
	public float Range;
	public float ProjectileSpeed;

	// Advanced Stats
	public float CritChance;
	public float CritMult; 
	public float ShotSpread;
	public float ShotSpreadFalloff;


	// Unique Stats
	public int   ChainTargets;
	public float ChainDistance;
	public float SplashRadius;
	public float SplashDamage;
	public float PoisonDamage;
	public int   PoisonTicks;
	public int   PiercingAmount;
	public float KnockbackAmount;
	public float SlowdownPercent;
	public float SlowdownLength;
	public float HomingStrength;
}

public partial class Tower : Node2D
{
	//============STATS============
	[Export] public int BaseAspectSlots           = 3;
	[Export] public float BaseFireRate            = 1.5f;
	[Export] public float BaseDamage              = 5f;
	[Export] public float BaseRange               = 500f;
	[Export] public float BaseProjectileSpeed     = 100f;
	[Export] public float BaseCritChance          = 0f;
	[Export] public float BaseCritMult            = 2f;
	[Export] public float BaseShotSpread          = 0f;
	[Export] public float BaseShotSpreadFalloff   = 0f;
	[Export] public int   BaseChainTargets        = 0;


	private TowerStats baseStats;
	private TowerStats modifiedStats;
	public readonly List<Aspect> AttachedAspects = new List<Aspect>();

	public TargetingComponent Targeting { get; private set; }
	public ShooterComponent Shooter { get; private set; }

	public override void _Ready()
	{
		Targeting = GetNode<TargetingComponent>("TargetingComponent");
		Shooter   = GetNode<ShooterComponent>("ShooterComponent");

		baseStats = new TowerStats
		{
			AspectSlots        = BaseAspectSlots,
			FireRate           = BaseFireRate,
			Damage             = BaseDamage,
			Range              = BaseRange,
			ProjectileSpeed    = BaseProjectileSpeed,
			CritChance         = BaseCritChance,
			CritMult           = BaseCritMult,
			ShotSpread         = BaseShotSpread,
			ShotSpreadFalloff  = BaseShotSpreadFalloff,
			ChainTargets       = BaseChainTargets
		};

		UpdateModifiedStats();
		ApplyStatsToComponents();
	}
	
	// add aspect and recompute stats
	public bool AttachAspect(Aspect a, int slotIndex = -1)
	{
		//fail if it doesnt exist, count is full or its already attached
		if (a == null) return false;

		if (slotIndex < 0 || slotIndex > AttachedAspects.Count)
			AttachedAspects.Add(a);
		else
			AttachedAspects.Insert(slotIndex, a);

		Recompute();
		return true;
	}
	
	public bool DetachAspect(Aspect a)
	{
		if (a == null) return false;
		bool removed = AttachedAspects.Remove(a);
		if (removed) Recompute();
		return removed;
	}

	private void Recompute()
	{
		var s = baseStats;
		foreach (var a in AttachedAspects)
			s = a.ModifyGivenStats(s);

		modifiedStats = s;
		ApplyStatsToComponents();
	}
	/// <summary>
	/// Runs CalculateModifiedStats and sets modifiedStats to the result.
	/// </summary>
	public void UpdateModifiedStats() => modifiedStats = CalculateModifiedStats();

	private void ApplyStatsToComponents()
	{
		Shooter?.SetStats(modifiedStats);
		if (Targeting != null) Targeting.Range = modifiedStats.Range;
	}
	
	/// <summary>
	/// A method that uses the aspects slotted into this tower to calculate what it's modified stats should be.
	/// </summary>
	/// <returns></returns>
	public TowerStats CalculateModifiedStats()
	{
		TowerStats result = baseStats;

		// Apply in list order: this makes "add before multiply" vs "multiply before add" slot-dependent
		foreach (var a in AttachedAspects)
		{
			if (a == null) continue;

			result = a.ModifyGivenStats(result);
		}

		result.FireRate        = Mathf.Max(0.05f, result.FireRate);
		result.ProjectileSpeed = Mathf.Max(0f, result.ProjectileSpeed);

		return result;

	}
	
	

	static float Clamp01(float v) => v < 0f ? 0f : (v > 1f ? 1f : v);

}
