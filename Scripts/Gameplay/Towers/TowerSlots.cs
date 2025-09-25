using Godot;
using System.Linq;

public partial class TowerSlots : Control
{
	[Export] public NodePath TowerPath;
	[Export] public NodePath GameManagerPath;

	private Tower _tower;
	private GameManager _gm;

	public override void _Ready()
	{
		_tower = GetNode<Tower>(TowerPath);
		_gm    = GetNode<GameManager>(GameManagerPath);

		MouseFilter = MouseFilterEnum.Pass;
	}

	public override bool _CanDropData(Vector2 position, Variant data)
	{
		if (data.VariantType != Variant.Type.Dictionary) return false;
		var dict = (Godot.Collections.Dictionary)data;
		return dict.ContainsKey("type")
			&& dict["type"].AsStringName() == "aspect_token"
			&& dict.ContainsKey("name");
	}

	public override void _DropData(Vector2 position, Variant data)
	{
		var dict = (Godot.Collections.Dictionary)data;
		var name = dict["name"].AsString();

		var aspect = AspectLibrary.AllAspects.FirstOrDefault(a => a.Name == name);
		if (aspect == null) return;

		int slotIndex = GetSlotIndexFromPosition(position);
		_gm.Inventory.AttachTo(aspect, _tower, slotIndex);

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
				slot.Text = a.Name;
			}
			else
			{
				slot.Text = "+";
			}
		}
	}

}
