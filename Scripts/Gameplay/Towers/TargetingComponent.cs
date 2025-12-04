using Godot;
using System;
public enum TargetingMode
{
	First,
	Last,
	Closest,
	Farthest,
	Strongest,
	Weakest,
	Fastest,
	Slowest,
	Dangerous
}
public partial class TargetingComponent : Node2D
{
	[Export] public float Range = 160f;
	[Export] public TargetingMode Mode = TargetingMode.First;

	//weight values used by some targeting heuristics (such as Dangerous)
	private float _HealthWeight = 1;
	private float _MaxHealthWeight = .33f;
	private float _DistanceWeight = 2;
	private float _SpeedWeight = 2;
	private float _DPSWeight = 2;

	private float _HighestEverSeenHealth = 0; //Note: Potentially reset this between rounds to avoid things like bosses breaking the scale totally?
	private float _HighestEverSeenDPS = 0; //Note: Reset as above?
	private float _HighestEverSeenSpeed = 0; //note: as above

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

		//any mode looking for the highest value
		if (Mode == TargetingMode.First || 
			Mode == TargetingMode.Strongest || 
			Mode == TargetingMode.Dangerous ||
			Mode == TargetingMode.Farthest ||
			Mode == TargetingMode.Fastest )
		{
			bestValue = float.MinValue; // Looking for the max value
		}
		else //all other modes (lowest value)
		{
			bestValue = float.MaxValue; // Looking for the min value
		}

		foreach (var child in enemiesRoot.GetChildren())
		{
			if (child is not Enemy e) continue;

			float dist = origin.DistanceTo(e.GlobalPosition);
			if (dist > Range) continue;

			float value = 0f;

			float dps = 0; //used in multiple calculations, should be calculated there for effeciency

			switch (Mode)
			{
				//targets the enemy closest to the tower
				case TargetingMode.Closest:
					value = dist;
					if (value < bestValue)
					{
						bestValue = value;
						bestEnemy = e;
					}
					break;
				//targets the enemy farthest from the tower
				case TargetingMode.Farthest:
					value = dist;
					if (value > bestValue)
					{
						bestValue = value;
						bestEnemy = e;
					}
					break;
				//targets the enemy the furthest back along the path
				case TargetingMode.Last:
					value = e.ProgressRatio;
					if (value < bestValue)
					{
						bestValue = value;
						bestEnemy = e;
					}
					break;
				//targets the enemy that has traveled the furthest along the path
				case TargetingMode.First:
					value = e.ProgressRatio; 
					if (value > bestValue)
					{
						bestValue = value;
						bestEnemy = e;
					}
					break;
				//considers both max and current health, according to their weights
				case TargetingMode.Strongest:
					value = e.MaxHp * _MaxHealthWeight + e.HP * _HealthWeight; 
					if (value > bestValue)
					{
						bestValue = value;
						bestEnemy = e;
					}
					break;
				//considers only current health
				case TargetingMode.Weakest:
					value = e.HP;
					if(value < bestValue)
					{
						bestValue = value;
						bestEnemy = e;
					}
					break;

				//considers both max and current health, according to their weights
				case TargetingMode.Fastest:
					value = e.Speed;
					if (value > bestValue)
					{
						bestValue = value;
						bestEnemy = e;
					}
					break;
				//considers only current health
				case TargetingMode.Slowest:
					value = e.Speed;
					if (value < bestValue)
					{
						bestValue = value;
						bestEnemy = e;
					}
					break;

				//the most "dangerous" enemy, analyzed by a few factors - distance to end, speed, potential damage, remaining health, etc. 
				case TargetingMode.Dangerous:

					dps = e.AttackDamage * (1 / e.AttackRate);

					//check updating max seens for some calculations
					if(e.HP > _HighestEverSeenHealth)
					{
						_HighestEverSeenHealth = e.HP;
					}
					if(dps > _HighestEverSeenDPS)
					{
						_HighestEverSeenDPS = dps;
					}
					if (e.Speed > _HighestEverSeenSpeed) 
					{
						_HighestEverSeenSpeed = e.Speed;
					}

					//enemies with high relative health are more dangerous than those that have been hit previously
					value += (e.HP / e.MaxHp) * _HealthWeight;
					//enemies who have maxHP close to the highest we've seen are dangerous
					value += (e.MaxHp / _HighestEverSeenHealth) * _MaxHealthWeight;
					//enemies who are further along the path are more dangerous
					value += e.ProgressRatio * _DistanceWeight;
					//enemies who do more damage are more dangerous
					value += (dps / _HighestEverSeenDPS) * _DPSWeight;
					//enemies who travel faster are more dangerous
					value += (e.Speed / _HighestEverSeenSpeed) * _SpeedWeight;


					if(value > bestValue)
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
