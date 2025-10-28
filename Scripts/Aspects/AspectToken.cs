using Godot;

public partial class AspectToken : Control
{
	public Aspect Aspect { get; private set; }


	private TextureRect _icon;
	private Label _label;

	public override void _Ready()
	{
		_icon  = GetNodeOrNull<TextureRect>("TextureRect");
		_label = GetNodeOrNull<Label>("Label");

		if (_icon != null && Aspect != null) _icon.Texture = Aspect.Template.AspectSprite;
		if (_label != null && Aspect != null) _label.Text = Aspect.Template.DisplayName;
		


		MouseFilter = MouseFilterEnum.Pass;
		
	}

	public void Init(Aspect aspect, Texture2D icon = null)
	{
		Aspect = aspect;
		Aspect.Template.AspectSprite = icon;
		if (IsInsideTree()) _Ready();
	}

	public override Variant _GetDragData(Vector2 atPosition)
	{
		if (Aspect == null || Aspect.Template == null) return default;

		if (GetChildCount() > 0)
		{
			var preview = (Control)Duplicate();
			SetDragPreview(preview);
		}

		var data = new Godot.Collections.Dictionary
		{
			{ "type", "aspect_token" },
			{ "origin", "bar" },
			{ "aspect_id", Aspect.ID }
		};
		return data;
	}

}
