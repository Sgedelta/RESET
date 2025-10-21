	using Godot;
using System;
using System.Linq;

public partial class TowerSlots : Control
{
	[Export] public NodePath TowerPath;
	public Tower Tower { get; private set; }

	private GameManager _gm;
	private AspectBar _bar;

	public override void _Ready()
	{
		AddToGroup("TowerSlotsUI");

		Tower = GetParent()?.GetParent() as Tower;
	if (Tower == null)
	{
		GD.PushError($"[TowerSlots {GetPath()}] Couldn't resolve Tower via ../..");
		return;
	}
		_gm  = GetTree().Root.GetNode<GameManager>("/root/Run/GameManager");
		_bar = GetTree().Root.GetNodeOrNull<AspectBar>("/root/Run/CanvasLayer/AspectBar");

		RefreshIcons();
	}


	public void AttachFromBarToTargetSlot(Godot.Collections.Dictionary data, int targetIndex)
	{
		var aspectId = (string)data["aspect_id"];
		var aspect   = AspectLibrary.GetById(aspectId);
		if (aspect == null) return;

		int empty = Tower.FirstEmptySlotIndex();
		if (empty >= 0)
		{
			if (_gm.Inventory.AttachTo(aspect, Tower, empty))
			{
				Tower.Recompute();
				RefreshAllUIs();
			}
			return;
		}

		var displaced = Tower.GetAspectInSlot(targetIndex);
		if (displaced == null)
		{
			if (_gm.Inventory.AttachTo(aspect, Tower, targetIndex))
			{
				Tower.Recompute();
				RefreshAllUIs();
			}
			return;
		}
		if (_gm.Inventory.DetachFrom(displaced, Tower) &&
			_gm.Inventory.AttachTo(aspect, Tower, targetIndex))
		{
			Tower.Recompute();
			RefreshAllUIs();
		}
	}

	public void AttachFromSlotToTargetSlot(Godot.Collections.Dictionary data, int targetIndex)
	{
		var srcTowerPath = (string)data["tower_path"];
		var srcIndex     = (int)data["slot_index"];
		var srcTower     = GetNode<Tower>(srcTowerPath);

		if (srcTower == Tower && srcIndex == targetIndex)
			return;

		var srcAspect = srcTower.GetAspectInSlot(srcIndex);
		var dstAspect = Tower.GetAspectInSlot(targetIndex);

		if (srcTower == Tower)
		{
			Tower.SwapSlots(srcIndex, targetIndex);
			Tower.Recompute();
			RefreshAllUIs();
			return;
		}

		if (dstAspect == null)
		{
			if (_gm.Inventory.DetachFrom(srcAspect, srcTower) &&
				_gm.Inventory.AttachTo(srcAspect, Tower, targetIndex))
			{
				srcTower.Recompute();
				Tower.Recompute();
				RefreshAllUIs();
			}
			return;
		}

		if (_gm.Inventory.DetachFrom(srcAspect, srcTower) &&
			_gm.Inventory.DetachFrom(dstAspect, Tower) &&
			_gm.Inventory.AttachTo(srcAspect, Tower, targetIndex) &&
			_gm.Inventory.AttachTo(dstAspect, srcTower, srcIndex))
		{
			srcTower.Recompute();
			Tower.Recompute();
			RefreshAllUIs();
		}
	}

	private void RefreshAllUIs()
	{
		RefreshIcons();
		_bar?.Refresh();

		foreach (var node in GetTree().GetNodesInGroup("TowerSlotsUI"))
			(node as TowerSlots)?.RefreshIcons();
	}


	
	public void RefreshIcons()
	{
		var hbox = GetNode<HBoxContainer>("Slots");

		foreach (Node child in hbox.GetChildren())
		{
			if (child is not TowerSlot slot) continue;
			var aspect = Tower.GetAspectInSlot(slot.Index);
			slot.Text = aspect != null ? aspect.Template.DisplayName : "+";
		}
	}

	
	
	private void DebugPrint()
	{
		GD.Print("Print");
	}

}
