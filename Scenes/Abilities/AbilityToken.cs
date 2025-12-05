using Godot;

public partial class AbilityToken : Control
{
	[Export] public TextureRect IconRect;

	[Export] public NodePath PriceLabelPath;
	[Export] public NodePath PriceIconPath;

	[Export] public Texture2D ScrapIcon;
	[Export] public Texture2D ManaIcon;

	private Label _priceLabel;
	private TextureRect _priceIcon;

	private AbilityBase _ability;

	public void Init(AbilityBase ability)
	{
		_ability = ability;

		IconRect?.Set("mouse_filter", (int)MouseFilterEnum.Ignore);

		_priceLabel = GetNode<Label>(PriceLabelPath);
		_priceIcon  = GetNode<TextureRect>(PriceIconPath);

		UpdateVisual();

		MouseEntered += OnMouseEntered;
		MouseExited  += OnMouseExited;
	}


	private void OnMouseEntered()
	{
		if (_ability == null) return;

		var menu = AbilityHoverMenu.Instance;
		if (menu != null)
		{
			menu.OnTokenMouseEntered(_ability, this);
		}
	}

	private void OnMouseExited()
	{
		var menu = AbilityHoverMenu.Instance;
		if (menu != null)
		{
			menu.OnTokenMouseExited();
		}
	}

	public override void _Process(double delta)
	{
		if (_ability == null) return;
		UpdateVisual();
	}

	public void RefreshFromAbility()
	{
		UpdateVisual();
		QueueRedraw();
	}

private void UpdateVisual()
{
	if (_ability == null) return;

	var gm = GameManager.Instance;
	if (gm == null) return;

	bool isUnlocked = _ability.IsUnlocked;

	int manaCost  = _ability.ManaCost;
	int scrapCost = _ability.ScrapUnlockCost;

	bool canAffordMana  = gm.Mana  >= manaCost;
	bool canAffordScrap = gm.Scrap >= scrapCost;

	bool isSelected = AbilityManager.Instance != null &&
					  AbilityManager.Instance.ArmedAbility == _ability;

	Texture2D iconTex = null;

	if (isSelected && _ability.SelectedIcon != null)
		iconTex = _ability.SelectedIcon;
	else if (!isUnlocked)
		iconTex = canAffordScrap ? (_ability.LockedIcon2 ?? _ability.LockedIcon1)
								 : (_ability.LockedIcon1 ?? _ability.LockedIcon2);
	else
		iconTex = canAffordMana ? (_ability.Icon2 ?? _ability.Icon1)
								: (_ability.Icon1 ?? _ability.Icon2);

	if (IconRect != null && iconTex != null)
		IconRect.Texture = iconTex;

	if (!isUnlocked)
	{
		if (_priceLabel != null)
			_priceLabel.Text = scrapCost.ToString();

		if (_priceIcon != null && ScrapIcon != null)
		{
			_priceIcon.Texture = ScrapIcon;
			_priceIcon.Visible = true;
		}
	}
	else
	{
		if (_priceLabel != null)
			_priceLabel.Text = manaCost.ToString();

		if (_priceIcon != null && ManaIcon != null)
		{
			_priceIcon.Texture = ManaIcon;
			_priceIcon.Visible = true;
		}
	}

	bool clickable = !isUnlocked ? canAffordScrap : canAffordMana;
	MouseFilter = clickable ? MouseFilterEnum.Pass : MouseFilterEnum.Ignore;

}


	public override void _GuiInput(InputEvent e)
	{
		if (_ability == null)
			return;

		var gm = GameManager.Instance;
		if (gm == null)
			return;

		if (e is not InputEventMouseButton { ButtonIndex: MouseButton.Left, Pressed: true })
			return;

		bool isUnlocked = _ability.IsUnlocked;

		if (!isUnlocked)
		{
			if (_ability.TryUnlock(gm))
				RefreshFromAbility();

			AcceptEvent();
			return;
		}

		if (gm.Mana < _ability.ManaCost)
			return;

		AbilityManager.Instance.Arm(_ability);
		AcceptEvent();
	}

	public override Variant _GetDragData(Vector2 atPosition)
	{
		if (_ability == null) return default;

		var gm = GameManager.Instance;
		if (gm == null)
			return default;

		if (!_ability.IsUnlocked)
			return default;

		if (gm.Mana < _ability.ManaCost)
			return default;

		AbilityManager.Instance.Arm(_ability);

		var preview = (Control)Duplicate();
		preview.MouseFilter = MouseFilterEnum.Ignore;
		SetDragPreview(preview);

		return new Godot.Collections.Dictionary
		{
			{ "type", "ability_token" },
			{ "ability", _ability }
		};
	}
}
