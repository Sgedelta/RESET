using Godot;
using System;

public partial class RangeDisplay : Node2D
{

	private bool show;
	public bool Show
	{
		get { return show; }

		set
		{
			show = value;
			Visible = value;
			QueueRedraw();
		}

	}

	private float size = 600;

	[Export] private float borderWidth = 5;
	[Export] private Color rangeColor = new Color("#5dbdce91");
	[Export] private Color rangeInteriorColor = new Color("#5dbdce2c");
	[Export] private float rangeChangeRate = 1250;

	private bool _animating = false;
	private Tween _animation;

	public override void _Process(double delta)
	{
		if (_animating)
		{
			QueueRedraw();
		}
	}

	public void SetDisplay(bool val)
	{
		Show = val;
	}

	public void UpdateSize(float range)
	{
		float currSize = size;
		//check if animating and stop the old tween if needed
		if(_animating && _animation != null)
		{
			_animation.Kill();
		}

		_animating = true;
		float delta = Mathf.Abs(range - currSize);

		_animation = GetTree().CreateTween();
		_animation.TweenProperty(this, nameof(size), range, delta / rangeChangeRate);
		_animation.TweenCallback(Callable.From(() => { _animating = false; }));

	}

	public void SetSize(float range)
	{
		size = range;
		QueueRedraw();
	}

	// Custom Draw command that overwrites hownthis is displayed
	public override void _Draw()
	{
		//draw the interior circle

		DrawCircle(Vector2.Zero, size, rangeInteriorColor, true);

		//draw the border circle

		DrawCircle(Vector2.Zero, size, rangeColor, false, borderWidth, true);
	}

}
