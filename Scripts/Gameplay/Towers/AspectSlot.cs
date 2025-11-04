using Godot;

public partial class AspectSlot : PanelContainer
{
	[Export] public int Index;
	[Export] public PackedScene TokenScene;

	private UI_TowerPullout _pullout;
	public UI_TowerPullout Pullout => _pullout;

	public Label Label;
	private AspectToken _token;

	private const string TokenSceneFallbackPath = "res://Scenes/Towers/AspectToken.tscn";

	private static readonly Vector2 MinSlotSize = new(96, 96);

	public override void _Ready()
	{
		Node n = GetParent();
		while (n != null && n is not UI_TowerPullout) n = n.GetParent();
		_pullout = n as UI_TowerPullout;

		Label = GetNodeOrNull<Label>("RichTextLabel") ?? GetNodeOrNull<Label>("Label");


		CustomMinimumSize = MinSlotSize;

		if (TokenScene == null)
		{
			var loaded = ResourceLoader.Load<PackedScene>(TokenSceneFallbackPath);
			if (loaded != null) TokenScene = loaded;
			else GD.PushError($"[AspectSlot] TokenScene not set and fallback not found at {TokenSceneFallbackPath}");
		}

		RefreshVisual();
		base._Ready();
	}

	public void SetIndex(int i)
	{
		Index = i;
		// keep the minimum size when indices change
		CustomMinimumSize = MinSlotSize;
		RefreshVisual();
	}

	public void RefreshVisual()
	{
		var aspect = _pullout?.ActiveTower?.GetAspectInSlot(Index);

		if (aspect != null)
		{
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
				_token.MouseFilter = Control.MouseFilterEnum.Ignore;

				_token.AnchorLeft = 0;  _token.AnchorTop = 0;
				_token.AnchorRight = 1; _token.AnchorBottom = 1;
				_token.OffsetLeft = _token.OffsetTop = _token.OffsetRight = _token.OffsetBottom = 0;

				AddChild(_token);
				_token.ZIndex = 100;
				_token.MoveToFront();
			}

			_token.Init(aspect, TokenPlace.Slot);

			if (Label != null) Label.Visible = false;
		}
		else
		{
			if (_token != null)
			{
				_token.QueueFree();
				_token = null;
			}
			if (Label != null)
			{
				Label.Visible = true;
				if (string.IsNullOrEmpty(Label.Text)) Label.Text = "Empty\nSlot";
			}
		}
	}

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

		if (!dict.TryGetValue("type", out var t) || (string)t != "aspect_token")
			return false;

		if (!dict.TryGetValue("origin", out var o)) return false;
		var origin = (string)o;

		bool ok = (origin == "bar"  && dict.ContainsKey("aspect_id"))
			   || (origin == "slot");

		return ok;
	}

	public override void _DropData(Vector2 atPosition, Variant data)
	{
		var dict = (Godot.Collections.Dictionary)data;

		_pullout.Container.AttachAspectToIndex(dict, Index);

		RefreshVisual();
		_pullout.RefreshUIs();
	}
}
