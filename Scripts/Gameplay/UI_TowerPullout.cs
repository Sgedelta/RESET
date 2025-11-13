using Godot;
using System.Threading.Tasks;

public partial class UI_TowerPullout : CanvasLayer
{
	private bool _active = false;
	public bool Active { get { return _active; } }

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
		SetToActivePosition();

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
			_tower.ShowOrHideRange(false);
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

		DisplaySlots();

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

		slot.MouseFilter = Control.MouseFilterEnum.Stop;

		foreach (var ch in slot.GetChildren())
			if (ch is Control cc)
				cc.MouseFilter = Control.MouseFilterEnum.Ignore;

	}

	public void DisplaySlots(int count)
	{
		if (count < 0) { GD.PushWarning("[UI_TowerPullout] DisplaySlots called with count < 0"); count = 0; }
		if (AspectSlotScn == null) { GD.PushError("[UI_TowerPullout] AspectSlotScn not set!"); return; }

		int slotsFound = 0;
		for (int i = 0; i < _container.GetChildCount(); i++)
			if (_container.GetChild(i) is AspectSlot) slotsFound++;

		// Create missing
		for (int need = slotsFound; need < count; need++)
		{
			var slot = AspectSlotScn.Instantiate<AspectSlot>();
			slot.CustomMinimumSize = new Vector2(96, 96);
			ConfigureSlotInput(slot);
			_container.AddChild(slot);
		}

		int logical = 0;
		for (int i = 0; i < _container.GetChildCount(); i++)
		{
			if (_container.GetChild(i) is AspectSlot slot)
			{
				slot.CustomMinimumSize = new Vector2(96, 96);
				ConfigureSlotInput(slot);
				slot.SetIndex(logical);
				slot.Visible = logical < count;
				logical++;
			}
		}

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
	public override void _UnhandledInput(InputEvent e)
	{
		if (!_active || animating) return;
		if (e is InputEventMouseButton mb && mb.Pressed)
		{
			_tower.ShowOrHideRange(false);
			ToggleActive();
			GetViewport().SetInputAsHandled();
		}
	}

	public void DisplaySlots()
	{
		DisplaySlots(AvailableSlots);
	}
}
