using Godot;
using System;

public partial class PlayerManager : Node
{
	[Signal] public delegate void HealthChangedEventHandler(int currentHealth, int maxHealth);

	[Export] public NodePath GameManagerPath;
	[Export] public NodePath EnemiesRootPath;

	[Export] public int MaxHealth = 100;   // player starting health

	// UI references
	[Export] public TextureProgressBar HealthBar;
	[Export] public Label HealthLabel; // optional, can be left null if you don't want text

	private int _currentHealth;
	private GameManager _gameManager;
	private Node _enemiesRoot;

	public override void _Ready()
	{
		_currentHealth = MaxHealth;

		_gameManager = GetNode<GameManager>(GameManagerPath);
		_enemiesRoot = GetNode<Node>(EnemiesRootPath);

		// Initialize UI immediately
		InitHealthUI();
		UpdateHealthUI();

		// Emit initial signal for anything else listening
		EmitSignal(SignalName.HealthChanged, _currentHealth, MaxHealth);

		foreach (var child in _enemiesRoot.GetChildren())
		{
			if (child is Enemy enemy)
			{
				enemy.EnemyAttacked += OnEnemyAttacked;
			}
		}

		_enemiesRoot.ChildEnteredTree += OnEnemySpawned;
	}

	private void InitHealthUI()
	{
		if (HealthBar != null)
		{
			HealthBar.MinValue = 0;
			HealthBar.MaxValue = MaxHealth;
		}
	}

	private void UpdateHealthUI()
	{
		if (HealthBar != null)
		{
			HealthBar.Value = _currentHealth;
		}

		if (HealthLabel != null)
		{
			// If you want "HP: 50/100" instead of just "50", tweak this line
			HealthLabel.Text = $"{_currentHealth}";
		}
	}

	private void OnEnemySpawned(Node node)
	{
		if (node is Enemy enemy)
		{
			enemy.EnemyAttacked += OnEnemyAttacked;
		}
	}

	private void OnEnemyAttacked(Enemy enemy, float damage)
	{
		_currentHealth -= (int)damage;
		if (_currentHealth < 0)
			_currentHealth = 0;

		// Update UI first
		UpdateHealthUI();

		// Then notify any listeners
		EmitSignal(SignalName.HealthChanged, _currentHealth, MaxHealth);

		if (_currentHealth <= 0)
		{
			_gameManager.OnPlayerDefeated();
		}
	}

	// If you ever want to heal the player:
	public void Heal(int amount)
	{
		if (amount <= 0) return;

		_currentHealth += amount;
		if (_currentHealth > MaxHealth)
			_currentHealth = MaxHealth;

		UpdateHealthUI();
		EmitSignal(SignalName.HealthChanged, _currentHealth, MaxHealth);
	}
}
