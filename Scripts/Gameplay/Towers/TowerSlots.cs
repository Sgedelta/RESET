using Godot;
using System;
using System.Linq;

public partial class TowerSlots : Control
{
	[Export] public NodePath TowerPath;
	[Export] public NodePath GameManagerPath;

	private Tower _tower;
	private GameManager _gm;
	
	[Export] public int SlotIndex = -1;
	private static AspectInventory _aspectInventory = new();



	public override void _Ready()
	{
		_tower = GetNode<Tower>(TowerPath);
		_gm    = GetNode<GameManager>(GameManagerPath);

		MouseFilter = MouseFilterEnum.Pass;

		RefreshIcons();
	}

	public override bool _CanDropData(Vector2 atPos, Variant data)
	{
		if (data.VariantType != Variant.Type.Dictionary)
		return false;
		
		var dict = (Godot.Collections.Dictionary)data;
		return dict.TryGetValue("type", out var typeVar)
			&& (string)typeVar == "aspect_token"
			&& dict.ContainsKey("aspect_id");
	}

	public override void _DropData(Vector2 atPos, Variant data)
	{
		if (data.VariantType != Variant.Type.Dictionary) return;
	
		var dict = (Godot.Collections.Dictionary)data;
		if (!dict.TryGetValue("aspect_id", out var idVar)) return;
	
		string id = (string)idVar;
	
		if (!AspectLibrary.ById.TryGetValue(id, out var aspect))
		{
			GD.PushWarning($"TowerSlots: Unknown aspect id '{id}'");
			return;
		}
	
		if (_tower == null)
		{
			GD.PushWarning("TowerSlots: thisTower not set");
			return;
		}
	
		_aspectInventory.AttachTo(aspect, _tower, SlotIndex);
		RefreshIcons();
	}



	private int GetSlotIndexFromPosition(Vector2 localPosInThis)
	{
		var hbox = GetNode<HBoxContainer>("Slots");
		var globalMouse = GetGlobalMousePosition();
	
		for (int i = 0; i < hbox.GetChildCount(); i++)
		{
			if (hbox.GetChild(i) is Control c)
			{
				if (c.GetGlobalRect().HasPoint(globalMouse))
					return i;
			}
		}
	
		return _tower.AttachedAspects.Count;
	}

	private void DetachSlotFromTower(int slotIndex)
	{
		if (_tower == null) return;
		if (slotIndex >= _tower.AttachedAspects.Count) return;

		GD.Print("removing aspect at " + slotIndex);
		_aspectInventory.DetachFrom(_tower.AttachedAspects[slotIndex], _tower);


		RefreshIcons();

	}
	
	private void RefreshIcons()
	{
		var hbox = GetNode<HBoxContainer>("Slots");
	
		for (int i = 0; i < hbox.GetChildCount(); i++)
		{
	
	        if (hbox.GetChild(i) is not TowerSlot slot)
			{
				continue;
	        }
	
			//detach button connections - we will reatach correctly later
			var connections = slot.GetSignalConnectionList("PressedSlot");
			foreach (var connection in connections)
			{
				//slot.Disconnect("PressedSlot", ((Callable)connection["callable"]));
				slot.PressedSlot -= DetachSlotFromTower;
			}

	
	
	        if (i < _tower.AttachedAspects.Count)
			{
				var a = _tower.AttachedAspects[i];
				slot.Text = a.Template.DisplayName;
				//atach the detach signal to the slot
				slot.PressedSlot += DetachSlotFromTower;
			}
			else
			{
				slot.Text = "+";
			}
		}
	}

	private void DebugPrint()
	{
		GD.Print("Print");
	}

}
