using Godot;
using System.Runtime.InteropServices;
using System;

public partial class ShooterComponent : Node2D
{
	[Export] public PackedScene ProjectileScene;
	[Export] public PackedScene HomingProjectileScene;
	[Export] public PackedScene ExplosiveProjectileScene;
	[Export] public PackedScene PiercingProjectileScene;
	[Export] public PackedScene PoisonProjectileScene;
	[Export] public PackedScene ChainProjectileScene;
	/// <summary>
	/// used in the falloff calculation. what we multiply the 
	/// max spread by to get the "normal" standard deviation. 
	/// Lower values result in tighter/more accurate falloff, 
	/// higher values result in more looser/more spread out
	/// /linear falloff
	/// </summary>
	private const float FALLOFF_STDDEV_RATIO = .5f;

	private Tower tower;
	private RandomNumberGenerator rng;

	private float _cooldown;

	private float i_fireRate = 1.5f;
	private float i_damage   = 5f;
	private float i_projectileSpeed;
	private float i_critChance;
	private float i_critMult;
	private float i_shotSpread;
	private float i_shotSpreadFalloff;

	private ProjectileType _projectileType = ProjectileType.Regular;


	public override void _Ready()
	{
		rng = new RandomNumberGenerator();
	}




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

		
		//TODO: Replace with Fire() once Init is fixed

		var tower = GetParent<Tower>();
		var target = tower?.Targeting?.PickTarget(tower.GlobalPosition);
		if (target == null) return;

		var scene = GetProjectileScene();
		if (scene == null) return;

		var p = (Projectile)scene.Instantiate();
		p.Init(tower.GlobalPosition, target, i_damage, i_projectileSpeed, i_critChance, i_critMult, i_shotSpread, i_shotSpreadFalloff);
		GetTree().CurrentScene.AddChild(p);

		_cooldown = 1f / Mathf.Max(0.05f, i_fireRate);
		//END REPLACE
		// Fire();
	}

	public void Fire()
	{
        //reassign tower if it's not assigned
        if (tower == null)
        {
            tower = GetParent<Tower>();
            //if its still not assigned we couldn't find it.
            if (tower == null)
            {
                GD.PushError("Shooter Component attempted to fire without finding Tower!");
                return;
            }
        }

		//get target
		var target = tower.Targeting?.PickTarget(tower.GlobalPosition);
		if(target == null)
		{
			return;
		}

		//FireAt target
		FireAt(target);

		//update cooldown
        _cooldown = 1f / Mathf.Max(0.05f, i_fireRate);
    }

	public void FireAt(Node2D target)
	{

		//TODO: update/replace once merged with projectile branch
		var projScene = GetProjectileScene();
		if(projScene == null)
		{
			GD.PushError("Projectile Scene Not Found!");
			return;
		}
		 
		Projectile projectile = (Projectile)projScene.Instantiate();
        
        projectile.GlobalPosition = tower.GlobalPosition;

        //calc & update initial direction based on falloff
			// all random is gaussian, but with the falloff close to 0 it approximates linear
        Vector2 initialDirection = (target.GlobalPosition - projectile.GlobalPosition)
			.Normalized()
			.Rotated(Mathf.DegToRad(
				rng.Randfn(0, ((i_shotSpread * FALLOFF_STDDEV_RATIO) / i_shotSpreadFalloff) % i_shotSpread)
            ));
		
		//TODO: Fix Init - this should point the projectile in a specific direction and set its relevant stats. 
		
		//projectile.Init();

		
		GetTree().CurrentScene.AddChild(projectile);

		
		
	}
}
