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

	Tween anim;

	public override void _Ready()
	{
		label = GetNode<Label>("Label");


		_baseSettings = label.LabelSettings as LabelSettings ?? new LabelSettings();

		if (_baseSettings.OutlineSize <= 0)
		{
			_baseSettings.OutlineSize = 2;
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
		anim = GetTree().CreateTween();
		Tween sideAnim = GetTree().CreateTween();

		int wobbleCount = (int)(animTime / wobbleSpeed);
		int initialDir = GD.Randf() >= .5f ? 1 : -1;

		// movement
		anim.SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Sine);

		anim.TweenProperty(this, "position:y", Position.Y + (-1 * moveSpeed * animTime), animTime);
		anim.Parallel().TweenProperty(this, "scale", Vector2.Zero, animTime);

		// side to side
		sideAnim.SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
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

			sideAnim.TweenProperty(this, "position:x", Position.X + (initialDir * wobbleAmnt * 2), animTime / wobbleCount * 2);
		}

		// deletion
		anim.TweenCallback(Callable.From(() => { QueueFree(); }));
	}

	public void SetDamage(float damage, Color color)
	{
		if (label != null)
		{
			label.Modulate = color;
			label.Text = damage.ToString("F2");

			// a float from 0-1 representing the position of damage between min and max
			float damageScale = (Mathf.Clamp(damage, minScaleDamage.X, maxScaleDamage.X) - minScaleDamage.X)
								/ (maxScaleDamage.X - minScaleDamage.X);

			// Duplicate our base settings so outline / font / etc. are preserved
			var settings = _baseSettings.Duplicate() as LabelSettings;
			settings.FontSize = (int)Mathf.Lerp(minScaleDamage.Y, maxScaleDamage.Y, damageScale);

			// Apply to the label
			label.LabelSettings = settings;

			// Animation time based on damage
			animTime = Mathf.Pow(damageScale, animFalloffExp) * maxAnimTime;

			label.QueueRedraw();
		}

		StartAnimation();
	}
}
