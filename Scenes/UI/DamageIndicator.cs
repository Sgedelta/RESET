using Godot;
using System;
using System.Data.SqlTypes;

public partial class DamageIndicator : Node2D
{

	[Export] public float moveSpeed = 30;
	[Export] public float animFalloffExp = 3;
	[Export] private float maxAnimTime = 1.5f;

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

        //movement
        anim.SetParallel(true).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Sine);

		anim.TweenProperty(this, "position", Position + new Vector2(0, -1 * moveSpeed * animTime), animTime);
		anim.TweenProperty(this, "scale", Vector2.Zero, animTime);
		//anim.TweenProperty(this)


        //deletion
        anim.SetParallel(false);
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

		GD.Print(animTime);
		

		StartAnimation();
	}
}
