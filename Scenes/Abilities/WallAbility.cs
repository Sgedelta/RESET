using Godot;

[GlobalClass]
public partial class WallAbility : AbilityBase
{
	[Export] public float DurationSeconds = 3.0f;  // Base duration at level 1
	[Export] public PackedScene WallScene;

	public override void Execute(Vector2 worldPos)
	{
		int level = Mathf.Max(CurrentLevel, 1);
		float effectiveDuration = DurationSeconds * level;

		WallObject wall;

		if (WallScene?.Instantiate() is WallObject sceneWall)
		{
			wall = sceneWall;
		}
		else
		{
			wall = new WallObject();
		}

		wall.GlobalPosition    = worldPos;
		wall.DurationSeconds   = effectiveDuration;

		GameManager.Instance.AddChild(wall);
	}
}
