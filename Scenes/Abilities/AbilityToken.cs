using Godot;

public partial class AbilityToken : Control
{
	[Export] public TextureRect IconRect;
	private AbilityBase _ability;

	public void Init(AbilityBase ability)
	{
		_ability = ability;
		if (IconRect != null) IconRect.Texture = ability?.Icon;
		MouseFilter = MouseFilterEnum.Pass;
		IconRect?.Set("mouse_filter", (int)MouseFilterEnum.Ignore);
	}

	public override void _GuiInput(InputEvent e)
	{
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
