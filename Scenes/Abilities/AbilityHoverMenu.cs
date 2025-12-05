using Godot;

public partial class AbilityHoverMenu : Control
{
	[Export] public NodePath NamePath          = "Panel/MarginContainer/VBoxContainer/Name";
	[Export] public NodePath LevelPath         = "Panel/MarginContainer/VBoxContainer/Level";
	[Export] public NodePath CostPath          = "Panel/MarginContainer/VBoxContainer/Cost";
	[Export] public NodePath UpgradeButtonPath = "Panel/MarginContainer/VBoxContainer/UpgradeButton";
	[Export] public NodePath BackgroundPath    = "Panel/TextureRect";
	[Export] public NodePath PanelPath         = "Panel";
	[Export] public NodePath CostRowLabelPath   = "Panel/MarginContainer/VBoxContainer/CostRow/Cost";
	[Export] public NodePath CostRowIconPath    = "Panel/MarginContainer/VBoxContainer/CostRow/CostIcon";
	[Export] public NodePath UpgradeTextPath    = "Panel/MarginContainer/VBoxContainer/UpgradeButton/HBoxContainer/UpgradeText";
	[Export] public NodePath UpgradeIconPath    = "Panel/MarginContainer/VBoxContainer/UpgradeButton/HBoxContainer/UpgradeIcon";

	[Export] public Texture2D ScrapIcon;
	[Export] public Texture2D ManaIcon;

	private Label _costRowLabel;
	private TextureRect _costRowIcon;
	private Label _upgradeText;
	private TextureRect _upgradeIcon;


	[Export] public Vector2 MouseOffset = new Vector2(0, 0);
	[Export] public float ClampPadding  = 8f;
	[Export] public float HideDelay     = 0.15f; // seconds

	private Label _name;
	private Label _level;
	private Label _cost;
	private Button _upgradeButton;
	private TextureRect _bg;
	private Control _panel;

	private AbilityBase  _ability;
	private AbilityToken _token;

	private bool _hoveringToken;
	private bool _hoveringMenu;
	private bool _pendingHide;
	private float _hideCountdown;

	public static AbilityHoverMenu Instance { get; private set; }

	public override void _EnterTree()
	{
		Instance = this;
	}

	public override void _Ready()
	{
		MouseFilter = MouseFilterEnum.Stop;

		_name          = GetNode<Label>(NamePath);
		_level         = GetNode<Label>(LevelPath);
		_cost          = GetNode<Label>(CostPath); // you can stop using this if you want
		_upgradeButton = GetNode<Button>(UpgradeButtonPath);
		_bg            = GetNodeOrNull<TextureRect>(BackgroundPath);
		_panel         = GetNodeOrNull<Control>(PanelPath);

		_costRowLabel  = GetNode<Label>(CostRowLabelPath);
		_costRowIcon   = GetNode<TextureRect>(CostRowIconPath);
		_upgradeText   = GetNode<Label>(UpgradeTextPath);
		_upgradeIcon   = GetNode<TextureRect>(UpgradeIconPath);

		_upgradeButton.Pressed += OnUpgradePressed;

		Visible = false;
		AddToGroup("AbilityTooltip");

		if (_panel != null)
		{
			_panel.MouseEntered += OnMenuMouseEntered;
			_panel.MouseExited  += OnMenuMouseExited;
		}
		if (_upgradeButton != null)
		{
			_upgradeButton.MouseEntered += OnMenuMouseEntered;
			_upgradeButton.MouseExited  += OnMenuMouseExited;
		}
		if (_panel == null)
		{
			MouseEntered += OnMenuMouseEntered;
			MouseExited  += OnMenuMouseExited;
		}

		SetProcess(true);
	}

	public override void _Process(double delta)
	{
		if (_pendingHide)
		{
			_hideCountdown -= (float)delta;
			if (_hideCountdown <= 0f)
			{
				_pendingHide = false;
				if (!_hoveringToken && !_hoveringMenu)
					Hide();
			}
		}
		if (Visible && _ability != null)
		{
			RefreshText();
		}
	}

	private void OnMenuMouseEntered()
	{
		_hoveringMenu = true;
		_pendingHide = false;
	}

	private void OnMenuMouseExited()
	{
		_hoveringMenu = false;
		ScheduleHide();
	}

	private void ScheduleHide()
	{
		_pendingHide = true;
		_hideCountdown = HideDelay;
	}

