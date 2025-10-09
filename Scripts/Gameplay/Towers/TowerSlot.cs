using Godot;
using System;
using System.Linq;


public partial class TowerSlot : Button
{
	[Export] public int Index;
	[Export] public NodePath TowerSlotsPath;
	private TowerSlots _slots;
	private Tower _tower;
	private GameManager _gm;

	public override void _Ready()
	{
		_slots = GetNode<TowerSlots>(TowerSlotsPath);
		_gm = GetTree().Root.GetNode<GameManager>("/root/Run/GameManager");
		MouseFilter = MouseFilterEnum.Stop;
	}

	public override Variant _GetDragData(Vector2 atPosition)
	{
		_tower = _slots?.Tower; 
		GD.Print($"[TowerSlot {_tower?.Name ?? "null"}:{Index}] _GetDragData called. Disabled={Disabled}");
		
	
		if (_tower == null) { GD.Print("  -> NO TOWER"); return default; }

		var aspect = _tower.GetAspectInSlot(Index);
		GD.Print($"  -> aspect in slot? {(aspect != null ? aspect.Template._id : "NULL")}");

		if (aspect == null) return default;

		if (GetChildCount() > 0)
		{
			var preview = (Control)Duplicate();
			SetDragPreview(preview);
		}

		GD.Print($"  -> starting drag with {aspect.Template._id}");
		return new Godot.Collections.Dictionary {
			{ "type","aspect_token" }, { "origin","slot" },
			{ "tower_path", _tower.GetPath().ToString() },
			{ "slot_index", Index },
			{ "aspect_id", aspect.Template._id }
		};
	}

	public override bool _CanDropData(Vector2 atPosition, Variant data)
	{
		if (data.VariantType != Variant.Type.Dictionary) return false;
		var dict = (Godot.Collections.Dictionary)data;
		if (!dict.TryGetValue("type", out var t) || (string)t != "aspect_token") return false;
		return true;
	}

	public override void _DropData(Vector2 atPosition, Variant data)
	{
		var dict = (Godot.Collections.Dictionary)data;
		var origin = (string)dict["origin"];

		if (origin == "bar")
		{
			_slots.AttachFromBarToTargetSlot(dict, Index);
		}
		else if (origin == "slot")
		{
			_slots.AttachFromSlotToTargetSlot(dict, Index);
		}
	}
}
