using Godot;

public abstract partial class AbilityBase : Resource
{
	[Export] public Texture2D Icon;
	[Export] public string AbilityName = "Ability";
	[Export] public float CooldownSeconds;
	[Export] public int MaxCharges = 0;
	
	public float CurrentCooldown { get; private set; } = 0f;
	public bool IsOnCooldown => CurrentCooldown > 0f;
	public void TriggerCooldown()
	{
		CurrentCooldown = CooldownSeconds;
	}
	public void TickCooldown(float delta)
	{
		if (CurrentCooldown <= 0f) return;
		CurrentCooldown -= delta;
		if (CurrentCooldown < 0f) CurrentCooldown = 0f;
	}

	public abstract void Execute(Vector2 worldPos);
}
