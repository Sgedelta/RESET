using Godot;

public partial class ScrapConfirmMenu : Control
{
	[Signal] public delegate void ConfirmedEventHandler();
	[Signal] public delegate void CancelledEventHandler();

	[Export] private Label _nameLabel;
	[Export] private Label _rarityLabel;

	[Export] private VBoxContainer _rewardsContainer;
	[Export] private HBoxContainer _scrapRow;
	[Export] private Label _scrapValueLabel;
	[Export] private HBoxContainer _manaRow;
	[Export] private Label _manaValueLabel;

	[Export] private Button _confirmButton;
	[Export] private Button _cancelButton;

	private Aspect _aspect;

	public override void _Ready()
	{
		MouseFilter = MouseFilterEnum.Stop;

		if (_confirmButton != null)
			_confirmButton.Pressed += () => EmitSignal(SignalName.Confirmed);

		if (_cancelButton != null)
			_cancelButton.Pressed += () => EmitSignal(SignalName.Cancelled);
	}

	public void Initialize(Aspect aspect)
	{
		_aspect = aspect;
		var template = aspect.Template;

		if (_nameLabel != null)
			_nameLabel.Text = template.DisplayName ?? aspect.ToString();

		if (_rarityLabel != null)
			_rarityLabel.Text = template.Rarity.ToString();

		int scrap = template.ScrapAmount;
		int mana  = template.ManaAmount;

		// Scrap row
		if (_scrapRow != null)
		{
			if (scrap > 0)
			{
				_scrapRow.Visible = true;
				if (_scrapValueLabel != null)
					_scrapValueLabel.Text = FormatValue(scrap);
			}
			else
			{
				_scrapRow.Visible = false;
			}
		}

		// Mana row
		if (_manaRow != null)
		{
			if (mana > 0)
			{
				_manaRow.Visible = true;
				if (_manaValueLabel != null)
					_manaValueLabel.Text = FormatValue(mana);
			}
			else
			{
				_manaRow.Visible = false;
			}
		}

		if (_rewardsContainer != null)
			_rewardsContainer.Visible = (scrap > 0 || mana > 0);
	}

	private string FormatValue(int value)
	{
		if (value >= 1000)
		{
			float k = value / 1000f;
			// 3000 -> 3K, 1500 -> 1.5K
			return (k % 1f == 0f) ? $"{(int)k}K" : $"{k:0.#}K";
		}

		return value.ToString();
	}
}
