using Godot;

public partial class AbilityMenu : Control
{
	[Export] public PackedScene TokenScene;
	[Export] public NodePath RowPath;

	private Control _row;

	public override void _Ready()
	{
		_row = GetNode<Control>(RowPath);
		Refresh();
	}

	public void Refresh()
	{
		foreach (var c in _row.GetChildren()) c.QueueFree();

		foreach (var ability in AbilityManager.Instance.AllAbilities)
		{
			if (ability == null) continue;
			var token = TokenScene.Instantiate<AbilityToken>();
			_row.AddChild(token);
			token.Init(ability);
		}
	}
}
