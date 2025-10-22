using Godot;

public partial class AspectBar : Control
{
	[Export] public PackedScene TokenScene;
	[Export] public NodePath RowPath = "Panel/Margin/Scroll/Row";
	private HBoxContainer _row;

	public override void _Ready()
	{
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
			token.Init(aspect);
			_row.AddChild(token);
		}
	}

	public override bool _CanDropData(Vector2 atPosition, Variant data)
	{
		if (data.VariantType != Variant.Type.Dictionary) return false;
		var dict = (Godot.Collections.Dictionary)data;
		return dict.TryGetValue("type", out var t) && (string)t == "aspect_token"
			&& dict.TryGetValue("origin", out var o) && (string)o == "slot";
	}

	public override void _DropData(Vector2 atPosition, Variant data)
	{
		var dict = (Godot.Collections.Dictionary)data;
		var towerPath = (string)dict["tower_path"];
		var slotIndex = (int)dict["slot_index"];

		var tower = GetNode<Tower>(towerPath);
		var aspect = tower.GetAspectInSlot(slotIndex);
		if (aspect == null) return;

		if (GameManager.Instance.Inventory.DetachFrom(aspect, tower))
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
