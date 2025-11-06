using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static System.Net.Mime.MediaTypeNames;


public enum DamageType
{
	Normal,
	Posion,
	Splash,
	Chain
}

public partial class Projectile : Area2D
{
	[Export] public float Speed = 800f; //speed, in pixels
	protected Enemy _target;
	protected float _damage;

	// how frequently a projectile will retarget to find the nearest enemy, in seconds
	private const float PROJ_RETARGET_SPEED = .1f;
	private const float PERFECT_TURN_TIME = .6f; //Scalar for homing speed tuning - roughly correlates to time, but not exactly
	private float _timeSinceRetarget = float.MaxValue;

	protected Vector2 dir;

	private float _critChance;
	private float _critMult;
	private int _chainTargets;
	private float _chainDistance;
	private float _splashRadius;
	private float _splashCoef;
	private float _poisonDamage;
	private int _poisonTicks;
	private int _piercingAmount;
	private float _knockbackAmount;
	private float _slowdownPercent;
	private float _slowdownLength;
	private float _homingStrength;

	//Internal stats 
	private HashSet<Enemy> _hitEnemies = new(); //For chain projectiles and exploding 
	private bool _hasExploded = false;
	private float _homingTurnSpeed; 

	/// <summary>
	/// Initializes a Projectile so that it fires correctly
	/// </summary>
	public void Init(Vector2 initialDir, TowerStats stats)
	{
		dir = initialDir;


		_damage = stats.Damage;
		Speed = stats.ProjectileSpeed;
		
		_critChance = stats.CritChance;
		_critMult = stats.CritMult;
		_chainTargets = stats.ChainTargets;
		_chainDistance = stats.ChainDistance;
		_splashRadius = stats.SplashRadius;
		_splashCoef = stats.SplashCoef;
		_poisonDamage = stats.PoisonDamage;
		_poisonTicks = stats.PoisonTicks;
		_piercingAmount = stats.PiercingAmount;
		_knockbackAmount = stats.KnockbackAmount;
		_slowdownPercent = stats.SlowdownPercent;
		_slowdownLength = stats.SlowdownLength;
		_homingStrength = stats.HomingStrength;


		_homingTurnSpeed = Mathf.Clamp(_homingStrength / 50f, 0f, 1f);
	}


	public override void _PhysicsProcess(double delta)
	{
		
		//Located Nearest Enemy if we don't have a target or if time has passed
		if(_target == null || !IsInstanceValid(_target) || _timeSinceRetarget > PROJ_RETARGET_SPEED)
		{
			_target = GameManager.Instance.GetNearestEnemyToPoint(GlobalPosition);
			_timeSinceRetarget = 0;
		}
		_timeSinceRetarget += (float)delta;

		//Homing Logic 
		if(_homingStrength > 0f && _target != null && IsInstanceValid(_target))
		{
			Vector2 desiredDir = (_target.GlobalPosition - GlobalPosition).Normalized();

			dir = dir.Lerp(desiredDir, _homingStrength * (float)delta / PROJ_RETARGET_SPEED).Normalized();
		}

		GlobalPosition += dir * Speed * (float)delta;

		
	}

	private void OnAreaEntered(Area2D other)
	{

		if(other.GetParent<Enemy>() == null)
		{
			return;
		}

		OnHit(other.GetParent<Enemy>());
	}

