using Godot;
using System;
using System.Collections;
using System.Threading.Tasks;

public partial class UI_TowerPullout : CanvasLayer
{

    private bool _active = false;
    private bool animating = false;
    public bool Animating { get { return animating;  } 
        set {  
            animating = value;
            EmitSignal(SignalName.AnimationStateChanged, value);
                } 
    }

    private ScrollContainer i_ScrollContainer;

    [Export] float SlideTime = .5f;

    [Export] private Tower _tower;
    public Tower ActiveTower { get { return _tower; } 
        set
        {
            if(value != _tower)
            {
                ChangeActiveTower(value);
            }
        }
    
    }

    [Export] private AspectUIContainer _container;
    public AspectUIContainer Container { get { return _container; } }

    [Signal] public delegate void AnimationStateChangedEventHandler(bool AnimState);

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

    [Export] private PackedScene AspectSlotScn;

    public override void _Ready()
    {
        i_ScrollContainer = GetChild<ScrollContainer>(0);

        AddToGroup("tower_pullout");

        _active = false;
        SetToActivePosition(); //do not tween

        base._Ready();
    }


    public void ToggleActive()
    {
        _active = !_active;
        TweenToActivePosition();
    }

    public void SetActiveState(bool state)
    {
        if(_active != state)
        {
            ToggleActive();
        }
    }


    public void TweenToActivePosition()
    {
        if (animating) return; //currently already animating, so don't do anything

        Animating = true; //otherwise, lock until tween is over
        Tween slideAnim = CreateTween();
        slideAnim.SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Cubic);
        slideAnim.TweenProperty(this, "offset", 
            new Vector2((_active ? 0 : i_ScrollContainer.Size.X), 0), 
            SlideTime);
        slideAnim.TweenCallback(Callable.From(() => {Animating = false;}));
    }

    public void SetToActivePosition()
    {

        Offset = new Vector2( (_active ? 0 : i_ScrollContainer.Size.X), 0);
    }

    private async void ChangeActiveTower(Tower tower)
    {
        if(_active)
        {
            ToggleActive();
            await ToSignal(this, SignalName.AnimationStateChanged);
        }
        _tower = tower;
        RefreshUIs();
        ToggleActive();
    }


    public void RefreshUIs()
    {
        // update UIs 
            // _container's children
        DisplaySlots();
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


        // get and update the Bar
        GetNodeOrNull<AspectBar>("/root/Run/CanvasLayer/AspectBar")?.Refresh();

    }

    public void DisplaySlots(int count)
    {
        // activate children that exist
        for (int i = 0; i < _container.GetChildCount(); i++)
        {
            //loop through _container's AspectSlot children - if they don't exist, create more
            //activate each one (is visible)
            _container.GetChild<AspectSlot>(i).Visible = true;
        }

        // create new ones that we still need
        for (int i = _container.GetChildCount(); i < count; i++)
        {
            var newSlot = AspectSlotScn.Instantiate();
            _container.AddChild(newSlot);
        }

        // hide ones we don't
        for(int i = count; i < _container.GetChildCount(); i++)
        {
            //set others to not visible
            _container.GetChild<AspectSlot>(i).Visible = false;
        }
    }

    public void DisplaySlots()
    {
        DisplaySlots(AvailableSlots);
    }



}
