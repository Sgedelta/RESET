using Godot;
using System;

public partial class PathVisualizer : Node
{
	[Export] public NodePath Path2DPath;
	[Export] public NodePath Line2DPath;
	[Export] public float Width = 3f;

	public override void _Ready()
	{
		var path = GetNode<Path2D>(Path2DPath);
		var line = GetNode<Line2D>(Line2DPath);

		var pts = path.Curve.GetBakedPoints();
		line.ClearPoints();
		foreach (var p in pts)
			line.AddPoint(p + path.Position);
		line.Width = Width;
	}
}