	private void OnUpgradePressed()
	{
		if (_ability == null) return;

		var gm = GameManager.Instance;
		if (gm == null) return;

		if (_ability.TryUpgrade(gm))
		{
			RefreshText();
			_token?.RefreshFromAbility();
		}
	}

	public void ShowForToken(AbilityBase ability, AbilityToken token)
	{
		if (ability == null || token == null)
			return;

		_ability = ability;
		_token   = token;
		_hoveringToken = true;
		_hoveringMenu  = false;
		_pendingHide   = false; 

		RefreshText();
		ApplyAbilityBackground();

		CallDeferred(nameof(PlaceMenuAtControlClamped), token.GetPath());
		Show();
	}

	public void OnTokenMouseEntered(AbilityBase ability, AbilityToken token)
	{
		ShowForToken(ability, token);
	}

	public void OnTokenMouseExited()
	{
		_hoveringToken = false;
		ScheduleHide();
	}

private void RefreshText()
{
	if (_ability == null)
		return;

	var gm = GameManager.Instance;

	_name.Text  = _ability.AbilityName;
	_level.Text = $"Level {_ability.CurrentLevel}/{_ability.MaxLevel}";

	_cost.Text = "";


	if (!_ability.IsUnlocked)
	{
		int scrapCost = _ability.ScrapUnlockCost;
		_costRowLabel.Text = $"Unlock: {scrapCost}";
		_costRowIcon.Texture = ScrapIcon;
		
		_costRowIcon.Visible = true;
		_upgradeButton.Visible  = false;
		_upgradeButton.Disabled = true;

		ApplyAbilityBackground();
		return;
	}

	int manaCost = _ability.ManaCost;

	_costRowLabel.Text = $"Cast: {manaCost}";

	if (_costRowIcon != null && ManaIcon != null)
	{
		_costRowIcon.Texture = ManaIcon;
		_costRowIcon.Visible = true;
	}

	if (gm != null && _ability.CanUpgrade(gm))
	{
		int nextLevel   = _ability.CurrentLevel + 1;
		int upgradeCost = _ability.GetUpgradeCost(nextLevel);
		bool canAffordUpgrade = gm.Mana >= upgradeCost;

		_upgradeButton.Visible  = true;
		_upgradeButton.Disabled = !canAffordUpgrade;

		if (_upgradeText != null)
			_upgradeText.Text = $" Upgrade     {upgradeCost}";

		if (_upgradeIcon != null && ManaIcon != null)
		{
			_upgradeIcon.Texture = ManaIcon;
			_upgradeIcon.Visible = true;
		}
	}
	else
	{
		_upgradeButton.Visible  = false;
		_upgradeButton.Disabled = true;
	}

	ApplyAbilityBackground();
}


	private void ApplyAbilityBackground()
	{
		if (_bg == null || _ability == null)
			return;

		if (_ability.HoverBackground != null)
		{
			_bg.Texture = _ability.HoverBackground;
			_bg.Modulate = Colors.White;
		}
	}

	private void PlaceMenuAtControlClamped(NodePath targetPath)
	{
		var target = GetNodeOrNull<Control>(targetPath);
		if (target == null) return;

		var targetRect = target.GetGlobalRect();
		const float verticalOffset = -300f;

		Vector2 desired = new Vector2(
			targetRect.Position.X - 100f,
			targetRect.Position.Y + verticalOffset
		);

		desired += MouseOffset;

		GlobalPosition = ClampToViewport(desired, GetMenuSize());
	}

	private Vector2 GetMenuSize()
	{
		var min = GetCombinedMinimumSize();
		return new Vector2(Mathf.Max(Size.X, min.X), Mathf.Max(Size.Y, min.Y));
	}

	private Vector2 ClampToViewport(Vector2 desiredGlobalPos, Vector2 menuSize)
	{
		var vp = GetViewport().GetVisibleRect();
		float pad = ClampPadding;

		float minX = vp.Position.X + pad;
		float minY = vp.Position.Y + pad;
		float maxX = vp.End.X - pad - menuSize.X;
		float maxY = vp.End.Y - pad - menuSize.Y;

		return new Vector2(
			Mathf.Clamp(desiredGlobalPos.X, minX, Mathf.Max(minX, maxX)),
			Mathf.Clamp(desiredGlobalPos.Y, minY, Mathf.Max(minY, maxY))
		);
	}
}
