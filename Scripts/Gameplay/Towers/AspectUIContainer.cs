using Godot;
using System;

public partial class AspectUIContainer : HFlowContainer
{

    [Export] private UI_TowerPullout _pullout;

    public void AttachFromBarToTargetSlot(Godot.Collections.Dictionary data, int targetIndex)
    {
        var aspectId = (string)data["aspect_id"];
        var aspect = AspectLibrary.GetById(aspectId); //TODO: Replace with new Aspect Management System Eventually

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

        //attempt to attach in earliest slot
        int emptySlot = _pullout.ActiveTower.FirstEmptySlotIndex();
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
        }

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


    
}
