using Godot;

public partial class ScrapBox : PanelContainer
{
	public override bool _CanDropData(Vector2 atPosition, Variant data)
	{
		if (data.VariantType != Variant.Type.Dictionary)
			return false;

		var dict = (Godot.Collections.Dictionary)data;

		if (!dict.TryGetValue("type", out var t) || (string)t != "aspect_token")
			return false;

		if (dict.ContainsKey("aspect_id"))
			return true;

		if (dict.ContainsKey("tower_path") && dict.ContainsKey("slot_index"))
			return true;

		return false;
	}

	public override void _DropData(Vector2 atPosition, Variant data)
	{
		var dict = (Godot.Collections.Dictionary)data;

		var gm  = GameManager.Instance;
		var inv = gm.Inventory;

		Aspect aspect = null;
		Tower tower   = null;

		if (dict.ContainsKey("aspect_id"))
		{
			var id = (string)dict["aspect_id"];
			aspect = inv.GetByID(id);
		}

		if (dict.ContainsKey("tower_path") && dict.ContainsKey("slot_index"))
		{
			var towerPath = (string)dict["tower_path"];
			var slotIndex = (int)dict["slot_index"];

			tower = GetNodeOrNull<Tower>(towerPath);
			if (aspect == null && tower != null)
				aspect = tower.GetAspectInSlot(slotIndex);
		}

		if (aspect == null)
		{
			GD.PushWarning("[ScrapBox] Drop ignored: aspect couldn't be resolved.");
			return;
		}

		// If it's attached, detach first so tower stats stay clean
		tower ??= inv.AttachedTo(aspect);
		if (tower != null)
		{
			inv.DetachFrom(aspect, tower);
			tower.Recompute();
		}

		// Remove from inventory entirely (this is the "scrap" action)
		if (!inv.RemoveInstance(aspect))
		{
			GD.PushWarning("[ScrapBox] Failed to remove aspect from inventory.");
		}

		// Reward player: mana + scrap
		var template = aspect.Template;

		var manaReward  = template.ManaAmount;
		var scrapReward = template.ScrapAmount;

		gm.AddMana(manaReward);
		gm.AddScrap(scrapReward);

		// Refresh bar so the deleted token disappears
		AspectBar.Instance?.Refresh();

		// Refresh any open tower pullouts for that tower
		if (tower != null)
		{
			foreach (var n in GetTree().GetNodesInGroup("tower_pullout"))
			{
				if (n is UI_TowerPullout p && p.ActiveTower == tower)
					p.RefreshUIs();
			}
		}
	}
}
