using Godot;
using System;

public partial class GameManager : Node
{
	[Export] public NodePath WaveDirectorPath;
	[Export] public int StartingWaveSize = 3;

	private WaveDirector _waveDirector;
	private int _currentWave = 0;
	private int _enemiesRemaining = 0;

	public override void _Ready()
	{
		_waveDirector = GetNode<WaveDirector>(WaveDirectorPath);
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
		GD.Print("GAME OVER");
	}
}
