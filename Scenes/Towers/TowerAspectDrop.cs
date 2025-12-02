using Godot;
using System;

public partial class TowerAspectDrop : Control
{
	private Tower _tower;

	public override void _Ready()
	{
		// Assuming the Tower is the parent
		_tower = GetParent() as Tower;
		if (_tower == null)
			GD.PushError("[TowerAspectDropTarget] Parent is not a Tower!");
	}

	public override bool _CanDropData(Vector2 atPosition, Variant data)
	{
		if (data.VariantType != Variant.Type.Dictionary) return false;
		var dict = (Godot.Collections.Dictionary)data;

		if (!dict.TryGetValue("type", out var t) || (string)t != "aspect_token")
			return false;

		if (!dict.TryGetValue("origin", out var o)) return false;
		var origin = (string)o;

		// Same logic as AspectSlot / AspectBar – allow bar and slot origins
		bool ok = (origin == "bar"  && dict.ContainsKey("aspect_id"))
			   || (origin == "slot");

		return ok;
	}

	public override void _DropData(Vector2 atPosition, Variant data)
	{
		if (_tower == null) return;
		var dict = (Godot.Collections.Dictionary)data;

		_tower.AttachAspectFromDragData(dict);
	}
}
