using Godot;
using System;
using System.Data;
using System.Collections.Generic;

public class AspectInventory
{
	// What tower a specific Aspect instance is attached to
	private readonly Dictionary<Aspect, Tower> _owner = new();
	
	// All aspects the player has
	private readonly List<Aspect> _bag = new();

	public IEnumerable<Aspect> BagAspects() => _bag;

	public bool Acquire(Aspect a)
	{
		if (a == null) return false;
		_bag.Add(a);
		return true;
	}

	public Aspect AcquireFromTemplate(AspectTemplate t)
	{
		if (t == null) return null;
		var instance = new Aspect(t);
		_bag.Add(instance);
		GD.Print($"[Inventory] +1 {t._id} (total now {_bag.Count})");
		return instance;
	}

	public Aspect GetByID(string ID)
	{
		foreach(Aspect a in _bag)
		{
			if (a.ID == ID) return a;
		}
		return null;
	}


	public bool RemoveInstance(Aspect a) => _bag.Remove(a);

	public int Count(AspectTemplate t) => t == null ? 0 : _bag.FindAll(x => x.Template == t).Count;
	public bool HasAny(AspectTemplate t) => Count(t) > 0;

	public bool IsAttached(Aspect a) => a != null && _owner.ContainsKey(a);
	public Tower AttachedTo(Aspect a) => _owner.TryGetValue(a, out var t) ? t : null;

	public bool AttachTo(Aspect a, Tower t, int slotIndex = -1)
	{
		if (a == null || t == null) return false;
		if (!_bag.Contains(a)) return false;
		if (IsAttached(a) && AttachedTo(a) != t) return false;

		bool ok = t.AttachAspect(a, slotIndex);
		if (ok)
		{
			_owner[a] = t;
			
		}

		return ok;
	}

	public bool DetachFrom(Aspect a, Tower t)
	{
		if(a == null || t == null) return false;
		if (!_owner.TryGetValue(a, out var current) || current != t) return false;
		bool ok = t.DetachAspect(a);
		if (ok) _owner.Remove(a);
		return ok;
	}

	public bool DetachFrom(int index, Tower t)
	{
		Aspect a = t.GetAspectInSlot(index);
		return DetachFrom(a, t);
    }

    public bool Move(Aspect a, Tower toTower, int slotIndex = -1) => AttachTo(a, toTower, slotIndex);
}
