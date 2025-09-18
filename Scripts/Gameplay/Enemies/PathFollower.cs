using Godot;
using System;

/// <summary>
/// Path Follower is a node that makes it's parent move along a path node at a given speed in pixels. It does NOT need to be a PathFollow2D Node. 
/// </summary>
public partial class PathFollower : Node2D
{
	[Export] public float Speed = 80f;

	private Path2D _path;
	private Curve2D _curve;
	private float _distance; //distance along the curve in pixels


	public void SetPath(Path2D path)
	{
		_path = path;
		_curve = _path?.Curve;
		_distance = 0f;
	}

	public override void _Process(double delta)
	{
		if (_curve == null || _curve.PointCount < 2) return;

		_distance += Speed * (float)delta;
		float total = _curve.GetBakedLength();

		if (_distance >= total)
		{
			GetParent<Node2D>().QueueFree();
			return;
		}

		Vector2 local = _curve.SampleBaked(_distance);
		var enemy = (Node2D)GetParent();
		enemy.GlobalPosition = _path.GlobalPosition + local;
	}

	/// <summary>
	/// Method to read ahead the follow's position by timeAhead milliseconds.
	/// </summary>
	/// <param name="timeAhead">milliseconds to read ahead</param>
	/// <returns></returns>
    public Vector2 GetFuturePosition(float timeAhead)
    {
		Vector2 pos = GetParent<Node2D>().GlobalPosition;

		

		pos += _curve.SampleBaked(_distance + Speed * timeAhead);

		return pos;
    }
}
