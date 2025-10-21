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

	//Slow information
	private float _slowPercent = 0f;
	private float _slowDuration = 0f;
	private float _slowTimer = 0f;
	private bool _isSlowed = false;

	public override void _Ready()
	{
		HP = MaxHp;

		// attack timer setup
		attackTimer = new Timer();
		attackTimer.WaitTime = AttackRate;
		attackTimer.OneShot = false;
		attackTimer.Timeout += OnAttackTimeout;
		AddChild(attackTimer);

		AddToGroup("enemies");
	}
	public override void _Process(double delta)
	{
		if (_isSlowed)
		{
			_slowTimer -= (float)delta;
			if (_slowTimer <= 0)
			{
				ResetSlow();
			}
		}
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

	public void ApplyDamageOverTime(float damagePerTick, float duration, float tickInterval)
	{
		AddChild(new PoisonEffect(this, damagePerTick, duration, tickInterval));
	}

	public void ApplySlow(float percent, float duration)
	{
		if (percent <= 0f || duration <= 0f) 
			return;

		_slowPercent = percent;
		_slowDuration = duration;
		_slowTimer = duration;
		_isSlowed = true;

		if (Follower != null)
			Follower.Speed *= (1f - percent);

		GD.Print($"[Enemy] Slowed by {percent * 100}% for {duration}s");
	}

	private void ResetSlow()
	{
		_isSlowed = false;
		_slowPercent = 0f;
		_slowDuration = 0f;

		if (Follower != null)
			Follower.Speed = 100f; // replace with base speed later
	}

	public void ApplyKnockback(Vector2 direction, float force)
	{
		GlobalPosition += direction.Normalized() * force;
		GD.Print($"[Enemy] Knocked back by {force}px");
	}

	public void ApplyPoison(float damagePerTick, int ticks)
	{
		if (damagePerTick <= 0f || ticks <= 0) return;

		float tickInterval = 1f; // 1 second per tick we can adjust this later 
		float duration = tickInterval * ticks;

		GD.Print($"[Enemy] Poison applied for {ticks} ticks at {damagePerTick} dmg/tick");
		AddChild(new PoisonEffect(this, damagePerTick, duration, tickInterval));
	}
}
