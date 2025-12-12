using Godot;
using System;

public partial class DamageIndicator : Node2D
{
	[Export] public float moveSpeed = 30;
	[Export] public float animFalloffExp = .5f;
	[Export] private float maxAnimTime = 5;
	[Export] private float wobbleSpeed = .3f;
	[Export] private float wobbleAmnt = 15;

	// two vec2s representing a damage value in X and a font size associated with it in Y. interpolated between for scaling
	[Export] private Vector2 minScaleDamage = new Vector2(1, 30);
	[Export] private Vector2 maxScaleDamage = new Vector2(250, 200);

	[Export] private Vector2 offsetSize = new Vector2(33, 25);

	private float animTime = 0;

	private Label label;
	private LabelSettings _baseSettings;

	private Tween anim;

	public override void _Ready()
	{
		label = GetNode<Label>("Label");

		_baseSettings = label.LabelSettings as LabelSettings ?? new LabelSettings();

		if (_baseSettings.OutlineSize <= 0)
		{
			_baseSettings.OutlineSize = 4;
			_baseSettings.OutlineColor = Colors.Black;
		}

		label.LabelSettings = _baseSettings;

		Position += new Vector2(
			(float)GD.RandRange(-offsetSize.X, offsetSize.X),
			(float)GD.RandRange(-offsetSize.Y, offsetSize.Y)
		);

		ZIndex = 999;
	}

	public void StartAnimation()
	{
		// If we're not in the scene tree anymore, don't try to animate
		if (!IsInsideTree() || !IsInstanceValid(this))
			return;

		// If somehow animTime is tiny or zero, just free immediately
		if (animTime <= 0.01f)
		{
			QueueFree();
			return;
		}

		// Kill any previous tween on this node
		anim?.Kill();

		anim = CreateTween();             // Node-bound tween
		var sideAnim = CreateTween();     // Also bound to this node

		int wobbleCount = Mathf.Max(1, (int)(animTime / wobbleSpeed));
		int initialDir = GD.Randf() >= .5f ? 1 : -1;

		// movement
		anim.SetEase(Tween.EaseType.Out)
			.SetTrans(Tween.TransitionType.Sine);

		anim.TweenProperty(this, "position:y", Position.Y + (-1 * moveSpeed * animTime), animTime);
		anim.Parallel().TweenProperty(this, "scale", Vector2.Zero, animTime);

		// side to side
		sideAnim.SetTrans(Tween.TransitionType.Sine)
				.SetEase(Tween.EaseType.InOut);

		sideAnim.TweenProperty(this, "position:x", Position.X + (initialDir * wobbleAmnt), animTime / wobbleCount);

		for (int i = 1; i < wobbleCount; i++)
		{
			if (i % 2 == 1)
			{
				initialDir = initialDir == 1 ? -1 : 1;
				sideAnim.SetEase(Tween.EaseType.Out);
			}
			else
			{
				sideAnim.SetEase(Tween.EaseType.In);
			}

			sideAnim.TweenProperty(
				this,
				"position:x",
				Position.X + (initialDir * wobbleAmnt * 2),
				(animTime / wobbleCount) * 2
			);
		}

		// deletion
		anim.TweenCallback(Callable.From(() =>
		{
			if (IsInsideTree())
				QueueFree();
		}));
	}

	public void SetDamage(float damage, Color color, bool isCrit = false)
	{
		if (label != null)
		{
			// Crit overrides color to blue
			if (isCrit)
				label.Modulate = Colors.SkyBlue;
			else
				label.Modulate = color;

			// No decimals
			label.Text = Mathf.RoundToInt(damage).ToString();

			// 0-1 scale for damage between min and max
			float damageScale = (Mathf.Clamp(damage, minScaleDamage.X, maxScaleDamage.X) - minScaleDamage.X)
								/ (maxScaleDamage.X - minScaleDamage.X);

			var settings = _baseSettings.Duplicate() as LabelSettings;
			settings.FontSize = (int)Mathf.Lerp(minScaleDamage.Y, maxScaleDamage.Y, damageScale);

			label.LabelSettings = settings;

			// Animation time based on damage
			animTime = Mathf.Pow(damageScale, animFalloffExp) * maxAnimTime;

			label.QueueRedraw();
		}

		StartAnimation(); // only once
	}
}
