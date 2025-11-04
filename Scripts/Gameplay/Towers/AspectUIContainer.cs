using Godot;

public partial class AspectUIContainer : HFlowContainer
{
	[Export] private UI_TowerPullout _pullout;

	public override void _Ready()
	{
		// The container should NOT intercept mouse/drag at all
		MouseFilter = Control.MouseFilterEnum.Ignore;
	}

	/// <summary>
	/// Central helper used by slots to attach an aspect into targetIndex.
	/// Handles bar-origin and slot-origin payloads.
	/// </summary>
	public void AttachAspectToIndex(Godot.Collections.Dictionary data, int targetIndex)
	{
		var inv = GameManager.Instance.Inventory;

		// ---- Resolve aspect instance ----
		Aspect aspect = null;
		if (data.ContainsKey("aspect_id"))
			aspect = inv.GetByID((string)data["aspect_id"]);

		Tower sourceTower = null;
		int sourceIndex = -1;

		if ((string)data["origin"] == "slot")
		{
			var towerPath = (string)data["tower_path"];
			sourceIndex   = (int)data["slot_index"];
			sourceTower   = GetNode<Tower>(towerPath);
			if (aspect == null && sourceTower != null)
				aspect = sourceTower.GetAspectInSlot(sourceIndex);
		}

		if (aspect == null)
		{
			GD.PushWarning("[AspectUIContainer] Attach ignored: aspect was null.");
			return;
		}
		if (_pullout?.ActiveTower == null)
		{
			GD.PushWarning("[AspectUIContainer] ActiveTower is null.");
			return;
		}

		var destTower = _pullout.ActiveTower;

		// Detach from current owner if needed
		var currentOwner = inv.AttachedTo(aspect);
		if (currentOwner != null && currentOwner != destTower)
			inv.DetachFrom(aspect, currentOwner);

		// Old aspect in destination slot?
		var oldAspect = destTower.GetAspectInSlot(targetIndex);

		// Ensure destination is free
		inv.DetachFrom(targetIndex, destTower);

		// Attach new aspect
		if (!inv.AttachTo(aspect, destTower, targetIndex))
		{
			GD.PushWarning("[AspectUIContainer] AttachTo(dest) failed.");
			return;
		}

		// Put old aspect back where new one came from (slot-origin only)
		if (oldAspect != null && (string)data["origin"] == "slot" && sourceTower != null)
		{
			inv.AttachTo(oldAspect, sourceTower, sourceIndex);
		}

		destTower.Recompute();
		_pullout.RefreshUIs();
	}
}
