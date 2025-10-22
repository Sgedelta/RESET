using Godot;
using System;
using System.Security.Cryptography;

public partial class Enemy : PathFollow2D
{
	[Signal] public delegate void EnemyDiedEventHandler(Enemy enemy);
	[Signal] public delegate void EnemyAttackedEventHandler(Enemy enemy, float damage);
	[Signal] public delegate void EnemyReachedEndEventHandler(Enemy enemy);

	private Path2D _path;
	private Curve2D _curve;

	[Export] public float MaxHp = 30f;         // max health
	[Export] public float AttackRate = 1.2f;   // time between attacks in seconds
	[Export] public float AttackDamage = 3f;   // damage per attack

	[Export] public float Speed = 80f;
	private bool _calledReachedEnd = false;
	public bool ReachedEnd { get { return ProgressRatio >= 1; } }

	public float HP;
	private Timer attackTimer;

	[Export] private AnimatedSprite2D _sprite;

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
		EnemyReachedEnd += (e) => attackTimer.Start();
		EnemyReachedEnd += (e) => {_sprite.Play("Attack");};
		AddChild(attackTimer);

		AddToGroup("enemies");

		_sprite.Play("Idle");
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

		Progress += Speed * (float)delta * (1-_slowPercent);
		
		if(ReachedEnd && !_calledReachedEnd)
		{
			_calledReachedEnd = true;
			EmitSignal(SignalName.EnemyReachedEnd, this);
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

	private void OnAttackTimeout()
	{
		EmitSignal(SignalName.EnemyAttacked, this, AttackDamage);
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

		GD.Print($"[Enemy] Slowed by {percent * 100}% for {duration}s");
	}

	private void ResetSlow()
	{
		_isSlowed = false;
		_slowPercent = 0f;
		_slowDuration = 0f;

	}

	public void SetPathAndCurve(Path2D path)
	{
		_path = path;
		_curve = _path?.Curve;
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

	/// <summary>
	/// Method to read ahead the follow's position by timeAhead milliseconds.
	/// </summary>
	/// <param name="timeAhead">milliseconds to read ahead</param>
	/// <returns></returns>
	public Vector2 GetFuturePosition(float timeAhead)
	{
		Vector2 pos = new Vector2();

		pos += _curve.SampleBaked(Progress + (Speed * timeAhead * (1-_slowPercent)));

		return pos;
	}
}
