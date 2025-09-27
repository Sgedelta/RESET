using Godot;
using System;
using System.Collections.Generic;
using static Aspect;

public partial class AspectLibrary : Node
{
	//TEMPORARY class for aspect storage!
	[Export] Godot.Collections.Array<AspectTemplate> AspectTemplates;


	public static List<Aspect> AllAspects = new List<Aspect>{};


	public override void _Ready()
	{
		foreach (AspectTemplate template in AspectTemplates) 
		{
			AllAspects.Add(new Aspect(template));		
		}
	}
}
