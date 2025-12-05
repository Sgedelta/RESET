using Godot;

[GlobalClass]
public partial class FireAbility : AbilityBase
{
	[Export] public float Radius = 96f;             // Base radius at level 1
	[Export] public float DurationSeconds = 4.0f;
	[Export] public float TickInterval = 0.25f;
	[Export] public float DamagePerTick = 8f;       // Base damage per tick at level 1

	[Export] public PackedScene EffectScene;

	public override async void Execute(Vector2 worldPos)
	{
		// Level scaling
		int level = Mathf.Max(CurrentLevel, 1);
		float effectiveRadius      = Radius * (level* 0.5f);
		float effectiveDamagePerTick = DamagePerTick * level;
		float effectiveDuration    = DurationSeconds; // duration not scaled, per your spec

		FireSFX sfx = null;
		if (EffectScene?.Instantiate() is FireSFX fx)
		{
			sfx = fx;
			sfx.Radius          = effectiveRadius;
			sfx.DurationSeconds = effectiveDuration;
			sfx.GlobalPosition  = worldPos;
			GameManager.Instance.AddChild(sfx);
		}

		var tree = GameManager.Instance.GetTree();
		float elapsed = 0f;

		while (elapsed < effectiveDuration)
		{
			var enemies = GameManager.Instance.WaveDirector.ActiveEnemies;
			for (int i = enemies.Count - 1; i >= 0; i--)
			{
				var e = enemies[i];
				if (!GodotObject.IsInstanceValid(e)) continue;

				if (e.GlobalPosition.DistanceTo(worldPos) <= effectiveRadius)
					e.TakeDamage(effectiveDamagePerTick);
			}

			await ToSignal(tree.CreateTimer(TickInterval), "timeout");
			elapsed += TickInterval;
		}
	}
}
