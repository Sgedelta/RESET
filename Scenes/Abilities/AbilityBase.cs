using Godot;

public abstract partial class AbilityBase : Resource
{
	[Export] public Texture2D LockedIcon1;
	[Export] public Texture2D LockedIcon2;
	[Export] public Texture2D Icon1;
	[Export] public Texture2D Icon2;
	[Export] public Texture2D SelectedIcon;
	
	[Export] public string AbilityName = "Ability";

	[Export] public int BaseManaCost = 10;

	public virtual int ManaCost => CurrentLevel <= 0 ? 0 : BaseManaCost * CurrentLevel;

	[Export] public int CurrentLevel { get; protected set; } = 0;
	[Export] public int MaxLevel    { get; protected set; } = 10;

	[Export] public int ScrapUnlockCost = 250;

	[Export] public Texture2D HoverBackground;


	public bool IsUnlocked => CurrentLevel > 0;

	public bool TryUnlock(GameManager gm)
	{
		if (gm == null) return false;
		if (IsUnlocked) return true;

		if (!gm.TrySpendScrap(ScrapUnlockCost))
			return false;

		CurrentLevel = 1;
		ApplyUpgradeEffects();
		return true;
	}

	public virtual bool CanUpgrade(GameManager gm)
	{
		if (gm == null) return false;
		if (!IsUnlocked) return false;
		return CurrentLevel < MaxLevel;
	}

	public virtual int GetUpgradeCost(int targetLevel)
	{
		return ScrapUnlockCost * targetLevel;
	}

	protected virtual void ApplyUpgradeEffects()
	{
		// Override per ability to scale damage, radius, etc
	}

	public bool TryUpgrade(GameManager gm)
	{
		if (gm == null) return false;
		if (!IsUnlocked) return false;
		if (CurrentLevel >= MaxLevel) return false;

		int targetLevel = CurrentLevel + 1;
		int cost = GetUpgradeCost(targetLevel);

		if (!gm.TrySpendScrap(cost))
			return false;

		CurrentLevel = targetLevel;
		ApplyUpgradeEffects();
		return true;
	}

	public abstract void Execute(Vector2 worldPos);
}
