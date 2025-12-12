using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class PiercingProjectile : Projectile
{
	[Export] public float maxDistance = 600f;
	[Export] public float PierceFalloff = 0.8f;
	[Export] public float hitRadius = 48f; 

	private Vector2 _startpos;
	private HashSet<Enemy> _hitEnemies = new();

	public override void _Ready()
	{
		_startpos = GlobalPosition;
		Monitoring = true; 
	}

	public override void _PhysicsProcess(double delta)
	{
		GlobalPosition += dir * Speed * (float)delta;

		float traveled = GlobalPosition.DistanceTo(_startpos);
		if (traveled > maxDistance)
		{
			QueueFree();
			return;
		}

		var enemies = GetTree().GetNodesInGroup("enemies").OfType<Enemy>();
		foreach (var enemy in enemies)
		{
			if (!_hitEnemies.Contains(enemy))
			{
				float dist = GlobalPosition.DistanceTo(enemy.GlobalPosition);

			 

				if (dist <= hitRadius)
				{
					_hitEnemies.Add(enemy);
					enemy.TakeDamage(_damage);

					GD.Print($"[PiercingProjectile] HIT {enemy.Name}, damage: {_damage}");

					_damage *= PierceFalloff;
					GD.Print($"[PiercingProjectile] Damage after falloff: {_damage}");

					if (_damage < 1f)
					{
						QueueFree();
						return;
					}
				}
			}
		}
	}
}
