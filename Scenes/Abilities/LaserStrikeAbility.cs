using Godot;
using System.Threading.Tasks;

[GlobalClass]
public partial class LaserStrikeAbility : AbilityBase
{
	[Export] public float Damage = 150f;
	[Export] public float Radius = 150f;
	[Export] public PackedScene EffectScene;

	public override void Execute(Vector2 worldPos)
	{
		int level = Mathf.Max(CurrentLevel, 1);
		float effectiveDamage = Damage * level;
		float effectiveRadius = Radius * (level * 0.3f);

		if (EffectScene?.Instantiate() is LaserSFX fx)
		{
			fx.GlobalPosition = worldPos;
			fx.Radius = effectiveRadius;

			GameManager.Instance.AddChild(fx);

			fx.Connect(LaserSFX.SignalName.ImpactStarted, Callable.From(() =>
			{
				DealDamage(worldPos, effectiveRadius, effectiveDamage);
			}));
		}
	}


	private void DealDamage(Vector2 worldPos, float effectiveRadius, float effectiveDamage)
	{
		var enemies = GameManager.Instance.WaveDirector.ActiveEnemies;
		for (int i = enemies.Count - 1; i >= 0; i--)
		{
			var e = enemies[i];
			if (!GodotObject.IsInstanceValid(e)) continue;

			if (e.GlobalPosition.DistanceTo(worldPos) <= effectiveRadius)
				e.TakeDamage(effectiveDamage);
		}
	}
}
