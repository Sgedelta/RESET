using Godot;

public partial class AbilityToken : Control
{
	[Export] public TextureRect IconRect;
	[Export] public Label CooldownLabel;

	private AbilityBase _ability;

	public void Init(AbilityBase ability)
	{
		if (IconRect != null) IconRect.Texture = ability?.Icon;

		_ability = ability;
		MouseFilter = ability.IsOnCooldown ? MouseFilterEnum.Ignore : MouseFilterEnum.Pass;
		IconRect?.Set("mouse_filter", (int)MouseFilterEnum.Ignore);

		UpdateCooldownVisual();
	}
	public override void _Process(double delta)
	{
		if (_ability == null) return;
		UpdateCooldownVisual();
	}

	
	private void UpdateCooldownVisual()
	{
		if (_ability.IsOnCooldown)
		{
			CooldownLabel.Visible = true;
			CooldownLabel.Text = Mathf.Ceil(_ability.CurrentCooldown).ToString();
			MouseFilter = MouseFilterEnum.Ignore;
		}
		else
		{
			CooldownLabel.Visible = false;
			MouseFilter = MouseFilterEnum.Pass;
		}
	}


	public override void _GuiInput(InputEvent e)
	{
		if (_ability == null || _ability.IsOnCooldown)
		return;
	
		if (e is InputEventMouseButton { ButtonIndex: MouseButton.Left, Pressed: true } && _ability != null)
		{
			AbilityManager.Instance.Arm(_ability);
			AcceptEvent();
		}
	}

	public override Variant _GetDragData(Vector2 atPosition)
	{
		if (_ability == null) return default;

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
