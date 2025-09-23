using Godot;
using System;

public partial class ShooterComponent : Node2D
{
	[Export] public PackedScene ProjectileScene; // Drag Projectile.tscn here
	private float _cooldown;

	//inherited stats - see TowerStats struct in Tower.cs
	private float i_fireRate = 1.5f;
	private float i_damage   = 5f;
	private float i_projectileSpeed;
	private float i_critChance;
	private float i_critMult;
	private float i_shotSpread;
	private float i_shotSpreadFalloff;

	public void SetStats(TowerStats stats)
	{
		i_fireRate = stats.FireRate;
		i_damage = stats.Damage;
		i_projectileSpeed = stats.ProjectileSpeed;
		i_critChance = stats.CritChance;
		i_critMult = stats.CritMult;	
		i_shotSpread = stats.ShotSpread;
		i_shotSpreadFalloff = stats.ShotSpreadFalloff;

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
		p.Init(tower.GlobalPosition, target, i_damage, i_projectileSpeed, i_critChance, i_critMult, i_shotSpread, i_shotSpreadFalloff);
		GetTree().CurrentScene.AddChild(p);

		_cooldown = 1f / Mathf.Max(0.05f, i_fireRate);
	}
}
