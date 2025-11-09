using Godot;

[GlobalClass]
public partial class WallAbility : AbilityBase
{
	[Export] public float DurationSeconds = 3.0f;
	[Export] public PackedScene WallScene;

	public override void Execute(Vector2 worldPos)
	{
		WallObject wall;

		if (WallScene?.Instantiate() is WallObject sceneWall)
		{
			wall = sceneWall;
		}
		else
		{
			wall = new WallObject();
		}

		wall.GlobalPosition = worldPos;
		wall.DurationSeconds = DurationSeconds;

		GameManager.Instance.AddChild(wall);
	}
}
