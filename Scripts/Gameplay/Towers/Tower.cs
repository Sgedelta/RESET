using Godot;
using System.Collections.Generic;

public struct TowerStats
{
	// Basic
	public int   AspectSlots;
	public float FireRate;
	public float Damage;
	public float Range;
	public float ProjectileSpeed;

	// Advanced
	public float CritChance;
	public float CritMult;
	public float ShotSpread;
	public float ShotSpreadFalloff;

	// Unique
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
	[Export] public int   BaseAspectSlots         = 3;
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
	public readonly List<Aspect> AttachedAspects = new();

	public TargetingComponent Targeting { get; private set; }
	public ShooterComponent   Shooter   { get; private set; }

	public override void _Ready()
	{
		Targeting = GetNodeOrNull<TargetingComponent>("TargetingComponent");
		Shooter   = GetNodeOrNull<ShooterComponent>("ShooterComponent");

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

	public bool AttachAspect(Aspect a, int slotIndex = -1)
	{
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

	public void UpdateModifiedStats() => modifiedStats = CalculateModifiedStats();

	private void ApplyStatsToComponents()
	{
		Shooter?.SetStats(modifiedStats);

		//preserve projectile logic via aspects’ templates
		var type = GetProjectileTypeFromAspects();
		Shooter?.SetProjectileType(type);

		if (Targeting != null) Targeting.Range = modifiedStats.Range;
	}

	public TowerStats CalculateModifiedStats()
	{
		var result = baseStats;
		foreach (var a in AttachedAspects)
		{
			if (a == null) continue;
			result = a.ModifyGivenStats(result);
		}

		result.FireRate        = Mathf.Max(0.05f, result.FireRate);
		result.ProjectileSpeed = Mathf.Max(0f, result.ProjectileSpeed);
		return result;
	}

	private ProjectileType GetProjectileTypeFromAspects()
	{
		foreach (var a in AttachedAspects)
		{
			if (a?.Template == null) continue;
			if (a.Template.ProjectileType != ProjectileType.Regular)
				return a.Template.ProjectileType;
		}
		return ProjectileType.Regular;
	}
}
