using Godot;

public partial class TowerSlot : Button
{
	[Signal] public delegate void PressedSlotEventHandler(int i);

	private int slotIndex;

	public override void _Ready()
	{
		base._Ready();

		//calc slot index
		CalculateSlotIndex();

		//recalc slot index
		if (GetParent() != null)
		{
			GetParent().ChildOrderChanged += CalculateSlotIndex;
		}

		//Pressed += () => {GD.Print("Pressed, Re-emitting"); EmitSignal("PressedSlot", slotIndex); };

		this.Connect("pressed", Callable.From(() => { GD.Print("Pressed, Re-emitting"); EmitSignal("PressedSlot", slotIndex); }));

	}

	public void DebugPrint()
	{
		GD.Print("Pressed");
	}

	private void CalculateSlotIndex()
	{
		slotIndex = GetIndex();
	}

}
