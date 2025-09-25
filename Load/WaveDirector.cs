using Godot;
using System;

public partial class WaveDirector : Node2D
{
	[Export] public PackedScene EnemyScene;
	[Export] public NodePath MapEnemiesPath;
	[Export] public NodePath Path2DPath;
	[Export] public float SpawnEvery = 1.0f;
	[Export] public int SpawnNumber = 5;
	
	private float _timer;
	private int _count;
	public int GetSpawnedCount() => _count;
	
	private Node2D _enemiesRoot;
	private Path2D _path2D;
	
	private GameManager _gameManager;
	
	public override void _Ready()
	{
	   _enemiesRoot = GetNode<Node2D>(MapEnemiesPath);
	   _path2D = GetNode<Path2D>(Path2DPath);
	   _count = 0;
	}

	public override void _Process(double delta)
	{
		_timer += (float)delta;
		if (_timer < SpawnEvery) return;
		_timer = 0f;

		if (EnemyScene == null) return;
		if (_count >= SpawnNumber) return;

		// Spawn enemy on the Enemies node
		 var enemy = (Enemy)EnemyScene.Instantiate();
		_enemiesRoot.AddChild(enemy);

		enemy.SetPath(_path2D);
		
		if (_gameManager != null)
		{
			enemy.EnemyDied += _gameManager.OnEnemyDied;
		}
		
		_count += 1;
	}
	
	public void StartWave(int enemyCount)
	{
		SpawnNumber = enemyCount;
		_count = 0;
		_timer = 0f;
	}
	
	public void SetGameManager(GameManager manager)
	{
		_gameManager = manager;
	}
}
