using Godot;
using System;

public partial class TargetingComponent : Node2D
{
	[Export] public float Range = 160f;
	[Export] public NodePath EnemiesRootPath; // Set to Run/Map/Enemies in Inspector

	public Enemy PickTarget(Vector2 origin)
	{
		var enemiesRoot = GetNodeOrNull<Node2D>(EnemiesRootPath);
		if (enemiesRoot == null) return null;

		Enemy best = null;
		float bestDist = float.MaxValue;

		foreach (var child in enemiesRoot.GetChildren())
		{
			if (child is Enemy e)
			{
				float d = origin.DistanceTo(e.GlobalPosition);
				if (d <= Range && d < bestDist)
				{
					bestDist = d;
					best = e;
				}
			}
		}
		return best;
	}
}
