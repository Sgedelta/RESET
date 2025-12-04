using Godot;

public abstract partial class AbilityBase : Resource
{
	[Export] public Texture2D Icon;
	[Export] public string AbilityName = "Ability";
	[Export] public int BaseManaCost = 10;
	public virtual int ManaCost => BaseManaCost;

	[Export] public int CurrentLevel { get; protected set; } = 1;
	[Export] public int MaxLevel    { get; protected set; } = 10;

	[Export] public Texture2D HoverBackground;

	public virtual bool CanUpgrade(GameManager gm)
	{
		if (gm == null) return false;
		return CurrentLevel < MaxLevel;
	}

	public virtual int GetUpgradeCost(int targetLevel)
	{
		return 10 * targetLevel;
	}

	protected virtual void ApplyUpgradeEffects()
	{
		// Override per ability to scale damage, radius, etc
	}

	public bool TryUpgrade(GameManager gm)
	{
		if (gm == null) return false;
		if (CurrentLevel >= MaxLevel) return false;

		int targetLevel = CurrentLevel + 1;
		int cost = GetUpgradeCost(targetLevel);

		if (!gm.TrySpendMana(cost))
			return false;

		CurrentLevel = targetLevel;
		ApplyUpgradeEffects();
		return true;
	}

	public abstract void Execute(Vector2 worldPos);
}
