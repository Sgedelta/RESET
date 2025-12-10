using Godot;

public partial class ScrapBox : PanelContainer
{
	// Reference to the always-present menu instance in the scene
	[Export] private ScrapConfirmMenu _scrapMenu;

	// Aspect we’re about to scrap (pending confirmation)
	private Aspect _pendingAspect;
	private Tower _pendingTower; // optional, may be null

	public override void _Ready()
	{
		if (_scrapMenu == null)
		{
			GD.PushWarning("[ScrapBox] _scrapMenu is not assigned in the inspector.");
			return;
		}

		// Ensure it's hidden initially and only shown when needed
		_scrapMenu.Visible = false;

		// Hook up signals once
		_scrapMenu.Confirmed += OnMenuConfirmed;
		_scrapMenu.Cancelled += OnMenuCancelled;
	}

	// ----------------- Drag & drop -----------------

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

		if (_scrapMenu == null)
		{
			GD.PushWarning("[ScrapBox] _scrapMenu is null, scrapping immediately.");
			_pendingAspect = aspect;
			_pendingTower  = tower;
			ConfirmScrap();
			ClearPending();
			return;
		}

		// Store pending data and show confirmation menu
		_pendingAspect = aspect;
		_pendingTower  = tower; // may be null, we'll resolve later if needed

		ShowScrapMenu();
	}

	// ----------------- Menu handling -----------------

	private void ShowScrapMenu()
	{
		if (_scrapMenu == null)
			return;

		_scrapMenu.Initialize(_pendingAspect);
		_scrapMenu.Visible = true;
	}

	private void HideScrapMenu()
	{
		if (_scrapMenu != null)
			_scrapMenu.Visible = false;
	}

	private void OnMenuConfirmed()
	{
		ConfirmScrap();
		ClearPending();
		HideScrapMenu();
	}

	private void OnMenuCancelled()
	{
		ClearPending();
		HideScrapMenu();
	}

	private void ClearPending()
	{
		_pendingAspect = null;
		_pendingTower  = null;
	}

	// ----------------- Actual scrapping logic -----------------

	private void ConfirmScrap()
	{
		if (_pendingAspect == null)
		{
			GD.PushWarning("[ScrapBox] ConfirmScrap called with no pending aspect.");
			return;
		}

		var gm  = GameManager.Instance;
		var inv = gm.Inventory;

		var aspect = _pendingAspect;

		// Resolve tower if we don't already have one
		var tower = _pendingTower ?? inv.AttachedTo(aspect);

		if (tower != null)
		{
			inv.DetachFrom(aspect, tower);
			tower.Recompute();
		}

		if (!inv.RemoveInstance(aspect))
		{
			GD.PushWarning("[ScrapBox] Failed to remove aspect from inventory.");
		}

		var template = aspect.Template;

		var manaReward  = template.ManaAmount;
		var scrapReward = template.ScrapAmount;

		gm.AddMana(manaReward);
		gm.AddScrap(scrapReward);

		AspectBar.Instance?.Refresh();

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
