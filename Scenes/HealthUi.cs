using Godot;
using System;

public partial class HealthUi : Label
{
	[Export] public NodePath PlayerManagerPath;

	private PlayerManager _playerManager;

	public override void _Ready()
	{
		_playerManager = GetNode<PlayerManager>(PlayerManagerPath);
		_playerManager.HealthChanged += OnHealthChanged;
	}

	private void OnHealthChanged(int currentHealth, int maxHealth)
	{
		Text = $"HP: {(currentHealth > 0 ? currentHealth : 0)} / {maxHealth}";
	}
}
