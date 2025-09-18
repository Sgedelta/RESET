using Godot;
using System;

public partial class ShooterComponent : Node2D
{
	[Export] public PackedScene ProjectileScene; // Drag Projectile.tscn here
	private float _cooldown;
	private float _fireRate = 1.5f;
	private float _damage   = 5f;

	public void SetStats(float fireRate, float dmg)
	{
		_fireRate = fireRate;
		_damage = dmg;
	}

	public override void _Process(double delta)
	{
		_cooldown -= (float)delta;
		if (_cooldown > 0f) return;

		var tower = GetParent<Tower>();
		var target = tower.Targeting.PickTarget(tower.GlobalPosition);
		if (target == null) return;

		if (ProjectileScene == null) return;

		var p = (Projectile)ProjectileScene.Instantiate();
		p.Init(tower.GlobalPosition, target, _damage);
		GetTree().CurrentScene.AddChild(p);

		_cooldown = 1f / Mathf.Max(0.05f, _fireRate);
	}
}
