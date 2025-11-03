using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class WaveDirector : Node2D
{
	[Export] public NodePath Path2DPath;
	[Export] public Wave CurrentWave;
	private Path2D _path2D;

	private List<Enemy> _activeEnemies = new();
	private GameManager _gameManager;
	private bool _isSpawning = false;

	public List<Enemy> ActiveEnemies => _activeEnemies;

	public override void _Ready()
	{
		_path2D = GetNode<Path2D>(Path2DPath);
	}

	public void SetGameManager(GameManager manager)
	{
		_gameManager = manager;
	}

	public void RemoveActiveEnemy(Enemy enemy)
	{
		_activeEnemies.Remove(enemy);
	}

	public async void StartWave(Wave wave)
	{
		if (wave == null || _isSpawning)
			return;

		CurrentWave = wave;
		_isSpawning = true;

		foreach (var info in wave.WaveInfo)
		{
			if ((bool)info[0] == false)
				continue;
				
			var enemy = (Enemy)info[0];
			GameManager.Instance.EnemiesRoot.AddChild(enemy);
			enemy.SetPathAndCurve(_path2D);

			//enemy.ModifyStats(info.HealthMultiplier, info.SpeedMultiplier);

			_activeEnemies.Add(enemy);

			if (_gameManager != null)
			{
				enemy.EnemyDied += _gameManager.OnEnemyDied;
			}

			await ToSignal(GetTree().CreateTimer((float)info[1]), "timeout");
		}

		_isSpawning = false;
	}
}
