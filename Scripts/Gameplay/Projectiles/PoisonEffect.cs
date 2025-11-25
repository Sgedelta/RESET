using Godot;
using System;

public partial class PoisonEffect : Node
{
    private Enemy _enemy;
    private float _damagePerTick;
    private float _duration;
    private float _tickInterval;
    private float _timeSinceTick = 0f;

    public PoisonEffect(Enemy enemy, float damagePerTick, float duration, float tickInterval)
    {
        _enemy = enemy;
        _damagePerTick = damagePerTick;
        _duration = duration;
        _tickInterval = tickInterval;
    }

    public override void _Process(double delta)
    {
        if (_enemy == null || !IsInstanceValid(_enemy))
        {
            QueueFree();
            return;
        }

        _duration -= (float)delta;
        _timeSinceTick += (float)delta;

        if (_timeSinceTick >= _tickInterval)
        {
            _enemy.TakeDamage(_damagePerTick, false, DamageType.Posion);
            _timeSinceTick = 0f;
        }

        if (_duration <= 0f)
        {
            QueueFree();
        }
    }
}
