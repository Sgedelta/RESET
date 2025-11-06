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
        }

    }

    private float size = 600;

    [Export] private float borderWidth = 5;
    [Export] private Color rangeColor = new Color("#5dbdce");
    [Export] private Color rangeInteriorColor = new Color("#5dbdce66");


    public void SetDisplay(bool val)
    {
        Show = val;
    }

    public void UpdateSize(float range)
    {
        size = range;
        QueueRedraw();
    }

    // Custom Draw command that overwrites hownthis is displayed
    public override void _Draw()
    {
        GD.Print(size);

        //draw the interior circle

        DrawCircle(Vector2.Zero, size, rangeInteriorColor, true);

        //draw the border circle

        DrawCircle(Vector2.Zero, size, rangeColor, false, borderWidth, true);
    }

}
