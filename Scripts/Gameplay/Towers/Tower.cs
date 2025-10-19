using Godot;
using System.Collections.Generic;
using System.Diagnostics;

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
	public float ShotSpread;	  // shot spread amount in degrees from 0 - represents the half arc, or as far away from perfect you can get. i.e. a 15 here can be 15 degrees from perfect, resulting in a 30 degree arc
	public float ShotSpreadFalloff; //

	// Unique
	public int   ChainTargets;	  // number of times the projectile will jump in a "chain lightning" style - hitting this many enemies within ChainDistance
	public float ChainDistance;	  // Radius to attempt chaining to
	public float SplashRadius;	  // radius of splash effects - when a projectile hits an enemy, it actually hits all enemies within SplashRadius
	public float SplashCoef;	  // float Coeffecient to apply to other effects within 
	public float PoisonDamage;	  // Amount of damage to do per poison tick.
	public int   PoisonTicks;	  // times to tick poison damage each time a projectile hits.
	public int   PiercingAmount;  // amount of enemies this projectile will go through before being destroyed
	public float KnockbackAmount; // amount of "force" to apply to an enemy to knock them back - some enemies have heavier "mass"
	public float SlowdownPercent; // a float from 0-1, 0 representing stopping completely and 1 representing full, normal speed
	public float SlowdownLength;  // the length of the slowdown effect
	public float HomingStrength;  // a float from 0-1, 0 representing no homing and 1 representing perfect homing
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
		GD.Print("Trying to add Aspect");
		if (a == null) return false;
		GD.Print("Aspect Exists");
		if (slotIndex < 0 || slotIndex > AttachedAspects.Count)
		{
			AttachedAspects.Add(a);

		}
		else
		{
			AttachedAspects.Insert(slotIndex, a);
		}
		GD.Print("Aspect Added");
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

	public void Recompute()
	{
		var s = baseStats;
		foreach (var a in AttachedAspects)
		{
			if (a == null) continue;
			s = a.ModifyGivenStats(s);
		}

		modifiedStats = s;
		ApplyStatsToComponents();
	}

	public void UpdateModifiedStats() => modifiedStats = CalculateModifiedStats();

	private void ApplyStatsToComponents()
	{
		Shooter?.SetStats(modifiedStats);

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
	
	public Aspect GetAspectInSlot(int index)
	{
		if (index < 0 || index >= AttachedAspects.Count) return null;
		return AttachedAspects[index];
	}

	public int FirstEmptySlotIndex()
	{
		for (int i = 0; i < AttachedAspects.Count; i++)
			if (AttachedAspects[i] == null) return i;
		return -1;
	}

	public void SwapSlots(int i, int j)
	{
		if (i == j) return;
		var tmp = AttachedAspects[i];
		AttachedAspects[i] = AttachedAspects[j];
		AttachedAspects[j] = tmp;
	}
	private void SetSlot(int index, Aspect a)
	{
		while (AttachedAspects.Count <= index)
			AttachedAspects.Add(null);

		AttachedAspects[index] = a;
	}

}
