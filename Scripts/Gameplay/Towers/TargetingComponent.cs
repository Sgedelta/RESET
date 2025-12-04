using Godot;
using System;
public enum TargetingMode
{
    First,
    Last,
    Closest,
    Strongest,
    Weakest
}
public partial class TargetingComponent : Node2D
{
	[Export] public float Range = 160f;
    [Export] public TargetingMode Mode = TargetingMode.First;


	public Enemy PickTarget(Vector2 origin)
	{
        /*var enemiesRoot = GameManager.Instance.EnemiesRoot;
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
		return best;*/

        var enemiesRoot = GameManager.Instance.EnemiesRoot;
        if (enemiesRoot == null) return null;

        Enemy bestEnemy = null;

        float bestValue;

        if (Mode == TargetingMode.Last)
        {
            bestValue = float.MinValue; // Looking for the max value
        }
        else
        {
            bestValue = float.MaxValue; // Looking for the min value
        }

        foreach (var child in enemiesRoot.GetChildren())
        {
            if (child is not Enemy e) continue;

            float dist = origin.DistanceTo(e.GlobalPosition);
            if (dist > Range) continue;

            float value = 0f;

            switch (Mode)
            {
                case TargetingMode.Closest:
                    value = dist;
                    if (value < bestValue)
                    {
                        bestValue = value;
                        bestEnemy = e;
                    }
                    break;

                case TargetingMode.Last:
                    value = dist;
                    if (value > bestValue)
                    {
                        bestValue = value;
                        bestEnemy = e;
                    }
                    break;

                case TargetingMode.Strongest:
                    value = e.MaxHp;    // or e.CurrentHealth
                    if (value > bestValue)
                    {
                        bestValue = value;
                        bestEnemy = e;
                    }
                    break;

                case TargetingMode.Weakest:
                    value = e.HP;
                    if(value < bestValue)
                    {
                        bestValue = value;
                        bestEnemy = e;
                    }
                    break;
                case TargetingMode.First:
                    // FIRST means the enemy that has progressed MOST along the path
                    value = e.Progress;  // YOU must expose this from your enemy!
                    if (value > bestValue)
                    {
                        bestValue = value;
                        bestEnemy = e;
                    }
                    break;

            }
        }

        return bestEnemy;
    }
}
