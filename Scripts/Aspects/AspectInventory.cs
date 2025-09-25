using Godot;
using System;
using System.Data;
using System.Collections.Generic;

public class AspectInventory
{
	// Who currently owns each aspect instance?
	private readonly Dictionary<Aspect, Tower> _owner = new();
	public bool IsOwned(Aspect a) => _owner.ContainsKey(a);
	public Tower OwnerOf(Aspect a) => _owner.TryGetValue(a, out var t) ? t : null;

	public bool AttachTo(Aspect a, Tower t, int slotIndex = -1)
	{
		if (_owner.TryGetValue(a, out var current) && current != null)
		{
			if (current == t) return false;
			current.DetachAspect(a);
		}

		bool ok = t.AttachAspect(a, slotIndex);
		if (ok) _owner[a] = t;
		return ok;
	}

	public bool DetachFrom(Aspect a, Tower t)
	{
		if (!_owner.TryGetValue(a, out var current) || current != t) return false;
		bool ok = t.DetachAspect(a);
		if (ok) _owner.Remove(a);
		return ok;
	}

	public bool Move(Aspect a, Tower toTower, int slotIndex = -1) => AttachTo(a, toTower, slotIndex);
}
