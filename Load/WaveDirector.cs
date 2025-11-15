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
	
	public Path2D Path => _path2D;
	public Curve2D Curve => _path2D?.Curve;


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
			if (info == null || info.Count == 0)
			continue;
				
			//grab the enemy to spawn
			var enemyScene = info[0].As<PackedScene>();
			if (enemyScene == null)
				continue;

			float delay = 0.5f;
			if (info.Count > 1)
				delay = (float)info[1];
			//create it
			Enemy enemy = (Enemy)enemyScene.Instantiate();
			//put it into the tree
			GameManager.Instance.EnemiesRoot.AddChild(enemy);
			enemy.SetPathAndCurve(_path2D);

			//enemy.ModifyStats(info.HealthMultiplier, info.SpeedMultiplier);

			_activeEnemies.Add(enemy);

			if (_gameManager != null)
			{
				enemy.EnemyDied += _gameManager.OnEnemyDied;
			}

			await ToSignal(GetTree().CreateTimer(delay), "timeout");
		}

		_isSpawning = false;
	}
}
