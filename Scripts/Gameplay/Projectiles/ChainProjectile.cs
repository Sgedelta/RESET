using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class ChainProjectile : Projectile
{
    [Export] public int MaxChains = 2;         
    [Export] public float ChainDelay = 0.15f;   
    [Export] public float ChainRange = 200f;

    private int _remainingChains;

    public override void _Ready()
    {
        _remainingChains = MaxChains;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_target == null || !IsInstanceValid(_target)){
            QueueFree();
            return;
        }

        GlobalPosition += dir * Speed * (float)delta;

        if (GlobalPosition.DistanceTo(_target.GlobalPosition) < 32f)
        {
            _target.TakeDamage(_damage);
            HandleChain();
        }
    }

    private async void HandleChain()
    {
        if (_remainingChains <= 0)
        {
            QueueFree();
            return;
        }

        _remainingChains--;

        Enemy nextTarget = FindNextTarget();
        if (nextTarget == null)
        {
            QueueFree();
            return;
        }

        await ToSignal(GetTree().CreateTimer(ChainDelay), "timeout");

        _target = nextTarget;
        float timeToTarget = (_target.GlobalPosition - GlobalPosition).Length() / Speed;
        dir = (_target.Follower.GetFuturePosition(timeToTarget) - GlobalPosition).Normalized();
    }

    private Enemy FindNextTarget()
    {
        List<Enemy> enemies = GetTree()
            .GetNodesInGroup("enemies")
            .OfType<Enemy>()
            .Where(e => e != _target && IsInstanceValid(e))
            .ToList();

        if (enemies.Count == 0)
            return null;

        Enemy closest = null;
        float closestDist = float.MaxValue;

        foreach (Enemy e in enemies)
        {
            float dist = GlobalPosition.DistanceTo(e.GlobalPosition);
            if (dist < ChainRange && dist < closestDist)
            {
                closest = e;
                closestDist = dist;
            }
        }

        return closest;
    }
}
