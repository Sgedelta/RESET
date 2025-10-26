using Godot;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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
	[Export] public float BaseProjectileSpeed     = 1000f;
	[Export] public float BaseCritChance          = 0f;
	[Export] public float BaseCritMult            = 2f;
	[Export] public float BaseShotSpread          = 0f;
	[Export] public float BaseShotSpreadFalloff   = 0f;
	[Export] public int   BaseChainTargets        = 0;

	private TowerStats baseStats;
	private TowerStats modifiedStats;
	public TowerStats ModifiedStats { get { return modifiedStats; } }

	public readonly SortedList<int, Aspect> AttachedAspects = new();
	public int LowestOpenSlot 
	{
		get { 
			int lowest = 0;
			//its in order, so count up until we can't count
			foreach (int index in AttachedAspects.Keys)
			{
				if (lowest == index) lowest++;
				else break;
			}
			if (lowest >= modifiedStats.AspectSlots) return -1; //no open slots within range
			return lowest; //the value, which is in range and is the lowest
		}     
	}

	public UI_TowerPullout Pullout {
		get {
			var pullouts = GetTree().GetNodesInGroup("tower_pullout");
			if(pullouts.Count == 0) return null;
			return (UI_TowerPullout)pullouts[0]; 
		} 
	}

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

		if (slotIndex < 0 || slotIndex >= modifiedStats.AspectSlots)
		{
			slotIndex = LowestOpenSlot;
			if (slotIndex == -1) return false;
		}

		if (AttachedAspects.ContainsKey(slotIndex))
			return false;

		AttachedAspects.Add(slotIndex, a);
		GD.Print("Aspect Added");
		Recompute();
		return true;
	}


	/// <summary>
	/// Detatches an Aspect from the tower. Returns true if removal is successful, false if that aspect is not here.
	/// </summary>
	/// <param name="a"></param>
	/// <returns></returns>
	public bool DetachAspect(Aspect a)
	{
		if (a == null) return false;
		int indexOfAspect = AttachedAspects.IndexOfValue(a);

		if(indexOfAspect == -1)
		{
			return false;
		}

		bool removed = AttachedAspects.Remove(AttachedAspects.GetKeyAtIndex(indexOfAspect));
		if (removed) Recompute();
		return removed;
	}

	/// <summary>
	/// Detatches an Aspect at the given index from the tower. Returns true if removal is successful or if there is no aspect in that slot
	/// </summary>
	/// <param name="aspectIndex"></param>
	/// <returns></returns>
	public bool DetachAspect(int aspectIndex)
	{
		if (!AttachedAspects.ContainsKey(aspectIndex))
		{
			return true;
		}
		bool removed = AttachedAspects.Remove(aspectIndex);
		if (removed) Recompute();
		return removed;
	}

	public void Recompute()
	{ 

		modifiedStats = CalculateModifiedStats();
		ApplyStatsToComponents();
	}

	public void UpdateModifiedStats() => modifiedStats = CalculateModifiedStats();

	private void ApplyStatsToComponents()
	{
		Shooter?.SetStats(modifiedStats);

		if (Targeting != null) Targeting.Range = modifiedStats.Range;
	}

	public TowerStats CalculateModifiedStats()
	{
		var result = baseStats;
		foreach (Aspect a in AttachedAspects.Values)
		{
			if (a == null) continue;
			result = a.ModifyGivenStats(result);
		}

		result.FireRate        = Mathf.Max(0.05f, result.FireRate);
		result.ProjectileSpeed = Mathf.Max(0f, result.ProjectileSpeed);
		result.ShotSpread	   = Mathf.Max(0.5f, result.ShotSpread);
		result.ShotSpreadFalloff = Mathf.Max(0.1f, result.ShotSpreadFalloff);
		return result;
	}
	
	public Aspect GetAspectInSlot(int index)
	{
		// Guard against invalid slot numbers, not "occupied count"
		if (index < 0 || index >= modifiedStats.AspectSlots)
			return null;

		return AttachedAspects.TryGetValue(index, out var a) ? a : null;
	}


	public void SwapSlots(int i, int j)
	{
		if (i == j) return;
		Aspect iAsp = null;
		Aspect jAsp = null;

		if(AttachedAspects.ContainsKey(i))
		{
			iAsp = AttachedAspects[i];
		}
		if(AttachedAspects.ContainsKey(j))
		{
			jAsp = AttachedAspects[j];
		}

		//swap w/ sorted list
		if (iAsp != null && jAsp != null)
		{
			var tmp = AttachedAspects[i];
			AttachedAspects[i] = AttachedAspects[j];
			AttachedAspects[j] = tmp;
		}
		else if (iAsp == null)
		{
			AttachedAspects.Remove(j);
			AttachedAspects.Add(i, jAsp);
		} 
		else if (jAsp == null)
		{
			AttachedAspects.Remove(i);
			AttachedAspects.Add(j, iAsp);
		}


	}
	private void SetSlot(int index, Aspect a)
	{
		if(AttachedAspects.ContainsKey(index))
		{
			AttachedAspects[index] = a;
		}
		else
		{
			AttachedAspects.Add(index, a);
		}
		
	}

	public void OnTowerClicked(Node view, InputEvent input, int shapeIndex)
	{
		if (input is not InputEventMouseButton buttonInput) return;
		if (!buttonInput.Pressed || buttonInput.ButtonIndex != MouseButton.Left) return;

		if(Pullout.ActiveTower == this)
		{
			Pullout.ToggleActive();
		} 
		else
		{
			Pullout.ActiveTower = this;
		}

		
	}

	public string StatDisplay()
	{
		string res = "";
		//display all base stats
		res += $"Damage: {GetStatIncrease(baseStats.Damage, modifiedStats.Damage, 1)}\n";
		res += $"Range: {GetStatIncrease(baseStats.Range, modifiedStats.Range)}\n";
		res += $"Fire Rate: {GetStatIncrease(baseStats.FireRate, modifiedStats.FireRate, 1)}\n";
		res += $"Projectile Speed: {GetStatIncrease(baseStats.ProjectileSpeed, modifiedStats.ProjectileSpeed)}\n";
		res += $"Critical Hit Chance: {GetStatIncrease(baseStats.CritChance, modifiedStats.CritChance, 2, true, true)}\n";
		res += $"Critical Hit Multiplier: {GetStatIncrease(baseStats.CritMult, modifiedStats.CritMult, 1)}\n";
		res += $"Shot Spread Angle: {GetStatIncrease(baseStats.ShotSpread, modifiedStats.ShotSpread, 1)}\n";
		res += $"Shot Spread Tightness: {GetStatIncrease(baseStats.ShotSpreadFalloff, modifiedStats.ShotSpreadFalloff, 2, false, true)}\n";

		//display special stats if they are changed
		if(baseStats.ChainTargets != modifiedStats.ChainTargets) 
			res += $"Chain Targets: {GetStatIncrease(baseStats.ChainTargets, modifiedStats.ChainTargets, 0,  false)}\n";
		if (baseStats.ChainDistance != modifiedStats.ChainDistance)
			res += $"Chain Distance: {GetStatIncrease(baseStats.ChainDistance, modifiedStats.ChainDistance, 0, false)}\n";
		if (baseStats.SplashRadius != modifiedStats.SplashRadius)
			res += $"Splash Effect Radius: {GetStatIncrease(baseStats.SplashRadius, modifiedStats.SplashRadius, 0, false)}\n";
		if (baseStats.SplashCoef != modifiedStats.SplashCoef)
			res += $"Splash Effect Effectiveness: {GetStatIncrease(baseStats.SplashCoef, modifiedStats.SplashCoef, 2, false, true)}\n";
		if (baseStats.PoisonDamage != modifiedStats.PoisonDamage)
			res += $"Poison Damage: {GetStatIncrease(baseStats.PoisonDamage, modifiedStats.PoisonDamage, 1, false)}\n";
		if (baseStats.PoisonTicks != modifiedStats.PoisonTicks)
			res += $"Poison Ticks: {GetStatIncrease(baseStats.PoisonTicks, modifiedStats.PoisonTicks, 0, false)}\n";
		if (baseStats.PiercingAmount != modifiedStats.PiercingAmount)
			res += $"Piercing Amount: {GetStatIncrease(baseStats.PiercingAmount, modifiedStats.PiercingAmount, 0, false)}\n";
		if (baseStats.KnockbackAmount != modifiedStats.KnockbackAmount)
			res += $"Knockback Force: {GetStatIncrease(baseStats.KnockbackAmount, modifiedStats.KnockbackAmount, 1, false)}\n";
		if (baseStats.SlowdownPercent != modifiedStats.SlowdownPercent)
			res += $"Slowdown Amount: {GetStatIncrease(baseStats.SlowdownPercent, modifiedStats.SlowdownPercent, 2, false, true)}\n";
		if (baseStats.SlowdownLength != modifiedStats.SlowdownLength)
			res += $"Slowdown Length: {GetStatIncrease(baseStats.SlowdownLength, modifiedStats.SlowdownLength, 1, false)}\n";
		if (baseStats.HomingStrength != modifiedStats.HomingStrength)
			res += $"Homing Strength: {GetStatIncrease(baseStats.HomingStrength, modifiedStats.HomingStrength, 2, false, true)}\n";



		return res;
	}

	public string GetStatIncrease( float start, float real, int decimalPlaces = 0, bool showRealValue = true, bool showAsPercent = false)
	{
		float num = real - start;
		string plusminus = num <= 0 ? "" : "+";
		string decFormat = $"F{decimalPlaces}";
		string realVal = $" ({real.ToString(decFormat)})";

		if(showAsPercent)
		{
			realVal = showRealValue ? $" ({(real * 100).ToString(decFormat)}%)" : "" ;
			return $"{plusminus}{(num * 100).ToString(decFormat)}%{realVal}";
		}

		return $"{plusminus}{num.ToString(decFormat)}{realVal}";
	}


}
