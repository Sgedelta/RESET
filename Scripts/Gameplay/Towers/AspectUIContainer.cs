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
           
        }

        //if there are no open slots, displace and replace into bar/inventory

    }
}
