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

	[Export] private RichTextLabel _statDisplay;

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
		i_ScrollContainer = GetChild<ScrollContainer>(1);

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
	GD.Print("Refreshing UIs");

	DisplaySlots(); // ensures the right number are visible

	for (int i = 0; i < _container.GetChildCount(); i++)
	{
		if (_container.GetChild(i) is AspectSlot slot)
			slot.RefreshVisual();           // <-- update token/label in each slot
	}

	_statDisplay.Text = "Tower Stats\n" + ActiveTower.StatDisplay();

	if (AspectBar.Instance != null)
		AspectBar.Instance.Refresh();
}


	public void DisplaySlots(int count)
	{
		// Guard
		if (count < 0) { GD.PushWarning("[UI_TowerPullout] DisplaySlots called with count < 0"); count = 0; }
		if (AspectSlotScn == null) { GD.PushError("[UI_TowerPullout] AspectSlotScn not set!"); return; }

		// 1) Count existing slots (ignore non-slot UI)
		int slotsFound = 0;
		for (int i = 0; i < _container.GetChildCount(); i++)
			if (_container.GetChild(i) is AspectSlot) slotsFound++;

		// 2) Create missing slots to reach 'count'
		for (int need = slotsFound; need < count; need++)
			_container.AddChild(AspectSlotScn.Instantiate<AspectSlot>());

		// 3) Assign indices to *slots only* and control visibility
		int logical = 0;
		for (int i = 0; i < _container.GetChildCount(); i++)
		{
			if (_container.GetChild(i) is AspectSlot slot)
			{
				slot.SetIndex(logical);
				slot.Visible = logical < count;
				logical++;
			}
		}

		// 4) Hide any extra slots beyond `count`
		int seen = 0;
		for (int i = 0; i < _container.GetChildCount(); i++)
		{
			if (_container.GetChild(i) is AspectSlot slot)
			{
				slot.Visible = seen < count;
				seen++;
			}
		}
	}


	public void DisplaySlots()
	{
		DisplaySlots(AvailableSlots);
	}



}
