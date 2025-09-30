using Godot;
using System;

public partial class ShooterComponent : Node2D
{
	[Export] public PackedScene ProjectileScene; // Drag Projectile.tscn here
    [Export] public PackedScene HomingProjectileScene; // Drag HomingProjectile.tscn here
    [Export] public PackedScene ExplosiveProjectileScene; // Drag Explosive Projectile.tscn here
    [Export] public PackedScene PiercingProjectileScene; // Drag Piercing Projectile.tscn here
    [Export] public PackedScene PoisonProjectileScene; // Drag Poison Projectile.tscn here
	[Export] public PackedScene ChainProjectileScene; // Drag Chain Projectile.tscn here 

    private float _cooldown;

	//inherited stats - see TowerStats struct in Tower.cs
	private float i_fireRate = 1.5f;
	private float i_damage   = 5f;
	private float i_projectileSpeed;
	private float i_critChance;
	private float i_critMult;
	private float i_shotSpread;
	private float i_shotSpreadFalloff;

	private ProjectileType _projectileType;

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

	public void SetProjectileType(ProjectileType type)
	{
		_projectileType = type;
	}

	private PackedScene GetProjectileScene()
	{
		switch (_projectileType)
		{
			case ProjectileType.Regular:
				return ProjectileScene;
			case ProjectileType.Homing:
				return HomingProjectileScene;
            case ProjectileType.Explosive:
                return ExplosiveProjectileScene;
            case ProjectileType.Piercing:
                return PiercingProjectileScene;
            case ProjectileType.Poison:
                return PoisonProjectileScene;
            case ProjectileType.Chain:
                return ChainProjectileScene;

			default:
				return ProjectileScene;

        }
    }

	public override void _Process(double delta)
	{
		_cooldown -= (float)delta;
		if (_cooldown > 0f) return;

		var tower = GetParent<Tower>();
		var target = tower.Targeting.PickTarget(tower.GlobalPosition);
		if (target == null) return;

		var projectileScene = GetProjectileScene();
		if (projectileScene == null) return;

		var p = (Projectile)projectileScene.Instantiate();
		p.Init(tower.GlobalPosition, target, i_damage, i_projectileSpeed, i_critChance, i_critMult, i_shotSpread, i_shotSpreadFalloff);
		GetTree().CurrentScene.AddChild(p);

		_cooldown = 1f / Mathf.Max(0.05f, i_fireRate);
	}
}
