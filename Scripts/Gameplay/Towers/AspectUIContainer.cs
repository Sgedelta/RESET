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
        Tower sourceTower = null;
        if ((string)data["origin"] == "slot")
        {
            sourceTower = GetNode<Tower>((string)data["tower_path"]);
        }

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

        if((string)data["origin"] == "slot" && sourceTower == null)
        {
            GD.PushWarning($"Came from Slot but Source Tower ({(string)data["tower_path"]}) is null!");
            return;
        }

        GD.Print($"attaching to {targetIndex}");

        //grab aspect in slot (can be null)
        Aspect oldAspect = _pullout.ActiveTower.GetAspectInSlot(targetIndex);

        //detach our new aspect from wherever it is now
        if ((string)data["origin"] == "slot")
        {
            GameManager.Instance.Inventory.DetachFrom(aspect, sourceTower);
        }

        //place new aspect in slot (shouldn't be null)
        GameManager.Instance.Inventory.DetachFrom(targetIndex, _pullout.ActiveTower);
        GameManager.Instance.Inventory.AttachTo(aspect, _pullout.ActiveTower, targetIndex);

        //take oldAspect and put in newAspects past location
        if (oldAspect != null)
        {
            // for bar case, nothing needs to be done -
            // they are being recomputed in UI refresh and are already detached
            if ((string)data["origin"] == "slot")
            {
                GameManager.Instance.Inventory.AttachTo(
                    oldAspect, sourceTower, (int)data["slot_index"]);
            }
        }

        //refresh/recompute
        _pullout.RefreshUIs();

    }

    /// <summary>
    /// Attaches the given aspect data to this tower in the targetSlot from the tower within
    /// </summary>
    //public void AttachFromSlotToTargetSlot(Godot.Collections.Dictionary data,  int targetIndex)
    //{
    //    var srcTowerPath = (string)data["tower_path"];
    //    var srcIndex = (int)data["slot_index"];
    //    var srcTower = GetNode<Tower>(srcTowerPath);
    //
    //    //nothing needs to happen
    //    if(srcTower == _pullout.ActiveTower && srcIndex == targetIndex)
    //    {
    //        return;
    //    }
    //
    //    var srcAspect = srcTower.GetAspectInSlot(srcIndex);
    //    var dstAspect = _pullout.ActiveTower.GetAspectInSlot(targetIndex);
    //
    //    //case where we are transferring between the same towers
    //    if (srcTower == _pullout.ActiveTower)
    //    {
    //        _pullout.ActiveTower.SwapSlots(srcIndex, targetIndex);
    //        _pullout.ActiveTower.Recompute();
    //        _pullout.RefreshUIs();
    //        return;
    //    }
    //
    //    //case where we are swapping, but one half is null
    //    if(dstAspect == null)
    //    {
    //        if (GameManager.Instance.Inventory.DetachFrom(srcAspect, srcTower) &&
    //           GameManager.Instance.Inventory.AttachTo(srcAspect, _pullout.ActiveTower, targetIndex))
    //        {
    //            srcTower.Recompute();
    //            _pullout.ActiveTower.Recompute();
    //            _pullout.RefreshUIs();
    //        }
    //        return;
    //    }
    //
    //    //actually real swapping
    //    if(GameManager.Instance.Inventory.DetachFrom(srcAspect, srcTower) &&
    //        GameManager.Instance.Inventory.DetachFrom(dstAspect, _pullout.ActiveTower) &&
    //        GameManager.Instance.Inventory.AttachTo(srcAspect, _pullout.ActiveTower, targetIndex) &&
    //        GameManager.Instance.Inventory.AttachTo(dstAspect, srcTower, srcIndex)
    //
    //        )
    //    {
    //        srcTower.Recompute();
    //        _pullout.ActiveTower.Recompute();
    //        _pullout.RefreshUIs();
    //    }
    //
    //}
    
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

        AttachAspectToIndex(dict, targetIndex);

        //if(origin == "bar")
        //{
        //    string aspectID = (string)dict["aspect_id"];
        //    Aspect aspectInstance = GameManager.Instance.Inventory.GetByID(aspectID);
        //
        //
        //    if(GameManager.Instance.Inventory.AttachTo(aspectInstance, _pullout.ActiveTower, targetIndex))
        //    {
        //        _pullout.ActiveTower.Recompute();
        //        _pullout.RefreshUIs();
        //
        //    }
        //
        //} 
        //else if (origin == "slot")
        //{
        //    AttachAspectToIndex(dict, targetIndex);
        //}
    }
    
}
