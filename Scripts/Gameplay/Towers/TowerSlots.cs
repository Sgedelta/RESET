	using Godot;
using System;
using System.Collections.Generic;

public partial class TowerSlots : Control
{
	[Export] public NodePath TowerPath;
	[Export] public NodePath SlotsRowPath = "Slots";

	public Tower Tower { get; private set; }

	private GameManager _gm;
	private HBoxContainer _row;

	public override void _Ready()
	{
		_gm  = GameManager.Instance ?? GetTree().Root.GetNode<GameManager>("/root/Run/GameManager");
		Tower = !string.IsNullOrEmpty(TowerPath) ? GetNode<Tower>(TowerPath) : GetParent<Tower>();
		_row  = GetNode<HBoxContainer>(SlotsRowPath);

		AddToGroup("TowerSlotsUI");
		RefreshIcons();
	}

	// --- UI helpers ----------------------------------------------------------

	public void RefreshIcons()
	{
		if (Tower == null || _row == null) return;

		for (int i = 0; i < _row.GetChildCount(); i++)
		{
			if (_row.GetChild(i) is not TowerSlot slot) continue;

			var aspect = Tower.GetAspectInSlot(i);
			slot.Text = aspect != null ? aspect.Template.DisplayName : "+";
		}
	}

	private int FindFirstEmptySlotIndex()
	{
		if (Tower == null || _row == null) return 0;
		for (int i = 0; i < _row.GetChildCount(); i++)
		{
			if (Tower.GetAspectInSlot(i) == null)
				return i;
		}
		return 0;
	}

	private int GetSlotIndexFromPosition(Vector2 localPos)
	{
		if (_row == null) return FindFirstEmptySlotIndex();

		for (int i = 0; i < _row.GetChildCount(); i++)
		{
			if (_row.GetChild(i) is Control slotCtrl)
			{
				var rect = new Rect2(slotCtrl.Position, slotCtrl.Size);
				if (rect.HasPoint(localPos))
					return i;
			}
		}

		return FindFirstEmptySlotIndex();
	}

	private Aspect TakeUnattachedInstance(AspectTemplate template)
	{
		if (template == null) return null;
		foreach (var a in _gm.Inventory.BagAspects())
		{
			if (!_gm.Inventory.IsAttached(a) && a.Template == template)
				return a;
		}
		return null;
	}


	public override bool _CanDropData(Vector2 atPosition, Variant data)
	{
		if (data.VariantType != Variant.Type.Dictionary) return false;
		var dict = (Godot.Collections.Dictionary)data;

		if (!dict.TryGetValue("type", out var t) || (string)t != "aspect_token")
			return false;

		if (dict.TryGetValue("origin", out var o))
		{
			var origin = (string)o;
			if (origin == "bar")
			{
				return dict.ContainsKey("aspect_id");
			}
			else if (origin == "slot")
			{
				return dict.ContainsKey("tower_path") && dict.ContainsKey("slot_index");
			}
		}

		return false;
	}

