using Godot;
using System;

public partial class GameManager : Node
{
	[Export] public NodePath WaveDirectorPath;
	[Export] public NodePath EnemiesRoot;
	[Export] public int StartingWaveSize = 3;

	//Singleton
	public static GameManager Instance;
	
	public AspectInventory Inventory { get; private set; }
	
	private WaveDirector _waveDirector;
	private int _currentWave = 0;
	private int _enemiesRemaining = 0;
	
	[Export] public NodePath gameOverTextPath;
	private Label _gameOverText;

	public override void _Ready()
	{
		//Singleton
		if(Instance != null)
		{
			QueueFree();
		}

		Instance = this;

		_gameOverText = GetNode<Label>(gameOverTextPath);
		_gameOverText.Visible = false;
		
		_waveDirector = GetNode<WaveDirector>(WaveDirectorPath);
		
		Inventory = new AspectInventory();
		_waveDirector.SetGameManager(this);

		StartNextWave();
	}

	private void StartNextWave()
	{
		_currentWave++;
		_enemiesRemaining = StartingWaveSize + (_currentWave * 2);
		_waveDirector.StartWave(_enemiesRemaining);

		GD.Print($"start wave {_currentWave} : {_enemiesRemaining} enemies");
	}

	public void OnEnemyDied(Enemy enemy)
	{
		_enemiesRemaining--;

		GD.Print($"enemy died, {_enemiesRemaining} left in wave");

		if (_enemiesRemaining <= 0)
		{
			GD.Print($"WAVE {_currentWave} CLEAR");
			StartNextWave();
		}
	}

	public void OnPlayerDefeated()
	{
		_gameOverText.Visible = true;
		GetTree().Paused = true;
	}
}
