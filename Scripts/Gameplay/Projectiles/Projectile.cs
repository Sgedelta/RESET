using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using static System.Net.Mime.MediaTypeNames;

public partial class Projectile : Area2D
{
	[Export] public float Speed = 800f; //speed, in pixels
	protected Enemy _target;
	protected float _damage;

	protected Vector2 dir;

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

    public void Init(Vector2 from, Enemy target, float damage, float speed, float critChance, float critMult, float shotSpread, float spreadFalloff, TowerStats stats)
    {
        GlobalPosition = from;
        _target = target;
        _damage = damage;
        Speed = speed;

        _chainTargets = stats.ChainTargets;
        _chainDistance = stats.ChainDistance;
        _splashRadius = stats.SplashRadius;
        _splashCoef = stats.SplashDamage;
        _poisonDamage = stats.PoisonDamage;
        _poisonTicks = stats.PoisonTicks;
        _piercingAmount = stats.PiercingAmount;
        _knockbackAmount = stats.KnockbackAmount;
        _slowdownLength = stats.SlowdownLength;
        _homingStrength = stats.HomingStrength;
        
        if (_target != null && IsInstanceValid(_target))
        {
            dir = (_target.GlobalPosition - GlobalPosition).Normalized();
        }
        else
        {
            dir = Vector2.Right;
        }

        _homingTurnSpeed = Mathf.Clamp(_homingStrength / 50f, 0f, 1f);
    }


    public override void _PhysicsProcess(double delta)
    {
        if (_target == null || !IsInstanceValid(_target))
        {
            QueueFree();
            return;
        }
        //Homing Logic 
        if(_homingStrength > 0f && _target != null && IsInstanceValid(_target))
        {
            Vector2 desiredDir = (_target.GlobalPosition - GlobalPosition).Normalized();
            dir = dir.Lerp(desiredDir, _homingTurnSpeed * (float)delta * 10f).Normalized();
        }

        GlobalPosition += dir * Speed * (float)delta;

        float hitRadius = 32f; 
        if (GlobalPosition.DistanceTo(_target.GlobalPosition) < hitRadius)
        {
            OnHit(_target);
            //QueueFree();
        }
    }

    private void OnHit(Enemy enemy)
    {
        if(enemy == null || !IsInstanceValid(enemy))
        {
            return;
        }

        if (_hitEnemies.Contains(enemy))
            return;

        _hitEnemies.Add(enemy);

        enemy.TakeDamage(_damage);

        ApplyKnockback(enemy);
        ApplySlow(enemy);
        ApplyPoison(enemy);

        if(_splashRadius > 0 && !_hasExploded)
        {
            ExplodeSplash();
            _hasExploded = true;
        }

        if(_chainTargets > 0)
        {
            ChainToNextTargets(enemy);
        }

        _piercingAmount--;
        if(_piercingAmount <= 0)
        {
            QueueFree();
        }
    }

    private void ApplyKnockback(Enemy enemy)
    {
        if (_knockbackAmount <= 0) return;

        Vector2 knockDir = (enemy.GlobalPosition - GlobalPosition).Normalized();
        enemy.ApplyKnockback(knockDir, _knockbackAmount);
    }

    private void ApplySlow(Enemy enemy)
    {
        if (_slowdownPercent <= 0 || _slowdownLength <= 0) return;

        enemy.ApplySlow(_slowdownPercent, _slowdownLength);

    }

    private void ApplyPoison(Enemy enemy)
    {
        if (_poisonDamage <= 0 || _poisonTicks <= 0) return;

        enemy.ApplyPoison(_poisonDamage, _poisonTicks);
    }

    private void ExplodeSplash()
    {
        var enemies = GetTree().GetNodesInGroup("enemies").OfType<Enemy>();
        foreach (var e in enemies)
        {
            if (!IsInstanceValid(e)) continue;

            float dist = GlobalPosition.DistanceTo(e.GlobalPosition);
            if (dist <= _splashRadius)
            {
                float coef = 1f;
                if (_splashCoef > 0)
                    coef = Mathf.Clamp(1f - (dist / _splashRadius) * _splashCoef, 0.2f, 1f);

                e.TakeDamage(_damage * coef);
            }
        }
    }

    private void ChainToNextTargets(Enemy firstHit)
    {
        var enemies = GetTree().GetNodesInGroup("enemies").OfType<Enemy>().Where(e => e != firstHit);
        Enemy closest = null;
        float closestDist = _chainDistance;

        foreach (var e in enemies)
        {
            if (!IsInstanceValid(e)) continue;
            float dist = firstHit.GlobalPosition.DistanceTo(e.GlobalPosition);
            if (dist < closestDist)
            {
                closest = e;
                closestDist = dist;
            }
        }

        if (closest != null)
        {
            var newProj = (Projectile)Duplicate();
            newProj._target = closest;
            newProj._chainTargets = _chainTargets - 1;
            GetParent().AddChild(newProj);
        }
    }

    private void DealDamage(float _damage)
    {
        if (_target == null || !IsInstanceValid(_target))
            return;

        _target.TakeDamage(_damage);

       
        _target.TakeDamage(_damage);
    }

    
}
