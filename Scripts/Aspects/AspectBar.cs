using Godot;

public partial class AspectBar : Control
{
	[Export] public PackedScene TokenScene;
	[Export] public NodePath RowPath = "Panel/Margin/Scroll/Row";
	private HBoxContainer _row;
	public static AspectBar Instance;

	public override void _Ready()
	{
		if (Instance != null && Instance != this)
			QueueFree();
		else
			Instance = this;

		_row = GetNode<HBoxContainer>(RowPath);
		Refresh();
	}

	public void Refresh()
	{
		GD.Print("Refreshing Bar");
		foreach (Node child in _row.GetChildren()) child.QueueFree();

		foreach (var aspect in GameManager.Instance.Inventory.BagAspects())
		{
			if (GameManager.Instance.Inventory.IsAttached(aspect)) continue;

			var token = TokenScene.Instantiate<AspectToken>();
			token.Init(aspect, TokenPlace.Bar);
			token.FocusMode = Control.FocusModeEnum.None;
			_row.AddChild(token);
		}
	}

	public override bool _CanDropData(Vector2 atPosition, Variant data)
	{
		if (data.VariantType != Variant.Type.Dictionary) return false;
		var dict = (Godot.Collections.Dictionary)data;

		// Accept either slot-origin payload, or bar-origin with just aspect_id
		if (!dict.TryGetValue("type", out var t) || (string)t != "aspect_token")
			return false;

		if (!dict.TryGetValue("origin", out var o)) return false;
		var origin = (string)o;

		if (origin == "slot")
			return true; // slot → bar detach
		if (origin == "bar")
			return dict.ContainsKey("aspect_id"); // no-op but safe

		return false;
	}

	public override void _DropData(Vector2 atPosition, Variant data)
	{
		var dict = (Godot.Collections.Dictionary)data;

		var inv = GameManager.Instance.Inventory;
		Aspect aspect = null;

		// Resolve aspect regardless of payload shape
		if (dict.ContainsKey("aspect_id"))
		{
			aspect = inv.GetByID((string)dict["aspect_id"]);
		}

		Tower tower = null;

		if (dict.ContainsKey("tower_path") && dict.ContainsKey("slot_index"))
		{
			var towerPath = (string)dict["tower_path"];
			var slotIndex = (int)dict["slot_index"];
			tower = GetNode<Tower>(towerPath);

			if (aspect == null && tower != null)
				aspect = tower.GetAspectInSlot(slotIndex);
		}

		if (aspect == null)
		{
			GD.PushWarning("[AspectBar] Drop ignored: aspect couldn't be resolved.");
			return;
		}

		// If we know the owner, detach from it
		tower ??= inv.AttachedTo(aspect);
		if (tower == null)
		{
			// already unattached – just refresh bar
			Refresh();
			return;
		}

		if (inv.DetachFrom(aspect, tower))
		{
			tower.Recompute();
			Refresh();
			RefreshPulloutsForTower(tower);
		}
	}

	private void RefreshPulloutsForTower(Tower tower)
	{
		foreach (var n in GetTree().GetNodesInGroup("tower_pullout"))
		{
			if (n is UI_TowerPullout p && p.ActiveTower == tower)
				p.RefreshUIs();
		}
	}
}
