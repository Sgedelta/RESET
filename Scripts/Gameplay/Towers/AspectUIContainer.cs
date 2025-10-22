using Godot;
using System;

public partial class AspectUIContainer : HFlowContainer
{

    [Export] private UI_TowerPullout _pullout;

    /// <summary>
    /// Takes a Data Dictionary (use _DropData format) and an index to 
    /// get an aspect and put it into the given slot of the ActiveTower.
    /// If there was an aspect there, places it wherever this aspect comes from.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="targetIndex"></param>
    public void AttachAspectToIndex(Godot.Collections.Dictionary data, int targetIndex)
    {
        //ensure data is good
        Aspect aspect = GameManager.Instance.Inventory.GetByID((string)data["aspect_id"]);

        if (aspect == null)
        {
            GD.PushWarning("Found Aspect was null!");
            return;
        }

        if (_pullout == null)
        {
            GD.PushWarning("Pullout UI is Null when attaching aspect!");
            return;
        }

        if (_pullout.ActiveTower == null)
        {
            GD.PushWarning("Active Tower for UI is null!");
            return;
        }


        //grab aspect in slot (can be null)

        Aspect oldAspect = _pullout.ActiveTower.GetAspectInSlot(targetIndex);

        //place new aspect in slot (shouldn't be null)

        if(_pullout.ActiveTower.DetachAspect(targetIndex))
        {
            _pullout.ActiveTower.AttachAspect(aspect, targetIndex);
        }
        //take oldAspect and put in newAspects past location

        if(oldAspect != null)
        {
            switch((string)data["origin"])
            {
                case "bar":

                    break;
            }
        }

        //refresh/recompute
        _pullout.RefreshUIs();

    }

    public void AttachFromBarToTargetSlot(Godot.Collections.Dictionary data, int targetIndex)
    {
        //setup and ensuring our data is valid
        var aspectId = (string)data["aspect_id"];
        var aspect = GameManager.Instance.Inventory.GetByID(aspectId); 

        if (aspect == null)
        {
            GD.PushWarning("Found Aspect was null!");
            return;
        }

        if (_pullout == null)
        {
            GD.PushWarning("Pullout UI is Null when attaching aspect!");
            return;
        }

        if(_pullout.ActiveTower == null)
        {
            GD.PushWarning("Active Tower for UI is null!");
            return;
        }

        //attempt to attach in earliest slot if there are empty slots
        // TODO: Consider if this is needed.
        /*int emptySlot = _pullout.ActiveTower.FirstEmptySlotIndex();
        if(emptySlot >= 0)
        {
            if (GameManager.Instance.Inventory.AttachTo(aspect, _pullout.ActiveTower, emptySlot))
            {
                _pullout.ActiveTower.Recompute();
                _pullout.RefreshUIs();
            }
            return;
        }

        //if there are no open slots, displace and replace into bar/inventory
        var displacedAspect = _pullout.ActiveTower.GetAspectInSlot(targetIndex);
        if (displacedAspect == null)
        {
            //if we're secretly not displacing an aspect (somehow), just behave as before
            if (GameManager.Instance.Inventory.AttachTo(aspect, _pullout.ActiveTower, targetIndex))
            {
                _pullout.ActiveTower.Recompute();
                _pullout.RefreshUIs();
            }
            return;

        } else if(GameManager.Instance.Inventory.DetachFrom(displacedAspect, _pullout.ActiveTower) &&
            GameManager.Instance.Inventory.AttachTo(aspect, _pullout.ActiveTower, targetIndex))
        {
            _pullout.ActiveTower.Recompute();
            _pullout.RefreshUIs();
        }*/


    }

    /// <summary>
    /// Attaches the given aspect data to this tower in the targetSlot from the tower within
    /// </summary>
    public void AttachFromSlotToTargetSlot(Godot.Collections.Dictionary data,  int targetIndex)
    {
        var srcTowerPath = (string)data["tower_path"];
        var srcIndex = (int)data["slot_index"];
        var srcTower = GetNode<Tower>(srcTowerPath);

        //nothing needs to happen
        if(srcTower == _pullout.ActiveTower && srcIndex == targetIndex)
        {
            return;
        }

        var srcAspect = srcTower.GetAspectInSlot(srcIndex);
        var dstAspect = _pullout.ActiveTower.GetAspectInSlot(targetIndex);

        //case where we are transferring between the same towers
        if (srcTower == _pullout.ActiveTower)
        {
            _pullout.ActiveTower.SwapSlots(srcIndex, targetIndex);
            _pullout.ActiveTower.Recompute();
            _pullout.RefreshUIs();
            return;
        }

        //case where we are swapping, but one half is null
        if(dstAspect == null)
        {
            if (GameManager.Instance.Inventory.DetachFrom(srcAspect, srcTower) &&
               GameManager.Instance.Inventory.AttachTo(srcAspect, _pullout.ActiveTower, targetIndex))
            {
                srcTower.Recompute();
                _pullout.ActiveTower.Recompute();
                _pullout.RefreshUIs();
            }
            return;
        }

        //actually real swapping
        if(GameManager.Instance.Inventory.DetachFrom(srcAspect, srcTower) &&
            GameManager.Instance.Inventory.DetachFrom(dstAspect, _pullout.ActiveTower) &&
            GameManager.Instance.Inventory.AttachTo(srcAspect, _pullout.ActiveTower, targetIndex) &&
            GameManager.Instance.Inventory.AttachTo(dstAspect, srcTower, srcIndex)

            )
        {
            srcTower.Recompute();
            _pullout.ActiveTower.Recompute();
            _pullout.RefreshUIs();
        }

    }
    
    /// <summary>
    /// Returns the slot index if the localPos is over one of the active slots. otherwise, returns the first empty slot
    /// </summary>
    /// <param name="localPos"></param>
    /// <returns></returns>
    private int GetSlotIndexFromPosition(Vector2 localPos)
    {
        for(int i = 0; i < _pullout.AvailableSlots; i++)
        {
            if(GetChild(i) is AspectSlot slot)
            {
                var rect = new Rect2(slot.Position, slot.Size);
                if(rect.HasPoint(localPos))
                {
                    return i;
                }
            }
        }

        return _pullout.ActiveTower.LowestOpenSlot;
    }

    public override bool _CanDropData(Vector2 atPosition, Variant data)
    {
        if (data.VariantType != Variant.Type.Dictionary) return false;
        var dict = (Godot.Collections.Dictionary)data;

        if(!dict.TryGetValue("type", out var t) || (string)t != "aspect_token")
        {
            return false;
        }

        if(dict.TryGetValue("origin", out var o))
        {
            string origin = (string)o;
            if(origin == "bar")
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

        //determine slot
        int targetIndex = dict.ContainsKey("slot_index")
            ? (int)dict["slot_index"]                // some UIs pass it along
            : GetSlotIndexFromPosition(atPosition);  // otherwise calculate from mouse

        if(origin == "bar")
        {
            string aspectID = (string)dict["aspect_id"];
            Aspect aspectInstance = GameManager.Instance.Inventory.GetByID(aspectID);


            if(GameManager.Instance.Inventory.AttachTo(aspectInstance, _pullout.ActiveTower, targetIndex))
            {
                _pullout.ActiveTower.Recompute();
                _pullout.RefreshUIs();

            }

        } 
        else if (origin == "slot")
        {
            AttachFromSlotToTargetSlot(dict, targetIndex);
        }
    }
    
}
