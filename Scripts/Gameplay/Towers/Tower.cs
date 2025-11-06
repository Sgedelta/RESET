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

	[Export] private string rangeDisplayPath = "RangeDisplay";
	private RangeDisplay rangeDisplay;


	public override void _Ready()
	{
		Targeting = GetNodeOrNull<TargetingComponent>("TargetingComponent");
		Shooter   = GetNodeOrNull<ShooterComponent>("ShooterComponent");
		rangeDisplay = GetNode<RangeDisplay>(rangeDisplayPath);
		rangeDisplay.Show = false;

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

		rangeDisplay?.UpdateSize(modifiedStats.Range);

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

		result.FireRate				= Mathf.Max(0.05f, result.FireRate);
		result.ProjectileSpeed		= Mathf.Max(0f, result.ProjectileSpeed);
		result.ShotSpread			= Mathf.Max(0.5f, result.ShotSpread);
		result.ShotSpreadFalloff	= Mathf.Max(0.1f, result.ShotSpreadFalloff);
		result.Range				= Mathf.Max(0, result.Range);
		result.Damage				= Mathf.Max(0f, result.Damage);
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
			rangeDisplay.Show = Pullout.Active;
		} 
		else
		{
			Pullout.ActiveTower = this;
			rangeDisplay.Show = true;
		}

		
	}

	public void ShowOrHideRange(bool state)
	{
		rangeDisplay.Show = state;
	}

	public string StatDisplayBBCode()
	{
		var sb = new System.Text.StringBuilder();
		sb.Append("[code]");

		sb.AppendLine("[font_size=0][/font_size]"); //the first line breaks on fill. so this has to exist?

        sb.AppendLine(FormatStatLine("Damage",               baseStats.Damage,            modifiedStats.Damage,            1));
        sb.AppendLine(FormatStatLine("Range",                baseStats.Range,             modifiedStats.Range));
        sb.AppendLine(FormatStatLine("Fire Rate",            baseStats.FireRate,          modifiedStats.FireRate,          1));
        sb.AppendLine(FormatStatLine("Projectile Speed",     baseStats.ProjectileSpeed,   modifiedStats.ProjectileSpeed));

        sb.AppendLine(FormatStatLine("Crit Chance",  baseStats.CritChance,        modifiedStats.CritChance,        2, showRealValue:true,  asPercent:true));
		sb.AppendLine(FormatStatLine("Crit Multiplier", baseStats.CritMult,       modifiedStats.CritMult,          1));
		sb.AppendLine(FormatStatLine("Shot Spread Angle",    baseStats.ShotSpread,        modifiedStats.ShotSpread,        1));
		sb.AppendLine(FormatStatLine("Shot Spread",baseStats.ShotSpreadFalloff, modifiedStats.ShotSpreadFalloff, 2, showRealValue:true,  asPercent:true));

		AppendIfDiff(sb, "Chain Targets",            baseStats.ChainTargets,    modifiedStats.ChainTargets,    0, showRealValue:false);
		AppendIfDiff(sb, "Chain Distance",           baseStats.ChainDistance,   modifiedStats.ChainDistance,   0, showRealValue:false);
		AppendIfDiff(sb, "Splash Radius",     baseStats.SplashRadius,    modifiedStats.SplashRadius,    0, showRealValue:false);
		AppendIfDiff(sb, "Splash Damage", baseStats.SplashCoef,   modifiedStats.SplashCoef,      2, showRealValue:true,  asPercent:true);
		AppendIfDiff(sb, "Poison Damage",            baseStats.PoisonDamage,    modifiedStats.PoisonDamage,    1, showRealValue:false);
		AppendIfDiff(sb, "Poison Ticks",             baseStats.PoisonTicks,     modifiedStats.PoisonTicks,     0, showRealValue:false);
		AppendIfDiff(sb, "Piercing Amount",          baseStats.PiercingAmount,  modifiedStats.PiercingAmount,  0, showRealValue:false);
		AppendIfDiff(sb, "Knockback Force",          baseStats.KnockbackAmount, modifiedStats.KnockbackAmount, 1, showRealValue:false);
		AppendIfDiff(sb, "Slowdown Amount",          baseStats.SlowdownPercent, modifiedStats.SlowdownPercent, 2, showRealValue:true,  asPercent:true);
		AppendIfDiff(sb, "Slowdown Length",          baseStats.SlowdownLength,  modifiedStats.SlowdownLength,  1, showRealValue:false);
		AppendIfDiff(sb, "Homing Strength",          baseStats.HomingStrength,  modifiedStats.HomingStrength,  2, showRealValue:true,  asPercent:true);

		// Close monospace
		sb.Append("[/code]");
		return sb.ToString();
	}

	// Helper to only add a line if changed
	private void AppendIfDiff(System.Text.StringBuilder sb, string name, float start, float real, int decimals = 0, bool showRealValue = true, bool asPercent = false)
	{
		if (System.Math.Abs(real - start) > 0.0001f)
			sb.AppendLine(FormatStatLine(name, start, real, decimals, showRealValue, asPercent));
	}

	// === Core formatter (left label, right aligned delta + final), colored +/– ===
	private string FormatStatLine(string label, float start, float real, int decimals = 0, bool showRealValue = true, bool asPercent = false)
	{
		float delta = real - start;

		// 1) Build plain strings (no bbcode) for alignment calculations
		string fmt = $"F{decimals}";
		string deltaPlain, realPlain = "";

		if (asPercent)
		{
			string pm = delta > 0 ? "+" : "";
			deltaPlain = $"{pm}{delta.ToString(fmt)}%";
			if (showRealValue)
				realPlain = $" ({real.ToString(fmt)}%)";
		}
		else
		{
			string pm = delta > 0 ? "+" : "";
			deltaPlain = $"{pm}{delta.ToString(fmt)}";
			if (showRealValue)
				realPlain = $" ({real.ToString(fmt)})";
		}

		string rightPlain = deltaPlain + realPlain;

		// 2) Make colored versions
		string colorHexPlus  = "#6CFF6C";  // green
		string colorHexMinus = "#FF6C6C";  // red
		string colorHexNeutral = "#DDDDDD"; // neutral grey for (real)
		string deltaColored =
			delta > 0 ? $"[color={colorHexPlus}]{deltaPlain}[/color]" :
			delta < 0 ? $"[color={colorHexMinus}]{deltaPlain}[/color]" :
						deltaPlain;

		string realColored = showRealValue
			? $"[color={colorHexNeutral}]{realPlain}[/color]"
			: "";

		string rightColored = deltaColored + realColored;

		// 3) Align: left column for label, right column for numbers
		// We’re in [code] monospace, so spaces are trustworthy.
		const int NAME_COL_WIDTH  = 28;   // tweak to taste
		const int RIGHT_COL_WIDTH = 18;   // tweak to taste

		string left = label.Length > NAME_COL_WIDTH
			? label.Substring(0, NAME_COL_WIDTH)
			: label;

		string leftPadded  = left.PadRight(NAME_COL_WIDTH, ' ');

		// Compute padding using *plain* text lengths (bbcode excluded)
		int pad = RIGHT_COL_WIDTH - rightPlain.Length;
		if (pad < 1) pad = 1; // at least one space

        return $"[fill]{leftPadded} {rightColored}[/fill]";
	}


	public string GetStatIncrease( float start, float real, int decimalPlaces = 0, bool showRealValue = true, bool showAsPercent = false)
	{
		float num = real - start;
		string plusminus = num <= 0 ? "" : "+";
		string decFormat = $"F{decimalPlaces}";
		string realVal = $" ({real.ToString(decFormat)})";

		// Certain stats go (typically) from 0-1, and serve as percentages. we display them as these.
		if(showAsPercent)
		{
			realVal = showRealValue ? $" ({(real * 100).ToString(decFormat)}%)" : "" ;
			return $"{plusminus}{(num * 100).ToString(decFormat)}%{realVal}";
		}

		return $"{plusminus}{num.ToString(decFormat)}{realVal}";
	}


}
