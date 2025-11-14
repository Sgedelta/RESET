using Godot;

public partial class AbilityToken : Control
{
	[Export] public TextureRect IconRect;
	[Export] public Label CooldownLabel;

	private AbilityBase _ability;

	private float _manaFill01 = 0f;

	public void Init(AbilityBase ability)
	{
		_ability = ability;

		if (IconRect != null)
			IconRect.Texture = ability?.Icon;

		IconRect?.Set("mouse_filter", (int)MouseFilterEnum.Ignore);

		UpdateManaVisual();
	}

	public override void _Process(double delta)
	{
		if (_ability == null) return;
		UpdateManaVisual();
	}

	private void UpdateManaVisual()
	{
		if (_ability == null) return;

		var gm = GameManager.Instance;
		if (gm == null) return;

		int currentMana = gm.Mana;
		int cost        = Mathf.Max(_ability.ManaCost, 1);

		float ratio = (float)currentMana / cost;
		_manaFill01 = Mathf.Clamp(ratio, 0f, 1f);

		if (CooldownLabel != null)
		{
			CooldownLabel.Visible = true;
			CooldownLabel.Text    = cost.ToString();
		}

		bool canAfford = currentMana >= cost;

		// Clickable only if you can afford it
		MouseFilter = canAfford ? MouseFilterEnum.Pass : MouseFilterEnum.Ignore;

		Color enabledColor  = new Color(1f, 1f, 1f, 1f);
		Color disabledColor = new Color(1f, 1f, 1f, 0.4f);

		if (IconRect != null)
			IconRect.Modulate = canAfford ? enabledColor : disabledColor;

		if (CooldownLabel != null)
			CooldownLabel.Modulate = canAfford ? enabledColor : disabledColor;

		QueueRedraw();
	}


	public override void _GuiInput(InputEvent e)
	{
		if (_ability == null)
			return;

		var gm = GameManager.Instance;
		if (gm == null)
			return;

		if (gm.Mana < _ability.ManaCost)
			return;

		if (e is InputEventMouseButton { ButtonIndex: MouseButton.Left, Pressed: true })
		{
			AbilityManager.Instance.Arm(_ability);
			AcceptEvent();
		}
	}

	public override Variant _GetDragData(Vector2 atPosition)
	{
		if (_ability == null) return default;

		var gm = GameManager.Instance;
		if (gm == null || gm.Mana < _ability.ManaCost)
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

	public override void _Draw()
	{
		var rect   = GetRect();
		var center = rect.Size / 2f;

		float baseRadius = Mathf.Min(rect.Size.X, rect.Size.Y) * 0.5f;

		float radius = baseRadius * 1.2f;
		int steps = 64;
		Color outlineColor = new Color(0f, 0f, 0f, 1f);
		DrawArc(center, radius, 0f, Mathf.Tau, steps, outlineColor, 2f, true);

		if (_manaFill01 <= 0.001f)
			return;

		float startAngle = -Mathf.Pi / 2f;
		float sweepAngle = Mathf.Tau * _manaFill01;

		Color fillColor = LerpRedToGreen(_manaFill01);
		fillColor.A = 1.0f;

		DrawArc(center, radius, startAngle, startAngle + sweepAngle, steps, fillColor, 4f, true);
	}


	private static Color LerpRedToGreen(float t)
	{
		t = Mathf.Clamp(t, 0f, 1f);
		float r = 1f - t;
		float g = t;
		return new Color(r, g, 0f);
	}
}
