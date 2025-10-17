using Godot;

public partial class ShooterComponent : Node2D
{
	[Export] public PackedScene ProjectileScene;
	[Export] public PackedScene HomingProjectileScene;
	[Export] public PackedScene ExplosiveProjectileScene;
	[Export] public PackedScene PiercingProjectileScene;
	[Export] public PackedScene PoisonProjectileScene;
	[Export] public PackedScene ChainProjectileScene;

	private float _cooldown;

	private float i_fireRate = 1.5f;
	private float i_damage   = 5f;
	private float i_projectileSpeed;
	private float i_critChance;
	private float i_critMult;
	private float i_shotSpread;
	private float i_shotSpreadFalloff;

	private ProjectileType _projectileType = ProjectileType.Regular;

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
		return _projectileType switch
		{
			ProjectileType.Regular  => ProjectileScene,
			ProjectileType.Homing   => HomingProjectileScene,
			ProjectileType.Explosive=> ExplosiveProjectileScene,
			ProjectileType.Piercing => PiercingProjectileScene,
			ProjectileType.Poison   => PoisonProjectileScene,
			ProjectileType.Chain    => ChainProjectileScene,
			_ => ProjectileScene
		};
	}

	public override void _Process(double delta)
	{
		_cooldown -= (float)delta;
		if (_cooldown > 0f) return;

		var tower = GetParent<Tower>();
		var target = tower?.Targeting?.PickTarget(tower.GlobalPosition);
		if (target == null) return;

		var scene = GetProjectileScene();
		if (scene == null) return;

		GD.Print($"[Tower] Shooting projectile of type: {_projectileType}");


		var p = (Projectile)scene.Instantiate();
		p.Init(tower.GlobalPosition, target, i_damage, i_projectileSpeed, i_critChance, i_critMult, i_shotSpread, i_shotSpreadFalloff);
		GetTree().CurrentScene.AddChild(p);

		_cooldown = 1f / Mathf.Max(0.05f, i_fireRate);
	}
}
