using Godot;

public enum TokenPlace { Bar, Slot }

public partial class AspectToken : Control
{
	public Aspect Aspect { get; private set; }

	private TextureRect _icon;

	public static readonly Vector2 SizeBar  = new(126, 126);
	public static readonly Vector2 SizeSlot = new(96, 96);

	private TokenPlace _place = TokenPlace.Bar;

	// NEW: expose placement for hover logic
	public TokenPlace Place => _place;

	public override void _Ready()
	{
		_icon = GetNodeOrNull<TextureRect>("TextureRect");
		MouseFilter = Control.MouseFilterEnum.Pass;

		if (_icon != null)
		{
			_icon.AnchorLeft   = 0;  _icon.AnchorTop    = 0;
			_icon.AnchorRight  = 1;  _icon.AnchorBottom = 1;
			_icon.StretchMode  = TextureRect.StretchModeEnum.Scale;
			_icon.ExpandMode   = TextureRect.ExpandModeEnum.IgnoreSize;
			_icon.MouseFilter  = Control.MouseFilterEnum.Ignore;
		}

		ApplySize();
		ApplyAnchorPreset();
		ApplyAspectVisual();
	}

	public void Init(Aspect aspect, TokenPlace place)
	{
		Aspect = aspect;
		_place = place;
		if (IsInsideTree())
		{
			ApplySize();
			ApplyAnchorPreset();
			ApplyAspectVisual();
		}
	}

	public void SetPlace(TokenPlace place)
	{
		_place = place;
		ApplySize();
		ApplyAnchorPreset();
	}

	private void ApplySize()
	{
		var size = _place == TokenPlace.Bar ? SizeBar : SizeSlot;
		CustomMinimumSize = size;
		Size = size;
	}

	private void ApplyAnchorPreset()
	{
		if (_place == TokenPlace.Slot)
			SetAnchorsPreset(LayoutPreset.Center);
		else
			SetAnchorsPreset(LayoutPreset.TopLeft);
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
		preview.MouseFilter = Control.MouseFilterEnum.Ignore;
		SetDragPreview(preview);

		var dict = new Godot.Collections.Dictionary
		{
			{ "type", "aspect_token" },
			{ "origin", _place == TokenPlace.Bar ? "bar" : "slot" },
			{ "aspect_id", Aspect.ID }
		};

		// If we're living inside a slot, include tower + slot index
		if (_place == TokenPlace.Slot)
		{
			AspectSlot slot = null;
			Node n = GetParent();
			while (n != null && slot == null)
			{
				slot = n as AspectSlot;
				n = n.GetParent();
			}
			if (slot != null && slot.Pullout?.ActiveTower != null)
			{
				dict["tower_path"] = slot.Pullout.ActiveTower.GetPath().ToString();
				dict["slot_index"] = slot.Index;
			}
		}

		return dict;
	}
}
