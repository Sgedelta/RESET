using Godot;

public partial class AspectBar : Control
{
	[Export] public PackedScene TokenScene;

	[Export] public NodePath RowPath = "Panel/Margin/Scroll/Row";

	public override void _Ready()
	{
		var row = GetNodeOrNull<HBoxContainer>(RowPath);

		foreach (var aspect in AspectLibrary.AllAspects)
		{
			var token = TokenScene.Instantiate<AspectToken>();
			token.Init(aspect);
			row.AddChild(token);
		}
	}
}
