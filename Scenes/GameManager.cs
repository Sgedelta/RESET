using Godot;
using System;

public partial class GameManager : Node
{
	[Export] public NodePath WaveDirectorPath;
	[Export] public NodePath EnemiesRootPath;
	[Export] public int StartingWaveSize = 3;

	private WaveDirector _waveDirector;
	private Node2D _enemiesRoot;
	private int _currentWave = 0;

	public override void _Ready()
	{
		_waveDirector = GetNode<WaveDirector>(WaveDirectorPath);
		_enemiesRoot = GetNode<Node2D>(EnemiesRootPath);

		StartNextWave();
	}

	public override void _Process(double delta)
	{
		if (_waveDirector == null || _enemiesRoot == null) return;
		
		// no more enemies AND WaveDirector finished spawning
		if (_enemiesRoot.GetChildCount() == 0 && IsWaveFinished())
		{
			StartNextWave();
		}
	}

	private void StartNextWave()
	{
		_currentWave++;
		
		// basic wave size scaling
		int enemiesToSpawn = StartingWaveSize + (_currentWave * 2);
		_waveDirector.StartWave(enemiesToSpawn);

		GD.Print($"start wave {_currentWave} : {enemiesToSpawn} enemies");
	}

	private bool IsWaveFinished()
	{
		return _waveDirector.SpawnNumber <= 0 || 
			   _waveDirector.SpawnNumber == _waveDirector.GetSpawnedCount();
	}

	public void OnPlayerDefeated()
	{
		GD.Print("game over");
	}
}
