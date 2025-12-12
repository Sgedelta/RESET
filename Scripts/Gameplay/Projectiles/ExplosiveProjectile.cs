using Godot;
using System;

public partial class ExplosiveProjectile : Projectile
{
	[Export] public float ExplosionRadius = 64f; // radius in pixels
	[Export] public bool HasFalloff = false;     // optional: damage decreases with distance

	public override void _PhysicsProcess(double delta)
	{
		if (_target == null || !IsInstanceValid(_target))
		{
			QueueFree();
			return;
		}

		GlobalPosition += dir * Speed * (float)delta;

		float hitRadius = 32f;
		float distToTarget = GlobalPosition.DistanceTo(_target.GlobalPosition);

		if (distToTarget < hitRadius)
		{
			Explode();
			QueueFree();
		}
	}

	private void Explode()
	{

		var enemiesRoot = GetTree().Root.FindChild("Enemies", true, false) as Node2D;
		if (enemiesRoot == null)
		{
			return;
		}

		foreach (var child in enemiesRoot.GetChildren())
		{
			if (child is Enemy e && IsInstanceValid(e))
			{
				float dist = GlobalPosition.DistanceTo(e.GlobalPosition);

				if (dist <= ExplosionRadius)
				{
					float dmg = _damage;
					if (HasFalloff)
					{
						float factor = Mathf.Clamp(1f - (dist / ExplosionRadius), 0.5f, 1f);
						dmg *= factor;
					}
					else
					{
						GD.Print($"[ExplosiveProjectile] Applying damage {dmg} to {e.Name}");
					}

					e.TakeDamage(dmg);
				}
			}
		}
	}
}
