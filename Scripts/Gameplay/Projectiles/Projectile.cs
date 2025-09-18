using Godot;
using System;

public partial class Projectile : Area2D
{
	[Export] public float Speed = 420f;
	private Enemy _target;
	private float _damage;

	public void Init(Vector2 from, Enemy target, float damage)
	{
		GlobalPosition = from;
		_target = target;
		_damage = damage;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_target == null || !IsInstanceValid(_target))
		{
			QueueFree();
			return;
		}

		// Fly straight at current target position (no prediction)
		Vector2 dir = (_target.GlobalPosition - GlobalPosition).Normalized();
		GlobalPosition += dir * Speed * (float)delta;

		// Cheap hit test – good enough to prove the loop
		if (GlobalPosition.DistanceTo(_target.GlobalPosition) < 8f)
		{
			_target.TakeDamage(_damage);
			QueueFree();
		}
	}
}
