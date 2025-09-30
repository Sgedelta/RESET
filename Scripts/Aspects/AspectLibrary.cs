using Godot;
using System;
using System.Collections.Generic;
using static Aspect;

public partial class AspectLibrary : Node
{
	//TEMPORARY class for aspect storage!
	[Export] Godot.Collections.Array<AspectTemplate> AspectTemplates;

	public static List<Aspect> AllAspects = new List<Aspect>{};
	public static readonly Dictionary<string, Aspect> ById = new();


	public override void _Ready()
	{
		AllAspects.Clear();
		ById.Clear();

		if (AspectTemplates == null) return;

		foreach (AspectTemplate template in AspectTemplates)
		{
			if (template == null || string.IsNullOrWhiteSpace(template._id))
			{
				GD.PushWarning("AspectTemplate missing or has empty _id");
				continue;
			}

			var a = new Aspect(template);
			AllAspects.Add(a);
			ById[template._id] = a;
		}
	}
}
