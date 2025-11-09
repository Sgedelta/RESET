using Godot;

public partial class AbilityMenu : Control
{
	[Export] public PackedScene AbilityTokenScene;
	[Export] public AbilityBase DefaultAbility;
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

		for (int i = 0; i < 3; i++)
		{
			var tok = AbilityTokenScene.Instantiate<AbilityToken>();
			tok.Init(DefaultAbility);
			_row.AddChild(tok);
		}
	}
}