	private void OnHit(Enemy enemy, bool allowDestroy = true, bool doChain = true, bool doSplash = true, bool doPierce = true)
	{
		if(enemy == null || !IsInstanceValid(enemy))
		{
			return;
		}

		//GD.Print($"Hit {enemy} with flags {allowDestroy}, {doChain}, {doSplash}, {doPierce}");

		if (_hitEnemies.Contains(enemy))
		{
			return;
		}
			

		_hitEnemies.Add(enemy);

		//check crit
		float hitDamage = _damage;
		bool wasCrit = GD.Randf() <= _critChance;

        if (wasCrit)
		{
			hitDamage *= _critMult;
		}

		enemy.TakeDamage(hitDamage, wasCrit);

		ApplyKnockback(enemy);
		ApplySlow(enemy);
		ApplyPoison(enemy);

		if(doSplash && _splashRadius > 0 && !_hasExploded)
		{
			ExplodeSplash(hitDamage);
			//_hasExploded = true;
		}

		if(doChain && _chainTargets > 0)
		{
			ChainToNextTargets(enemy);
		}

		if(doPierce)
		{
			_piercingAmount--;
		} 
		if (allowDestroy && _piercingAmount <= 0)
		{
			QueueFree();
		}

	}

	private void ApplyKnockback(Enemy enemy, float coef = 1)
	{
		if (_knockbackAmount <= 0) return;

		Vector2 knockDir = (enemy.GlobalPosition - GlobalPosition).Normalized();
		enemy.ApplyKnockback(knockDir, _knockbackAmount * coef);
	}

	private void ApplySlow(Enemy enemy, float coef = 1)
	{
		if (_slowdownPercent <= 0 || _slowdownLength <= 0)
		{

			return;
		}

		enemy.ApplySlow(_slowdownPercent * coef, _slowdownLength * coef);

	}

	private void ApplyPoison(Enemy enemy, float coef = 1)
	{
		if (_poisonDamage <= 0 || _poisonTicks <= 0) return;

		enemy.ApplyPoison(_poisonDamage * coef, Mathf.Max(1, (int)(_poisonTicks * coef)));
	}

	private void ExplodeSplash(float incomingDmg)
	{

		var enemies = GameManager.Instance.WaveDirector.ActiveEnemies;
		for (int i = 0; i < enemies.Count; i++) //Note: not using foreach because it was changing collection during run. I think it's enemies dying/being created, and I'm hoping that we're not missing enemies in this loop. No clue why things are being created in the middle of the process call though!
		{
			Enemy e = enemies[i];

			if (!IsInstanceValid(e)) continue;

			float dist = GlobalPosition.DistanceTo(e.GlobalPosition);
			if (dist <= _splashRadius)
			{
				float coef = 1f;
				//linear falloff
				if (_splashCoef > 0)
					coef = Mathf.Clamp((1f - (dist / _splashRadius)) * _splashCoef, 0.01f, 1f);

				e.TakeDamage(incomingDmg * coef); //use incoming damage so we can splash crits properly

				ApplyPoison(e, coef);
				ApplySlow(e, coef);
				ApplyKnockback(e, coef);

			}
		}
	}

	private void ChainToNextTargets(Enemy firstHit)
	{
		var enemies = GameManager.Instance.WaveDirector.ActiveEnemies.Where(e => e != firstHit);
		List<Enemy> chainedEnemies = new List<Enemy>();

		Enemy lastChained = firstHit;
		chainedEnemies.Add(firstHit);

		//loop through enemies and grab chain targets
		for(int i = 0; i < _chainTargets; i++ )
		{
			//get nearest
			Enemy potentialTarget = GameManager.Instance.GetNearestEnemyToPoint(lastChained.GlobalPosition, chainedEnemies);

			//check against distance and valid enemies
			if(potentialTarget == null || lastChained.GlobalPosition.DistanceTo(potentialTarget.GlobalPosition) > _chainDistance)
			{
				break; //break, because if the nearest is too far, all are 
			}

			chainedEnemies.Add(potentialTarget);
			lastChained = potentialTarget;
		}



		//chain to targets
		foreach(Enemy enemy in chainedEnemies)
		{
			OnHit(enemy, false, false, true, false);
		}
	}

	private void DealDamage(float _damage)
	{
		if (_target == null || !IsInstanceValid(_target))
			return;

	   
		_target.TakeDamage(_damage);
	}

	public void FreeProjectile()
	{
		QueueFree();
	}

	
}
