using Godot;

public partial class AspectBar : Control
{
	[Export] public PackedScene TokenScene;
	[Export] public NodePath RowPath = "Panel/Margin/Scroll/Row";
	private HBoxContainer _row;
	private GameManager _gm;

	public override void _Ready()
	{
		_row = GetNode<HBoxContainer>(RowPath);
		_gm  = GetTree().Root.GetNode<GameManager>("/root/Run/GameManager");
		Refresh();
	}

	public void Refresh()
	{
		foreach (Node child in _row.GetChildren())
			child.QueueFree();

		foreach (var aspect in AspectLibrary.AllAspects)
		{
			if (_gm.Inventory.IsOwned(aspect)) continue;
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

		if (_gm.Inventory.DetachFrom(aspect, tower))
		{
			tower.Recompute();
			Refresh();
			var slotsUI = tower.GetNodeOrNull<TowerSlots>("TowerControl/TowerSlots");
			slotsUI?.RefreshIcons();
		}
	}
}
