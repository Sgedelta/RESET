using Godot;
using System;
using System.Collections.Generic;

public partial class PiercingProjectile : Projectile
{
    [Export] public float maxDistance = 600f;
    [Export] public float PierceFalloff = 0.8f;

    private Vector2 _startpos;
    private HashSet<Enemy> _hitEnemies = new();

    public override void _PhysicsProcess(double delta)
    {
        GlobalPosition += dir * Speed * (float)delta;

        if(GlobalPosition.DistanceTo(_startpos) > maxDistance)
        {
            QueueFree();
            return;
        }

        foreach(var area in GetOverlappingAreas())
        {
            var enemy = area.GetParent() as Enemy;
            if(enemy != null && !_hitEnemies.Contains(enemy))
            {
                _hitEnemies.Add(enemy);
                enemy.TakeDamage(_damage);

                _damage *= PierceFalloff;

                if(_damage < 1f)
                {
                    QueueFree();
                    return;
                }
            }
        }
    }
}
