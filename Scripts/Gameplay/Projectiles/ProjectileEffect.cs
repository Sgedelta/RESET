using Godot;
using System;

public partial class ProjectileEffect : Node
{

    public Projectile Projectile { get; private set; }

    public virtual void Initialize(Projectile projectile)
    {
        Projectile = projectile;
    }

    public virtual void OnUpdate(double delta) { }
    public virtual void OnHit(Enemy enemy) { }
    public virtual void OnExpire() { }


}
