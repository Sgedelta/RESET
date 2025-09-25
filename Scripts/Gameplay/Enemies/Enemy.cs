using Godot;
using System;

public partial class Enemy : Node2D
{
	[Signal] public delegate void EnemyDiedEventHandler(Enemy enemy);
	
	[Export] public float MaxHp = 30f;
	[Export] public PathFollower Follower;
	public float HP;

	public override void _Ready()
	{
		HP = MaxHp;
	}

	public void TakeDamage(float dmg)
	{
		HP -= dmg;
		if (HP <= 0f) 
		{
			EmitSignal(SignalName.EnemyDied, this);
			QueueFree();
		}
	}
	
	public void SetPath(Path2D path)
	{
		Follower.SetPath(path);
	}
}
