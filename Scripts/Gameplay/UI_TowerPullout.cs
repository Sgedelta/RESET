using Godot;
using System.Threading.Tasks;

public partial class UI_TowerPullout : CanvasLayer
{
	private bool _active = false;
	private bool animating = false;
	public bool Animating
	{
		get => animating;
		set { animating = value; EmitSignal(SignalName.AnimationStateChanged, value); }
	}

	private ScrollContainer i_ScrollContainer;

	[Export] float SlideTime = .5f;

	[Export] private Tower _tower;
	public Tower ActiveTower
	{
		get => _tower;
		set { if (value != _tower) ChangeActiveTower(value); }
	}

	[Export] private AspectUIContainer _container;
	public AspectUIContainer Container => _container;

	[Signal] public delegate void AnimationStateChangedEventHandler(bool AnimState);

	[Export] private RichTextLabel _statDisplay;

	public int AvailableSlots
	{
		get
		{
			if (ActiveTower != null)
				return ActiveTower.ModifiedStats.AspectSlots;
			GD.PushError("No Active Tower to Get Slots From!");
			return -1;
		}
	}

	[Export] private PackedScene AspectSlotScn;

	public override void _Ready()
	{
		i_ScrollContainer = GetChild<ScrollContainer>(1);
		i_ScrollContainer.MouseFilter = Control.MouseFilterEnum.Pass;

		AddToGroup("tower_pullout");

		_active = false;
		SetToActivePosition(); // don't tween

		base._Ready();
	}

	public void ToggleActive()
	{
		_active = !_active;
		TweenToActivePosition();
	}

	public void SetActiveState(bool state)
	{
		if (_active != state) ToggleActive();
	}

	public void TweenToActivePosition()
	{
		if (animating) return;

		Animating = true;
		var slideAnim = CreateTween();
		slideAnim.SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Cubic);
		slideAnim.TweenProperty(this, "offset",
			new Vector2((_active ? 0 : i_ScrollContainer.Size.X), 0),
			SlideTime);
		slideAnim.TweenCallback(Callable.From(() => { Animating = false; }));
	}

	public void SetToActivePosition()
	{
		Offset = new Vector2((_active ? 0 : i_ScrollContainer.Size.X), 0);
	}

	private async void ChangeActiveTower(Tower tower)
	{
		if (_active)
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
			if (_container.GetChild(i) is AspectSlot slot)
				slot.RefreshVisual();

	_statDisplay.Text = ActiveTower.StatDisplayBBCode();

		if (AspectBar.Instance != null)
			AspectBar.Instance.Refresh();
	}

	private void ConfigureSlotInput(AspectSlot slot)
	{
		if (slot == null) return;

		// Slot is the drop target
		slot.MouseFilter = Control.MouseFilterEnum.Stop;

		// Children should not intercept mouse
		foreach (var ch in slot.GetChildren())
			if (ch is Control cc)
				cc.MouseFilter = Control.MouseFilterEnum.Ignore;

		// NOTE: We no longer connect 'child_entered_tree' here.
		// We set MouseFilter on the token itself when we create it in AspectSlot.RefreshVisual().
	}

	public void DisplaySlots(int count)
{
	if (count < 0) { GD.PushWarning("[UI_TowerPullout] DisplaySlots called with count < 0"); count = 0; }
	if (AspectSlotScn == null) { GD.PushError("[UI_TowerPullout] AspectSlotScn not set!"); return; }

	// Count existing slots
	int slotsFound = 0;
	for (int i = 0; i < _container.GetChildCount(); i++)
		if (_container.GetChild(i) is AspectSlot) slotsFound++;

	// Create missing
	for (int need = slotsFound; need < count; need++)
	{
		var slot = AspectSlotScn.Instantiate<AspectSlot>();
		// ensure usable hitbox before it’s added to the flow layout
		slot.CustomMinimumSize = new Vector2(96, 96);
		ConfigureSlotInput(slot);
		_container.AddChild(slot);
	}

	// Assign indices, visibility, and configure all
	int logical = 0;
	for (int i = 0; i < _container.GetChildCount(); i++)
	{
		if (_container.GetChild(i) is AspectSlot slot)
		{
			slot.CustomMinimumSize = new Vector2(96, 96); // keep size on existing ones, too
			ConfigureSlotInput(slot);
			slot.SetIndex(logical);
			slot.Visible = logical < count;
			logical++;
		}
	}

	// Hide extras
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
