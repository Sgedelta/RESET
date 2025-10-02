using Godot;

public partial class TowerSlot : Button
{
    [Signal] public delegate void PressedSlotEventHandler(int i);

    private int slotIndex;

    public override void _Ready()
    {
        //calc slot index
        CalculateSlotIndex();

        //recalc slot index
        if (GetParent() != null)
        {
            GetParent().ChildOrderChanged += CalculateSlotIndex;
        }

        Pressed += () => { EmitSignal("PressedSlot", slotIndex); };


    }

    private void CalculateSlotIndex()
    {
        slotIndex = GetIndex();
    }

}
