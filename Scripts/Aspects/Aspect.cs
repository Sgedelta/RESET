using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;

public enum StatType
{
	//basic and advanced stats from 0 - 999
	FireRate,
	Damage,
	Range,
	CritChance,
	CritMult,
	SpreadAngle,
	SpreadFalloff,



	//TODO - detail better as specific stats
	//unique stats and misc from 1000+ - there is almost no chance we need more than 1000 basic stats, and we can hardcode check value for < 1000 if needbe
	SplashCoef = 1000,
	SplashRadius,
	PoisonDamage,
	PoisonTicks,
	ChainTargets,
	ChainDistance,
	PiercingAmount,
	KnockbackAmount,
	SlowdownPercent,
	SlowdownLength,
	HomingStrength,

	//Super Special Zone
	//	Used for internal use - such as RANDOM, which will never be directly on an aspect, but is used in ModifierInfo/AspectTemplates
	//	using 10,000+, because there should never be more than that and it's less than int MaxVal
	//	All of these should be ALL_CAPS_UNDERSCORES
	RANDOM = 10000,
}	

public enum ModifierType
{
	Add,
	Multiply,
	Set
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

	public static Dictionary<string, int> CreatedAspectsOfType = new Dictionary<string, int>();

 	public AspectTemplate Template { get; }
	public List<ModifierUnit> Modifiers { get; } = new();

	private string _id;
	public string ID { get { return _id; } }
	
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
				case ModifierInfo modInfo:
					ModifierUnit mUnit = new ModifierUnit
					{
						Stat = modInfo.ModifiedStat,
						Type = modInfo.ModifierType,
						Value = modInfo.GetStat()
					};
					Modifiers.Add(mUnit);
				break;

				case null:
					GD.PushWarning($"ModifierInfo is null in {Template._id}");
					break;

				default:
					GD.PushWarning($"ModifierInfo type not handled: {info?.ModifiedStat}, {info?.GetType().Name}");
					break;
			}
		}

		//check for first of kind
		if(!CreatedAspectsOfType.ContainsKey(Template._id))
		{
			CreatedAspectsOfType.Add(Template._id, 0);
		}

		_id = $"{Template._id}_{CreatedAspectsOfType[Template._id].ToString()}";
		CreatedAspectsOfType[Template._id] += 1;


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
					newStats.ShotSpreadFalloff = ApplyFloat(newStats.ShotSpreadFalloff, unit);
					break;

				case StatType.CritChance :
					newStats.CritChance = ApplyFloat(newStats.CritChance, unit);
					break;

				case StatType.CritMult :
					newStats.CritMult = ApplyFloat(newStats.CritMult, unit);
					break;

				case StatType.SplashCoef :
					newStats.SplashCoef = ApplyFloat(newStats.SplashCoef, unit);
					break;

				case StatType.SplashRadius :
					newStats.SplashRadius = ApplyFloat(newStats.SplashRadius, unit);
					break;

				case StatType.PoisonDamage :
					newStats.PoisonDamage = ApplyFloat(newStats.PoisonDamage, unit);
					break;

				case StatType.PoisonTicks : //int
					if(unit.Type == ModifierType.Multiply)
					{
						newStats.PoisonTicks = (int)Mathf.Round(ApplyFloat(newStats.PoisonTicks, unit));
					} else 
					{
						newStats.PoisonTicks = ApplyInt(newStats.PoisonTicks, unit);
					}
					
					break;

				case StatType.ChainTargets : //int
					if (unit.Type == ModifierType.Multiply)
					{
						newStats.ChainTargets = (int)Mathf.Round(ApplyFloat(newStats.ChainTargets, unit));
					} else
					{
						newStats.ChainTargets = ApplyInt(newStats.ChainTargets, unit);
					}
					break;

				case StatType.ChainDistance :
					newStats.ChainDistance = ApplyFloat(newStats.ChainDistance, unit);
					break;

				case StatType.PiercingAmount : //int
					if (unit.Type == ModifierType.Multiply)
					{
						newStats.PiercingAmount = (int)Mathf.Round(ApplyFloat(newStats.PiercingAmount, unit));
					} else
					{
						newStats.PiercingAmount = ApplyInt(newStats.PiercingAmount, unit);
					}
						
					break;

				case StatType.KnockbackAmount :
					newStats.KnockbackAmount = ApplyFloat(newStats.KnockbackAmount, unit);
					break;

				case StatType.SlowdownPercent :
					newStats.SlowdownPercent = ApplyFloat(newStats.SlowdownPercent, unit);
					break;

				case StatType.SlowdownLength :
					newStats.SlowdownLength = ApplyFloat(newStats.SlowdownLength, unit);
					break;

				case StatType.HomingStrength :
					newStats.HomingStrength = ApplyFloat(newStats.HomingStrength, unit);
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
		float modVal = unit.Value;
		switch (unit.Type)
		{
			case ModifierType.Add:
				current = current + modVal;
				break;
			
			case ModifierType.Multiply:
				if (current == 0)
				{
					GD.Print("Setting value to default");
					//stats that start as percentages start at a default value of 1%
					if(unit.Stat == StatType.CritChance ||
						unit.Stat == StatType.SpreadFalloff ||
						unit.Stat == StatType.SplashCoef ||
						unit.Stat == StatType.SlowdownPercent ||
						unit.Stat == StatType.HomingStrength)
					{
						current = 0.01f;
					} 
					else //other stats use a default of 1
					{
						current = 1;
					}
				}
				current = current * modVal;
				break;
			
		};
		return current;
	}

	static int ApplyInt(int current, ModifierUnit unit) {
		int modVal = (int)unit.Value;
		return unit.Type switch
		{
			ModifierType.Add => current + modVal,
			ModifierType.Multiply => current * modVal,
			_ => current
		};
	}
}
