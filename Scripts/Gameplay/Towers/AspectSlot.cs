using Godot;
using System;

public partial class AspectSlot : PanelContainer
{

    [Export] public int Index;

    private UI_TowerPullout _pullout;

    public Label Label;


    public override void _Ready()
    {
        Index = GetIndex();
        _pullout = GetParent<UI_TowerPullout>();
        Label = GetChild<Label>(0);


        base._Ready();
    }

    public override Variant _GetDragData(Vector2 atPosition)
    {
        var aspect = _pullout.ActiveTower.GetAspectInSlot(Index);

        if (aspect == null)
        {
            return default;
        }

        if (GetChildCount() > 0)
        {
            var preview = (Control)Duplicate();
            SetDragPreview(preview);
        }

        return new Godot.Collections.Dictionary
        {
            { "type","aspect_token" }, { "origin","slot" },
            { "tower_path", _pullout.ActiveTower.GetPath().ToString() },
            { "slot_index", Index },
            { "aspect_id", aspect.Template._id }
        };
    }

    public override bool _CanDropData(Vector2 atPosition, Variant data)
    {
        if(data.VariantType != Variant.Type.Dictionary)
        {
            return false;
        }

        var dict = (Godot.Collections.Dictionary)data;

        if(!dict.TryGetValue("type", out var t) || (string)t != "aspect_token")
        {
            return false;
        }

        return true;
    }

    public override void _DropData(Vector2 atPosition, Variant data)
    {
        var dict = (Godot.Collections.Dictionary)data;
        var origin = (string)dict["origin"];

        if(origin == "bar")
        {
            _pullout.Container.AttachFromBarToTargetSlot(dict, Index);
        } else if (origin == "slot")
        {
            _pullout.Container.AttachFromSlotToTargetSlot(dict, Index);
        }
    }


}
