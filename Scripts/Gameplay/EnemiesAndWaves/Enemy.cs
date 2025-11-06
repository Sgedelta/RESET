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
	private bool _isSlowed = false;

	//Damage indicator 
	[Export] public PackedScene DamageIndicatorScene;

	Color damageColor;

	private float _knockbackVelocity = 0f;
	private float _knockbackDecay = 750.0f;
	private const float KNOCKBACK_SCALE = 100; //number in pixels/sec that 1 unit of knockback applies to a enemy of mass 1
	[Export] private float _mass = 1; //serves as a divisor for knockback calculations
	[Export] private float _kbScalar = 1; //serves as a float % of knockback resistance for this enemy - lower means more resistant, higher means more vulnerable


    public override void _Ready()
	{
		HP = MaxHp;

		// attack timer setup
		attackTimer = new Timer();
		attackTimer.WaitTime = AttackRate;
		attackTimer.OneShot = false;
		attackTimer.Timeout += OnAttackTimeout;
		EnemyReachedEnd += (e) => { attackTimer.Start(); };
		EnemyReachedEnd += (e) => {_sprite.Play("Attack");};
		AddChild(attackTimer);

		AddToGroup("enemies");

		_sprite.Play("Idle");
	}
	public override void _Process(double delta)
	{
		if (_isSlowed)
		{
			_slowDuration -= (float)delta;
			if (_slowDuration <= 0)
			{
				ResetSlow();
			}
		}

		if (Mathf.Abs(_knockbackVelocity) > 0.01f)
		{
			Progress += _knockbackVelocity * (float)delta;

			_knockbackVelocity = Mathf.MoveToward(_knockbackVelocity, 0, _knockbackDecay * (float)delta);
		}
		else
		{
			Progress += Speed * (float)delta * (1 - _slowPercent);
		}


		if(ReachedEnd && !_calledReachedEnd)
		{
			_calledReachedEnd = true;
			EmitSignal(SignalName.EnemyReachedEnd, this);
		}
	}

	public void TakeDamage(float dmg, DamageType type)
	{
		HP -= dmg;
		ShowDamageIndicator(dmg, type);
		if (HP <= 0f) 
		{
			EmitSignal(SignalName.EnemyDied, this);
			QueueFree();
		}

	}

	public void TakeDamage(float dmg)
	{
		TakeDamage(dmg, DamageType.Normal);
	}


	private void ShowDamageIndicator(float dmg, DamageType type)
	{
		//Make Damage Type enum - set indicator color off damage type?
		if (DamageIndicatorScene == null)
		{
			GD.PushWarning("Damage indicator failed due to scene being null. Took " + dmg + " damage.");
			return;
		}

		if(type == DamageType.Posion)
		{
			damageColor = new Color(0.0f, 1.0f,0.0f);
		}
		else
		{
			damageColor = new Color(1.0f, 0.7f, 0.0f);
		}
			
		var indicator = (DamageIndicator)DamageIndicatorScene.Instantiate();
		GetTree().CurrentScene.AddChild(indicator);

		
		indicator.GlobalPosition = GlobalPosition + new Vector2(0, -20);
		indicator.SetDamage(dmg, damageColor);


	}

	private void OnAttackTimeout()
	{
		EmitSignal(SignalName.EnemyAttacked, this, AttackDamage);
		GD.Print("Attack Timeout!");
	}

	public void ApplyDamageOverTime(float damagePerTick, float duration, float tickInterval)
	{
		AddChild(new PoisonEffect(this, damagePerTick, duration, tickInterval));
	}

	public void ApplySlow(float percent, float duration)
	{
		if (percent <= 0f || duration <= 0f) 
			return;
		
		//check if we are currently slowing
		if(_isSlowed)
		{
			//we have a stronger slow
			if(_slowPercent > percent)
			{
				//add to that slow by an "equivalent amount"
				float ratio = percent / _slowPercent;
				_slowDuration += ratio * duration;
			}
			else
			{
				//otherwise our new slow is better - make the old slow add to this by a equivalent amount
				float ratio = _slowPercent / percent;
				_slowDuration = duration + _slowDuration * ratio;
				_slowPercent = percent;
			}
		}
		else //otherwise apply slow
		{
            _slowPercent = percent;

            _slowDuration = duration;
            _isSlowed = true;
        }


		GD.Print($"[Enemy] Slowed by {percent * 100}% for {duration}s");
	}

	private void ResetSlow()
	{
		_isSlowed = false;
		_slowPercent = 0f;

	}

	public void SetPathAndCurve(Path2D path)
	{
		_path = path;
		_curve = _path?.Curve;
	}

	public void ApplyKnockback(Vector2 direction, float force)
	{
		_knockbackVelocity = -(force * KNOCKBACK_SCALE) * _kbScalar / _mass;
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
	
	public void ModifyStats(float healthMultiplier, float speedMultiplier)
	{
		MaxHp *= healthMultiplier;
		Speed *= speedMultiplier;
	}
}
