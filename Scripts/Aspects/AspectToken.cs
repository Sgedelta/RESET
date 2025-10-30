using Godot;

public enum TokenPlace { Bar, Slot }

public partial class AspectToken : Control
{
	public Aspect Aspect { get; private set; }

	private TextureRect _icon;

	public static readonly Vector2 SizeBar  = new(126, 126);
	public static readonly Vector2 SizeSlot = new(96, 96);

	private TokenPlace _place = TokenPlace.Bar;

	public override void _Ready()
	{
		_icon = GetNodeOrNull<TextureRect>("TextureRect");
		MouseFilter = MouseFilterEnum.Pass;

		if (_icon != null)
		{
			_icon.AnchorLeft = 0;  _icon.AnchorTop = 0;
			_icon.AnchorRight = 1; _icon.AnchorBottom = 1;
			_icon.StretchMode = TextureRect.StretchModeEnum.Scale;
			_icon.ExpandMode  = TextureRect.ExpandModeEnum.IgnoreSize;
		}

		ApplySize();
		ApplyAspectVisual();
	}

	public void Init(Aspect aspect, TokenPlace place)
	{
		Aspect = aspect;
		_place = place;
		if (IsInsideTree())
		{
			ApplySize();
			ApplyAspectVisual();
		}
	}

	public void SetPlace(TokenPlace place)
	{
		_place = place;
		ApplySize();
	}

	private void ApplySize()
	{
		var size = _place == TokenPlace.Bar ? SizeBar : SizeSlot;
		CustomMinimumSize = size;
		Size = size; 
	}

	private void ApplyAspectVisual()
	{
		if (Aspect?.Template == null || _icon == null) return;
		_icon.Texture = Aspect.Template.AspectSprite;
	}

	public override Variant _GetDragData(Vector2 atPosition)
	{
		if (Aspect == null) return default;

		var preview = (AspectToken)Duplicate();
		preview.MouseFilter = MouseFilterEnum.Ignore;
		SetDragPreview(preview);

		return new Godot.Collections.Dictionary
		{
			{ "type", "aspect_token" },
			{ "origin", _place == TokenPlace.Bar ? "bar" : "slot" },
			{ "aspect_id", Aspect.ID }
		};
	}
}
