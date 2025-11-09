using Godot;
using System;
using System.Data.SqlTypes;
using System.Diagnostics;

public partial class DamageIndicator : Node2D
{

	[Export] public float moveSpeed = 30;
	[Export] public float animFalloffExp = 3;
	[Export] private float maxAnimTime = 1.5f;
	[Export] private float wobbleSpeed = .3f;
	[Export] private float wobbleAmnt = 15;

	// two vec2s representing a damage value in X and a font size associated with it in Y. interpolated between for scaling
	[Export] private Vector2 minScaleDamage;
	[Export] private Vector2 maxScaleDamage;

	[Export] private Vector2 offsetSize;

	private float animTime = 0;

	private Label label;

	Tween anim;

	public override void _Ready()
	{
		label = GetNode<Label>("Label");

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

		//movement
		anim.SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Sine);

		anim.TweenProperty(this, "position:y", Position.Y + (-1 * moveSpeed * animTime), animTime);
		anim.Parallel().TweenProperty(this, "scale", Vector2.Zero, animTime);

		//side to side
		sideAnim.SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
		sideAnim.TweenProperty(this, "position:x", Position.X + (initialDir * wobbleAmnt), animTime/wobbleCount);
		

		for(int i = 1; i < wobbleCount; i++)
		{
			if (i %2 == 1)
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

		//deletion
		anim.TweenCallback(Callable.From(() => { QueueFree(); }));
	}


	public void SetDamage(float damage, Color color)
	{
		if (label != null)
		{
			label.Modulate = color;
			label.Text = damage.ToString($"F2");

			//a float from 0-1 representing the position of damage between min and max, within min and max
			float damageScale = (Mathf.Clamp( damage, minScaleDamage.X, maxScaleDamage.X ) - minScaleDamage.X) / (maxScaleDamage.X - minScaleDamage.X);
			label.LabelSettings.FontSize = (int)Mathf.Lerp(minScaleDamage.Y, maxScaleDamage.Y, damageScale);

			animTime = Mathf.Pow(damageScale, animFalloffExp) * maxAnimTime;

			label.QueueRedraw();
		}


		StartAnimation();
	}
}
