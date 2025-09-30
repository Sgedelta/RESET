using Godot;
using System;


	public enum StatType
	{
		//basic and advanced stats from 0 - 99
		FireRate,
		Damage,
		Range,
		Accuracy,
		Spread,


		//TODO - detail better as specific stats
		//unique stats and misc from 100+ - there is almost no chance we need more than 1000 basic stats, and we can hardcode check value for <1000 if needbe
		Splash = 1000,
		Poison,
		Chain,
		Piercing,
		Knockback,
		Critical,
		Slow,
		Homing
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
		public string Name { get; private set; }
		public StatType Stat { get; private set; }
		public ProjectileType ProjectileType { get; private set; }

		public ModifierType Modifier { get; private set; }
		public float Value { get; private set; }


		//TODO: redefine to be able to effect multiple stats
		//       and take an AspectTemplate and create self based on data within?
		//       can likely use ModifierInfo class (or slightly modified version
		public Aspect(string name, StatType stat, ProjectileType projectileType, ModifierType modifier, float value)
		{
			Name = name;
			Stat = stat;
			ProjectileType = projectileType;
			Modifier = modifier;
			Value = value;
		}
	}
