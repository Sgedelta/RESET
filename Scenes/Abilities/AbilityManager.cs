using Godot;

public partial class AbilityManager : Node
{
	[Signal] public delegate void AbilitySelectedEventHandler(AbilityBase ability);
	[Signal] public delegate void AbilityPlacedEventHandler(AbilityBase ability, Vector2 pos);

	public static AbilityManager Instance { get; private set; }

	public AbilityBase ArmedAbility { get; private set; }

	public override void _EnterTree() => Instance = this;

	public void Arm(AbilityBase ability)
	{
		if (ArmedAbility == ability) return;
		ArmedAbility = ability;
		EmitSignal(SignalName.AbilitySelected, ability);
	}

	public void Disarm()
	{
		if (ArmedAbility == null) return;
		ArmedAbility = null;
		EmitSignal(SignalName.AbilitySelected, (AbilityBase)null);
	}

	public void PlaceAt(Vector2 worldPos)
	{
		if (ArmedAbility == null) return;
		ArmedAbility.Execute(worldPos);
		EmitSignal(SignalName.AbilityPlaced, ArmedAbility, worldPos);
		Disarm();
	}
}
