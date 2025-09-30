using Godot;
using System;

public partial class ExplosiveProjectile : Projectile
{
    [Export] public float ExplosionRadius = 64f; // radius in pixels
    [Export] public bool HasFalloff = false;     // optional: damage decreases with distance

    public override void _PhysicsProcess(double delta)
    {
        if (_target == null || !IsInstanceValid(_target))
        {
            QueueFree();
            return;
        }

        // Move toward target like a regular projectile
        SetGlobalPosition(GlobalPosition + dir * Speed * (float)delta);

        // Check hit against target
        if (GlobalPosition.DistanceTo(_target.GlobalPosition) < 8f)
        {
            Explode();
            QueueFree();
        }
    }

    private void Explode()
    {
        //Implement later 

        //get a root of the nearest enemeies and put them in like a list or something 
        //Loop through the list and deal half damage or something 
    }
}
