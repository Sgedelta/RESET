using Godot;

public abstract partial class AbilityBase : Resource
{
	[Export] public Texture2D Icon;
	[Export] public string AbilityName = "Ability";
	[Export] public int ManaCost = 10;

	public abstract void Execute(Vector2 worldPos);
}
