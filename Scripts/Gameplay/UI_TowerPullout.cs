using Godot;
using System;

public partial class UI_TowerPullout : CanvasLayer
{

    private bool _active;
    private bool animating = false;

    private ScrollContainer i_ScrollContainer;

    [Export] float SlideTime = .5f;

    [Export] private Tower _tower;
    public Tower ActiveTower { get { return _tower; } }

    [Export] private AspectUIContainer _container;
    public AspectUIContainer Container { get { return _container; } }

    public int AvailableSlots
    {
        get
        {
            if(ActiveTower != null)
            {
                return ActiveTower.ModifiedStats.AspectSlots; ;
            }
            GD.PushError("No Active Tower to Get Slots From!");
            return -1;
        }
    }

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


    public void RefreshUIs()
    {
        // update UIs 
            // _container's children
        foreach(Node child in _container.GetChildren())
        {
            if (child is not AspectSlot slot) continue;
            var aspect = ActiveTower.GetAspectInSlot(slot.Index);
            slot.Label.Text = aspect != null ? aspect.Template.DisplayName : "+";
        }

            // text based on stats
        //loop through base stats and modified stats - display net changes?
            // use specific values? use generic +++/---?
            // whatever we do here, has to be similar-ish to what we do for aspect hover ui

    }

    public void DisplaySlots(int count)
    {
        for (int i = 0; i < count; i++)
        {
            //loop through _container's AspectSlot children - if they don't exist, create more
            //activate each one (is visible)
        }

        for(int i = count; i < _container.GetChildCount(); i++)
        {
            //set others to not visible
        }
    }

    public void DisplaySlots()
    {
        DisplaySlots(AvailableSlots);
    }



}
