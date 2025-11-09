using Godot;

[GlobalClass]
public partial class FireAbility : AbilityBase
{
	[Export] public float Radius = 96f;
	[Export] public float DurationSeconds = 4.0f;
	[Export] public float TickInterval = 0.25f;
	[Export] public float DamagePerTick = 8f;

	[Export] public PackedScene EffectScene;

	public override async void Execute(Vector2 worldPos)
	{
		FireSFX sfx = null;
		if (EffectScene?.Instantiate() is FireSFX fx)
		{
			sfx = fx;
			sfx.Radius = Radius;
			sfx.DurationSeconds = DurationSeconds;
			sfx.GlobalPosition = worldPos;
			GameManager.Instance.AddChild(sfx);
		}

		var tree = GameManager.Instance.GetTree();
		float elapsed = 0f;

		while (elapsed < DurationSeconds)
		{
			var enemies = GameManager.Instance.WaveDirector.ActiveEnemies;
			for (int i = enemies.Count - 1; i >= 0; i--)
			{
				var e = enemies[i];
				if (!GodotObject.IsInstanceValid(e)) continue;

				if (e.GlobalPosition.DistanceTo(worldPos) <= Radius)
					e.TakeDamage(DamagePerTick);
			}

			await ToSignal(tree.CreateTimer(TickInterval), "timeout");
			elapsed += TickInterval;

		}
	}
}
