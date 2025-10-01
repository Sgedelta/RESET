using Godot;
using System;

public partial class Enemy : Node2D
{
	[Signal] public delegate void EnemyDiedEventHandler(Enemy enemy);
	
	[Export] public float MaxHp = 30f;         // max health
	[Export] public float AttackRate = 1.2f;   // time between attacks in seconds
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

		AddToGroup("enemies");
	}

	public void TakeDamage(float dmg)
	{
		HP -= dmg;
		if (HP <= 0f) 
		{
			EmitSignal(SignalName.EnemyDied, this);
			QueueFree();
		}
		GD.Print("Enemy has " + HP);

    }
	
	public void OnReachedPathEnd()
	{
		GD.Print($"{Name} reached path end");
		attackTimer.Start();
	}

	private void OnAttackTimeout()
	{
		GD.Print($"{Name} attacks");
	}
	
	public void SetPath(Path2D path)
	{
		Follower.SetPath(path);
	}

	public void ApplyDamageOverTime(float damagePerTick, float duration, float tickInterval)
	{
        AddChild(new PoisonEffect(this, damagePerTick, duration, tickInterval));
    }
}
