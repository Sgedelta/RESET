using Godot;
using System;

public partial class HomingProjectile : Projectile
{
    [Export] public float turnRate = 5f;

    public override void _PhysicsProcess(double delta)
    {
        if (_target == null || !IsInstanceValid(_target))
        {
            QueueFree();
            return;
        }

        Vector2 desiredDir = (_target.GlobalPosition - GlobalPosition).Normalized();

        dir = dir.Lerp(desiredDir, (float) delta * turnRate).Normalized();

        SetGlobalPosition(GlobalPosition + dir * Speed * (float)delta);

        // Cheap hit test – good enough to prove the loop
        if (GlobalPosition.DistanceTo(_target.GlobalPosition) < 8f)
        {
            _target.TakeDamage(_damage);
            QueueFree();
        }
    }
}