	public override void _DropData(Vector2 atPosition, Variant data)
	{
		var dict = (Godot.Collections.Dictionary)data;
		var origin = (string)dict["origin"];

		// Determine target slot
		int targetIndex = dict.ContainsKey("slot_index")
			? (int)dict["slot_index"]                // some UIs pass it along
			: GetSlotIndexFromPosition(atPosition);  // otherwise calculate from mouse

		if (origin == "bar")
		{
			// Drag from AspectBar: dict contains "aspect_id" (TEMPLATE id)
			var templateId = (string)dict["aspect_id"];
			var template   = AspectLibrary.GetTemplate(templateId);
			if (template == null) return;

			var instance = TakeUnattachedInstance(template);
			if (instance == null) return; // no free copy available

			if (_gm.Inventory.AttachTo(instance, Tower, targetIndex))
			{
				Tower.Recompute();
				RefreshIcons();
				GetNodeOrNull<AspectBar>("/root/Run/AspectBar")?.Refresh();
			}
		}
		else if (origin == "slot")
		{
			// Moving an existing instance from a source slot (possibly another tower)
			var sourceTowerPath = (string)dict["tower_path"];
			var sourceSlotIndex = (int)dict["slot_index"];

			var sourceTower = GetNodeOrNull<Tower>(sourceTowerPath);
			if (sourceTower == null) return;

			var aspect = sourceTower.GetAspectInSlot(sourceSlotIndex);
			if (aspect == null) return;

			// If moving within the same tower to a different slot, simplest is detach & attach
			// (You could add a Tower.MoveAspectSlot(aspect, newIndex) later for efficiency)
			if (_gm.Inventory.DetachFrom(aspect, sourceTower))
			{
				if (_gm.Inventory.AttachTo(aspect, Tower, targetIndex))
				{
					Tower.Recompute();
					sourceTower.Recompute();

					// Refresh both UIs if available
					RefreshIcons();

					var otherSlots = sourceTower == Tower
						? this
						: sourceTower.GetNodeOrNull<TowerSlots>("TowerControl/TowerSlots");
					otherSlots?.RefreshIcons();

					GetNodeOrNull<AspectBar>("/root/Run/AspectBar")?.Refresh();
				}
				else
				{
					// Failed to attach to new slot -> put it back where it came from
					_gm.Inventory.AttachTo(aspect, sourceTower, sourceSlotIndex);
					sourceTower.Recompute();
					var slotsUI = sourceTower.GetNodeOrNull<TowerSlots>("TowerControl/TowerSlots");
					slotsUI?.RefreshIcons();
				}
			}
		}
	}

	public void AttachFromBarToTargetSlot(Godot.Collections.Dictionary dict, int targetIndex)
	{
		var gm = GameManager.Instance ?? GetTree().Root.GetNode<GameManager>("/root/Run/GameManager");
		var templateId = (string)dict["aspect_id"];
		var template   = AspectLibrary.GetTemplate(templateId);
		if (template == null) return;

		var instance = TakeUnattachedInstance(template);
		if (instance == null) return;

		if (gm.Inventory.AttachTo(instance, Tower, targetIndex))
		{
			Tower.Recompute();
			RefreshIcons();
			GetNodeOrNull<AspectBar>("/root/Run/CanvasLayer/AspectBar")?.Refresh();
		}
	}

	public void AttachFromSlotToTargetSlot(Godot.Collections.Dictionary dict, int targetIndex)
	{
		var gm = GameManager.Instance ?? GetTree().Root.GetNode<GameManager>("/root/Run/GameManager");

		var sourceTowerPath = (string)dict["tower_path"];
		var sourceSlotIndex = (int)dict["slot_index"];

		var sourceTower = GetNodeOrNull<Tower>(sourceTowerPath);
		if (sourceTower == null) return;

		var aspect = sourceTower.GetAspectInSlot(sourceSlotIndex);
		if (aspect == null) return;

		if (gm.Inventory.DetachFrom(aspect, sourceTower))
		{
			if (gm.Inventory.AttachTo(aspect, Tower, targetIndex))
			{
				Tower.Recompute();
				sourceTower.Recompute();

				RefreshIcons();

				var otherSlots = sourceTower == Tower
					? this
					: sourceTower.GetNodeOrNull<TowerSlots>("TowerControl/TowerSlots");
				otherSlots?.RefreshIcons();

				GetNodeOrNull<AspectBar>("/root/Run/CanvasLayer/AspectBar")?.Refresh();
			}
			else
			{
				gm.Inventory.AttachTo(aspect, sourceTower, sourceSlotIndex);
				sourceTower.Recompute();
				var srcUI = sourceTower.GetNodeOrNull<TowerSlots>("TowerControl/TowerSlots");
				srcUI?.RefreshIcons();
			}
		}
	}

}
