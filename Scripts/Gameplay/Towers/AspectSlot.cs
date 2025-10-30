using Godot;
using System;

public partial class AspectSlot : PanelContainer
{
	[Export] public int Index;
	[Export] public PackedScene TokenScene; // assign AspectToken.tscn

	private UI_TowerPullout _pullout;
	public UI_TowerPullout Pullout => _pullout;

	public Label Label;          // your “Empty Slot” label
	private AspectToken _token;  // mounted token (if any)

	public override void _Ready()
	{
		// climb to pullout
		Node n = GetParent();
		while (n != null && n is not UI_TowerPullout) n = n.GetParent();
		_pullout = n as UI_TowerPullout;

		Label = GetNodeOrNull<Label>("RichTextLabel") ?? GetNodeOrNull<Label>("Label");
		ClipChildren = ClipChildrenMode.Only; // keep token inside the slot rect

		RefreshVisual();
		base._Ready();
	}

	public void SetIndex(int i)
	{
		Index = i;
		RefreshVisual();
	}

	public void RefreshVisual()
	{
		var aspect = _pullout?.ActiveTower?.GetAspectInSlot(Index);

		if (aspect != null)
		{
			// Ensure token exists and is sized for SLOT
			if (_token == null)
			{
				if (TokenScene == null)
				{
					GD.PushError("[AspectSlot] TokenScene not set!");
					return;
				}
				_token = TokenScene.Instantiate<AspectToken>();
				_token.Name = "AspectToken";
				_token.FocusMode = FocusModeEnum.None;

				// Make it cover the slot
				_token.AnchorLeft = 0;  _token.AnchorTop = 0;
				_token.AnchorRight = 1; _token.AnchorBottom = 1;
				_token.OffsetLeft = 0;  _token.OffsetTop = 0;
				_token.OffsetRight = 0; _token.OffsetBottom = 0;

				AddChild(_token);
				_token.MoveToFront(); // ensure on top of label
			}

			_token.Init(aspect, TokenPlace.Slot); // <-- SLOT context (96x96)

			if (Label != null) Label.Visible = false;
		}
		else
		{
			// No aspect in this slot → remove token and show label
			if (_token != null)
			{
				_token.QueueFree();
				_token = null;
			}
			if (Label != null)
			{
				Label.Visible = true;
				Label.Text = Label.Text is { Length: > 0 } ? Label.Text : "Empty\nSlot";
			}
		}
	}

	// --- Drag & Drop stays the same ---

	public override Variant _GetDragData(Vector2 atPosition)
	{
		var aspect = _pullout.ActiveTower.GetAspectInSlot(Index);
		if (aspect == null) return default;

		if (GetChildCount() > 0)
		{
			var preview = (Control)Duplicate();
			SetDragPreview(preview);
		}

		return new Godot.Collections.Dictionary
		{
			{ "type","aspect_token" }, { "origin","slot" },
			{ "tower_path", _pullout.ActiveTower.GetPath().ToString() },
			{ "slot_index", Index },
			{ "aspect_id", aspect.ID }
		};
	}

	public override bool _CanDropData(Vector2 atPosition, Variant data)
	{
		if (data.VariantType != Variant.Type.Dictionary) return false;
		var dict = (Godot.Collections.Dictionary)data;
		return dict.TryGetValue("type", out var t) && (string)t == "aspect_token";
	}

	public override void _DropData(Vector2 atPosition, Variant data)
	{
		var dict = (Godot.Collections.Dictionary)data;
		_pullout.Container.AttachAspectToIndex(dict, Index);
		_pullout.RefreshUIs();
		RefreshVisual(); // update this slot immediately
	}
}
