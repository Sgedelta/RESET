using Godot;
using System;
using System.Collections.Generic;
using static Aspect;

public static class AspectLibrary
{
	public static List<Aspect> AllAspects = new List<Aspect>
   {
		//We just need to add aspects like this
	   new Aspect("Rapid Fire", StatType.FireRate, ProjectileType.Regular, ModifierType.Multiply, 2f),
	   new Aspect("Powerful", StatType.Damage, ProjectileType.Regular, ModifierType.Add, 5f)
   };
}
