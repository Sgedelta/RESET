using Godot;

public partial class AspectToken : Control
{
	public Aspect Aspect { get; private set; }

	private TextureRect _icon;

	[Export] public Vector2 TokenSize = new Vector2(96, 96);

	public override void _Ready()
	{
		CustomMinimumSize = TokenSize;
		_icon = GetNodeOrNull<TextureRect>("TextureRect");
		MouseFilter = MouseFilterEnum.Pass;

		if (Aspect != null)
			ApplyAspectVisual();
	}

	public void Init(Aspect aspect)
	{
		Aspect = aspect;

		if (IsInsideTree())
			ApplyAspectVisual();
	}

	private void ApplyAspectVisual()
	{
		if (Aspect?.Template == null || _icon == null)
			return;

		_icon.Texture = Aspect.Template.AspectSprite;        // fills the parent rect
	_icon.ExpandMode  = TextureRect.ExpandModeEnum.IgnoreSize;
	}

	public override Variant _GetDragData(Vector2 atPosition)
	{
		if (Aspect == null)
			return default;

		var preview = (AspectToken)Duplicate();
		preview.MouseFilter = MouseFilterEnum.Ignore;

		var hover = preview.GetNodeOrNull<Control>("HoverAspect");
		if (hover != null)
			hover.Visible = false;

		SetDragPreview(preview);

		var data = new Godot.Collections.Dictionary
		{
			{ "type", "aspect_token" },
			{ "origin", "bar" },
			{ "aspect_id", Aspect.ID }
		};

		return data;
	}
}
