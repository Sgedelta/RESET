using Godot;
using System;

public partial class HealthUi : Label
{
	[Export] public NodePath PlayerManagerPath;

    [Export] private NodePath damageOverlayPath;
    private DamageOverlay damageOverlay;

    private PlayerManager _playerManager;
    private bool _initialized = false;

    public override void _Ready()
	{
		_playerManager = GetNode<PlayerManager>(PlayerManagerPath);
		_playerManager.HealthChanged += OnHealthChanged;

        if (damageOverlayPath != null)
            damageOverlay = GetNode<DamageOverlay>(damageOverlayPath);
    }

	private void OnHealthChanged(int currentHealth, int maxHealth)
	{
		Text = $"HP: {(currentHealth > 0 ? currentHealth : 0)} / {maxHealth}";
        if (!_initialized)
        {
            _initialized = true;
            return;
        }
        damageOverlay?.Flash();

    }
}
