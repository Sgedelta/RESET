using Godot;
using System;
using System.Data.SqlTypes;

public partial class DamageIndicator : Node2D
{

    [Export] public int speed = 30;
    [Export] public int friction = 30;
    private Vector2 shiftDirection = Vector2.Zero;
    private Label label;

    public override void _Ready()
    {
        label = GetNode<Label>("Label");

       Position += new Vector2(
            (float)GD.RandRange(-42, 42),
           (float)GD.RandRange(-62, 62)
        );

        shiftDirection = new Vector2(
          (float)GD.RandRange(-1, 1),
        (float)GD.RandRange(-2,5)
        ).Normalized();

        ZIndex = 999;

    }
    public override void _Process(double delta)
    {
        Position += speed * shiftDirection * (float)delta;
        speed = (int)Mathf.Max(speed - friction * delta, 0);
    }


    public void SetDamage(float damage, Color color)
    {
        label.Modulate = color;
        if (label != null)
        {
            label.Text = damage.ToString();
            label.QueueRedraw();
        }
    }
}


