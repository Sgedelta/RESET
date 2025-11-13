using Godot;

public partial class AbilityHover : Control
{
	[Export] public NodePath WaveDirectorPath;
	[Export] public float AllowedDistanceToPath = 32f;
	[Export] public float HighlightWidth = 20f;

	private WaveDirector _wave;
	private Line2D _line;
	private RewardMenu _rewardMenu;

	public override void _Ready()
	{
		MouseFilter = MouseFilterEnum.Ignore;

		_wave = GetNode<WaveDirector>(WaveDirectorPath);
		Visible = true;

		_line = new Line2D
		{
			Width = HighlightWidth,
			DefaultColor = new Color(0.2f, 0.6f, 1f, 0.35f),
			Visible = false,
			ZIndex = 10
		};
		AddChild(_line);
		RebuildHighlightFromPath();

		AddToGroup("ability_hover");

		AbilityManager.Instance.AbilitySelected += OnAbilitySelected;

		var menus = GetTree().GetNodesInGroup("reward_menu");
		if (menus.Count > 0 && menus[0] is RewardMenu rm)
		{
			_rewardMenu = rm;
			_rewardMenu.VisibilityChanged += OnRewardMenuVisibilityChanged;
		}
	}

	// If reward menu opens, turn the overlay off
	private void OnRewardMenuVisibilityChanged()
	{
		if (_rewardMenu.Visible)
		{
			MouseFilter = MouseFilterEnum.Ignore;
			_line.Visible = false;
			if (AbilityManager.Instance.ArmedAbility != null)
				AbilityManager.Instance.Disarm();
		}
	}

	private void OnAbilitySelected(AbilityBase ability)
	{
		// Don’t enable overlay if reward menu is open
		bool armed = ability != null && (_rewardMenu == null || !_rewardMenu.Visible);
		MouseFilter = armed ? MouseFilterEnum.Stop : MouseFilterEnum.Ignore;
		_line.Visible = armed;
		if (armed) RebuildHighlightFromPath();
	}

	private void RebuildHighlightFromPath()
	{
		_line.ClearPoints();
		var curve = _wave?.Curve;
		if (curve == null) return;
		foreach (var p in curve.GetBakedPoints())
			_line.AddPoint(p);
	}

	public override bool _CanDropData(Vector2 atPosition, Variant data)
	{
		if (_rewardMenu?.Visible == true) return false;
		if (!IsAbilityDict(data, out _)) return false;
		var worldPos = GetViewport().GetMousePosition();
		return IsNearPath(worldPos);
	}

	public override void _DropData(Vector2 atPosition, Variant data)
	{
		if (_rewardMenu?.Visible == true) return;
		if (!IsAbilityDict(data, out var _)) return;

		var worldPos = GetViewport().GetMousePosition();
		if (IsNearPath(worldPos))
			AbilityManager.Instance.PlaceAt(worldPos);
	}

	public override void _GuiInput(InputEvent e)
	{
		if (_rewardMenu?.Visible == true) return; // ignore while menu is up
		if (AbilityManager.Instance.ArmedAbility == null) return;

		if (e is InputEventMouseButton { ButtonIndex: MouseButton.Left, Pressed: true })
		{
			var worldPos = GetViewport().GetMousePosition();
			if (IsNearPath(worldPos))
				AbilityManager.Instance.PlaceAt(worldPos);
			else
				AbilityManager.Instance.Disarm();
			AcceptEvent();
		}

		if (e is InputEventMouseButton { ButtonIndex: MouseButton.Right, Pressed: true } || e.IsActionPressed("ui_cancel"))
		{
			AbilityManager.Instance.Disarm();
			AcceptEvent();
		}
	}

	private bool IsAbilityDict(Variant data, out AbilityBase ability)
	{
		ability = null;
		if (data.VariantType != Variant.Type.Dictionary) return false;
		var dict = (Godot.Collections.Dictionary)data;
		if (!dict.TryGetValue("type", out var t) || (string)t != "ability_token") return false;
		if (!dict.TryGetValue("ability", out var a) || a.VariantType != Variant.Type.Object) return false;
		ability = a.As<AbilityBase>();
		return ability != null;
	}

	private bool IsNearPath(Vector2 worldPos)
	{
		var path = _wave?.Path;
		var curve = _wave?.Curve;
		if (path == null || curve == null) return false;

		var local = path.ToLocal(worldPos);
		var closestLocal = curve.GetClosestPoint(local);
		var closestWorld = path.ToGlobal(closestLocal);
		return worldPos.DistanceTo(closestWorld) <= AllowedDistanceToPath;
	}
	
}
