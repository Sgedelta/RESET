using Godot;
using System;

public partial class UI_TowerPullout : CanvasLayer
{

    private bool _active;
    private bool animating = false;
    private Tower activeTower;

    private ScrollContainer i_ScrollContainer;

    [Export] float SlideTime = .5f;

    [Export] private Tower _tower;
    public Tower ActiveTower { get { return _tower; } }

    public override void _Ready()
    {
        i_ScrollContainer = GetChild<ScrollContainer>(0);
    }


    public void ToggleActive()
    {
        _active = !_active;
        TweenToActivePosition();
    }


    public void TweenToActivePosition()
    {
        if (animating) return; //currently already animating, so don't do anything

        animating = true; //otherwise, lock until tween is over
        Tween slideAnim = CreateTween();
        slideAnim.SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Cubic);
        slideAnim.TweenProperty(this, "offset", 
            new Vector2((_active ? i_ScrollContainer.Size.X : 0), 0), 
            SlideTime);
        slideAnim.TweenCallback(Callable.From(() => {animating = false;}));
    }

    public void SetToActivePosition()
    {

        Offset = new Vector2( (_active ? i_ScrollContainer.Size.X : 0), 0);
    }


}
