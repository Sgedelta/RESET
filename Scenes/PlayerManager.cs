using Godot;
using System;

public partial class PlayerManager : Node
{
	[Signal] public delegate void HealthChangedEventHandler(int currentHealth, int maxHealth);

	[Export] public NodePath GameManagerPath;
	[Export] public NodePath EnemiesRootPath;
	[Export] public int MaxHealth = 100;   // player starting health

	private int _currentHealth;
	private GameManager _gameManager;
	private Node _enemiesRoot;

	public override void _Ready()
	{
		_currentHealth = MaxHealth;
		_gameManager = GetNode<GameManager>(GameManagerPath);
		_enemiesRoot = GetNode<Node>(EnemiesRootPath);

		EmitSignal(SignalName.HealthChanged, _currentHealth, MaxHealth);
		// TODO: initial signal doesn't fire. hardcoded in editor for now.

		foreach (var child in _enemiesRoot.GetChildren())
		{
			if (child is Enemy enemy)
			{
				enemy.EnemyAttacked += OnEnemyAttacked;
			}
		}

		_enemiesRoot.ChildEnteredTree += OnEnemySpawned;
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
		EmitSignal(SignalName.HealthChanged, _currentHealth, MaxHealth);

		if (_currentHealth <= 0)
		{
			_gameManager.OnPlayerDefeated();
		}
	}
}
