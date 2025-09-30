using Godot;
using System;

public partial class PoisonProjectile : Projectile
{
    [Export] public float PoisonDamagePerTick = 1f;
    [Export] public float PoisonDuration = 5f;
    [Export] public float PoisonTickInterval = 1f;

    public override void _PhysicsProcess(double delta)
    {
        if (_target == null || !IsInstanceValid(_target))
        {
            QueueFree();
            return;
        }

        SetGlobalPosition(GlobalPosition + dir * Speed * (float)delta);

        if (GlobalPosition.DistanceTo(_target.GlobalPosition) < 8f)
        {
            _target.TakeDamage(_damage);

            _target.ApplyDamageOverTime(PoisonDamagePerTick, PoisonDuration, PoisonTickInterval);

            QueueFree();
        }
    }
}
