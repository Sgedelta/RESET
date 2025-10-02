using Godot;
using System;
using System.Collections.Generic;

public enum StatType
{
	//basic and advanced stats from 0 - 999
		FireRate,
		Damage,
		Range,
		Accuracy,
		CritChance,
		CritMult,
		SpreadAngle,
		SpreadFalloff,



		//TODO - detail better as specific stats
		//unique stats and misc from 1000+ - there is almost no chance we need more than 1000 basic stats, and we can hardcode check value for < 1000 if needbe
		SplashDamage = 1000,
		SplashRadius,
		PoisonDamage,
		PoisonTicks,
		ChainTargets,
		ChainDistance,
		PiercingAmount,
		KnockbackAmount,
		SlowdownPercent,
		SlowdownLength,
		HomingStrength
}	

public enum ModifierType
{
	Add,
	Multiply,
	Subtract
}
public enum ProjectileType
{
	Regular,
	Homing,
	Explosive,
	Piercing,
	Poison,
	Chain
}



public class Aspect
{

 	public AspectTemplate Template { get; }
	public List<ModifierUnit> Modifiers { get; } = new();
	
	// prevent build from happening twice
	private bool _initializedFromTemplate = false;

	//TODO: redefine to be able to effect multiple stats
	//       and take an AspectTemplate and create self based on data within?
	//       can likely use ModifierInfo class (or slightly modified version
	public Aspect( AspectTemplate template)
	{
		 Template = template ?? throw new ArgumentNullException(nameof(template));
		BuildFromTemplate();
	}
	
	private void BuildFromTemplate()
	{
		if (_initializedFromTemplate || Template.Modifiers == null) return;

		foreach (var info in Template.Modifiers)
		{
			switch (info)
			{
				case FloatModifierInfo fmi:
					var unit = new FloatModifierUnit
					{
						Stat = fmi.StatType,
						Type = fmi.ModifierType,
						Value = (float)fmi.GetStat()
					};
					Modifiers.Add(unit);
					break;

				default:
					GD.PushWarning($"ModifierInfo type not handled: {info?.GetType().Name}");
					break;
			}
		}
		_initializedFromTemplate = true;
	}


	public TowerStats ModifyGivenStats(TowerStats stats)
	{
		TowerStats newStats = stats;

		//apply modifiers for each aspect in order

		foreach (ModifierUnit unit in Modifiers)
		{
			switch(unit.Stat)
			{
				case StatType.Damage:
					newStats.Damage = ApplyFloat(newStats.Damage, unit);
					break;

				case StatType.FireRate:
					newStats.FireRate = ApplyFloat(newStats.FireRate, unit);
					break;

				case StatType.Range:
					newStats.Range = ApplyFloat(newStats.Range, unit);
					break;

				case StatType.SpreadAngle:
					newStats.ShotSpread = ApplyFloat(newStats.ShotSpread, unit);
					break;

				case StatType.SpreadFalloff :
					GD.PushWarning("Modification Not Implemented");
					break;

				case StatType.CritChance :
					GD.PushWarning("Modification Not Implemented");
					break;

				case StatType.CritMult :
					GD.PushWarning("Modification Not Implemented");
					break;

				case StatType.SplashDamage :
					GD.PushWarning("Modification Not Implemented");
					break;

				case StatType.SplashRadius :
					GD.PushWarning("Modification Not Implemented");
					break;

				case StatType.PoisonDamage :
					GD.PushWarning("Modification Not Implemented");
					break;

				case StatType.PoisonTicks :
					GD.PushWarning("Modification Not Implemented");
					break;

				case StatType.ChainTargets :
					GD.PushWarning("Modification Not Implemented");
					break;

				case StatType.ChainDistance :
					GD.PushWarning("Modification Not Implemented");
					break;

				case StatType.PiercingAmount :
					GD.PushWarning("Modification Not Implemented");
					break;

				case StatType.KnockbackAmount :
					GD.PushWarning("Modification Not Implemented");
					break;

				case StatType.SlowdownPercent :
					GD.PushWarning("Modification Not Implemented");
					break;

				case StatType.SlowdownLength :
					GD.PushWarning("Modification Not Implemented");
					break;

				case StatType.HomingStrength :
					GD.PushWarning("Modification Not Implemented");
					break;

				default:
					GD.PushWarning("Modification Not Recognized!");
					break;

			}
		}
		return newStats;
	}

	static float ApplyFloat(float current, ModifierUnit unit)
	{
		float modVal = ((FloatModifierUnit)unit).Value;
		return unit.Type switch
		{
			ModifierType.Add => current + modVal,
			ModifierType.Multiply => current * modVal,
			ModifierType.Subtract => current - modVal,
			_ => current
		};
	}
}
