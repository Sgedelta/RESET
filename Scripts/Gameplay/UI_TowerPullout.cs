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
		set
		{
			if (animating == value)
				return;
			animating = value;
			if (!animating)
				EmitSignal(SignalName.AnimationStateChanged, value);
		}
	}

	private ScrollContainer i_ScrollContainer;
	private bool _changingTower = false;

	private const string BuySlotMetaKey = "buy_slot";

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
	[Export] private OptionButton _firingModeButton;

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
		if (Animating)
			return;

		_active = !_active;
		TweenToActivePosition();
	}

	public void SetActiveState(bool state)
	{
		if (_active != state) ToggleActive();
	}

	public void TweenToActivePosition()
	{
		Animating = true;

		var slideAnim = CreateTween();
		slideAnim.SetEase(Tween.EaseType.InOut)
				 .SetTrans(Tween.TransitionType.Cubic);

		slideAnim.TweenProperty(
			this,
			"offset",
			new Vector2((_active ? 0 : i_ScrollContainer.Size.X), 0),
			SlideTime
		);

		slideAnim.TweenCallback(Callable.From(() => { Animating = false; }));
	}

	public void SetToActivePosition()
	{
		Offset = new Vector2((_active ? 0 : i_ScrollContainer.Size.X), 0);
	}

	private async void ChangeActiveTower(Tower tower)
	{
		if (_changingTower)
			return;

		_changingTower = true;

		if (Animating)
		{
			await ToSignal(this, SignalName.AnimationStateChanged);
		}

		if (_active)
		{
			_tower?.ShowOrHideRange(false);
			_active = false;
			TweenToActivePosition();

			if (Animating)
				await ToSignal(this, SignalName.AnimationStateChanged);
		}

		_tower = tower;
		RefreshUIs();

		if (!_active)
		{
			_active = true;
			TweenToActivePosition();
		}

		_changingTower = false;
	}

	public void RefreshUIs()
	{
		GD.Print("Refreshing Tower Pullout UIs");

		DisplaySlots();

		for (int i = 0; i < _container.GetChildCount(); i++)
			if (_container.GetChild(i) is AspectSlot slot)
				slot.RefreshVisual();

		if (ActiveTower != null)
		{
            _statDisplay.Text = ActiveTower.StatDisplayBBCode();

			_firingModeButton.Selected = (int)ActiveTower.Targeting.Mode;

        }

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

	/// <summary>
	/// Attach a single GuiInput handler to a slot that knows how to
	/// react when the slot is currently marked as a "buy slot".
	/// Whether the slot is a buy slot or not is controlled via metadata.
	/// </summary>
	private void SetupBuySlotHandler(AspectSlot slot)
	{
		slot.SetMeta(BuySlotMetaKey, false);

		slot.GuiInput += (InputEvent ev) =>
		{
			if (ev is InputEventMouseButton mb &&
				mb.Pressed &&
				mb.ButtonIndex == MouseButton.Left)
			{
				var meta = slot.GetMeta(BuySlotMetaKey);
				if (meta.VariantType == Variant.Type.Bool && (bool)meta)
				{
					OnBuySlotClicked();
					GetViewport()?.SetInputAsHandled();
				}
			}
		};
	}

	private void OnBuySlotClicked()
	{
		if (ActiveTower == null)
		{
			GD.PushWarning("[UI_TowerPullout] Buy slot clicked but no ActiveTower.");
			return;
		}

		var gm = GameManager.Instance;
		if (gm == null)
		{
			GD.PushError("[UI_TowerPullout] GameManager.Instance is null.");
			return;
		}

		bool success = gm.TryBuyTowerSlot(ActiveTower);

		if (!success)
		{
			// TODO: Hook in some feedback ("Not enough scrap") when you add a toast/pop-up system
		}
		// GameManager.TryBuyTowerSlot already refreshes pullouts, so we don't strictly need
		// RefreshUIs() here, but calling again is harmless if you want to be explicit.
	}

	private void ConfigureBuySlotVisual(AspectSlot slot)
	{
		var gm = GameManager.Instance;
		int cost = gm != null ? gm.SlotScrapBaseCost : 0;

	}

	private void ConfigureRealSlotVisual(AspectSlot slot)
	{
		// Reset any visual hints for normal slots
		// slot.Modulate = Colors.White; // If you change Modulate above, reset it here
	}

	public void DisplaySlots(int totalCount)
	{
		if (totalCount < 0)
		{
			GD.PushWarning("[UI_TowerPullout] DisplaySlots called with count < 0");
			totalCount = 0;
		}
		if (AspectSlotScn == null)
		{
			GD.PushError("[UI_TowerPullout] AspectSlotScn not set!");
			return;
		}

		// Count existing AspectSlots
		int slotsFound = 0;
		for (int i = 0; i < _container.GetChildCount(); i++)
			if (_container.GetChild(i) is AspectSlot)
				slotsFound++;

		// Create missing slots up to totalCount
		for (int need = slotsFound; need < totalCount; need++)
		{
			var slot = AspectSlotScn.Instantiate<AspectSlot>();
			slot.CustomMinimumSize = new Vector2(96, 96);
			ConfigureSlotInput(slot);
			SetupBuySlotHandler(slot);
			_container.AddChild(slot);
		}
		int realSlots = AvailableSlots;
		if (realSlots < 0)
			realSlots = 0;

		int logical = 0;
		for (int i = 0; i < _container.GetChildCount(); i++)
		{
			if (_container.GetChild(i) is AspectSlot slot)
			{
				slot.CustomMinimumSize = new Vector2(96, 96);
				ConfigureSlotInput(slot);
				slot.SetIndex(logical);

				bool isReal = logical < realSlots;
				bool isBuy  = logical == realSlots;
				bool visible = logical < realSlots + 1;  // show all real + one buy slot

				slot.Visible = visible;

				slot.SetMeta(BuySlotMetaKey, isBuy);

				if (isBuy)
					ConfigureBuySlotVisual(slot);
				else
					ConfigureRealSlotVisual(slot);

				logical++;
			}
		}
	}

	public void SetTowerTargetingMode(int TargetingModeIndex)
	{
		if(ActiveTower == null)
		{
			GD.PushError("No Active Tower!");
			return;
		}

		ActiveTower.SetTargetingMode((TargetingMode)TargetingModeIndex);

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
		int realSlots = AvailableSlots;
		if (realSlots < 0)
			realSlots = 0;

		// Always display one extra "buy slot"
		DisplaySlots(realSlots + 1);
	}
}
