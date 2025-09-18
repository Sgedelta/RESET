using Godot;
using System;

public partial class Projectile : Area2D
{
	[Export] public float Speed = 420f; //speed, in pixels
	private Enemy _target;
	private float _damage;

	private Vector2 dir;

	public void Init(Vector2 from, Enemy target, float damage)
	{
		GlobalPosition = from;
		_target = target;
		_damage = damage;

		//do a basic readahead based on distance to target - this could cause issues if the target is very far away and fast moving, and we might need to revisit
		float timeToTarget = ((_target.GlobalPosition - from).Length()) / Speed;
		dir = (target.Follower.GetFuturePosition(timeToTarget / 1000) - from).Normalized();

	}

	public override void _PhysicsProcess(double delta)
	{
		if (_target == null || !IsInstanceValid(_target))
		{
			QueueFree();
			return;
		}

		// Fly straight at current target position (no prediction)
		//Vector2 dir = (_target.GlobalPosition - GlobalPosition).Normalized();
		GlobalPosition += dir * Speed * (float)delta;
		

		// Cheap hit test – good enough to prove the loop
		if (GlobalPosition.DistanceTo(_target.GlobalPosition) < 8f)
		{
			_target.TakeDamage(_damage);
			QueueFree();
		}
	}
}
