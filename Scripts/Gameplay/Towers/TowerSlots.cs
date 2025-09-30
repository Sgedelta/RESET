using Godot;
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
		if (data.VariantType != Variant.Type.Dictionary)
			return;

		var dict = (Godot.Collections.Dictionary)data;

		if (!dict.TryGetValue("aspect_id", out var idVar))
			return;

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

	private void RefreshIcons()
	{
		var hbox = GetNode<HBoxContainer>("Slots");

		for (int i = 0; i < hbox.GetChildCount(); i++)
		{
			if (hbox.GetChild(i) is not Button slot)
				continue;

			if (i < _tower.AttachedAspects.Count)
			{
				var a = _tower.AttachedAspects[i];
				slot.Text = a.Template.DisplayName;
			}
			else
			{
				slot.Text = "+";
			}
		}
	}

}
