using Godot;
using System;

public partial class Enemy : Node2D
{
	[Signal] public delegate void EnemyDiedEventHandler(Enemy enemy);
	[Signal] public delegate void EnemyAttackedEventHandler(Enemy enemy, float damage);

	[Export] public float MaxHp = 30f;         // max health
	[Export] public float AttackRate = 1.2f;   // time between attacks in seconds
	[Export] public float AttackDamage = 3f;   // damage per attack
	[Export] public PathFollower Follower;

	public float HP;
	private Timer attackTimer;

	public override void _Ready()
	{
		HP = MaxHp;

		// attack timer setup
		attackTimer = new Timer();
		attackTimer.WaitTime = AttackRate;
		attackTimer.OneShot = false;
		attackTimer.Timeout += OnAttackTimeout;
		AddChild(attackTimer);
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

	public void OnReachedPathEnd()
	{
		attackTimer.Start();
	}

	private void OnAttackTimeout()
	{
		EmitSignal(SignalName.EnemyAttacked, this, AttackDamage);
	}

	public void SetPath(Path2D path)
	{
		Follower.SetPath(path);
	}
}
