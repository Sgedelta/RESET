using Godot;

public partial class AspectToken : Control
{
	public Aspect Aspect { get; private set; }

	[Export] public Texture2D Icon;

	private TextureRect _icon;
	private Label _label;

	public override void _Ready()
	{
		_icon  = GetNodeOrNull<TextureRect>("TextureRect");
		_label = GetNodeOrNull<Label>("Label");

		if (_icon != null && Icon != null) _icon.Texture = Icon;
		if (_label != null && Aspect != null) _label.Text = Aspect.Template.DisplayName;

		MouseFilter = MouseFilterEnum.Pass;
		
	}

	public void Init(Aspect aspect, Texture2D icon = null)
	{
		Aspect = aspect;
		if (icon != null) Icon = icon;
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
			{ "aspect_id", Aspect.Template._id }
		};
		return data;
	}

}
