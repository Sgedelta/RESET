using Godot;
using System;

public partial class WaveDirector : Node2D
{
	[Export] public PackedScene EnemyScene;
	[Export] public NodePath MapEnemiesPath;
	[Export] public NodePath Path2DPath;
	[Export] public float SpawnEvery = 1.0f;    // Seconds between spawns

	private float _timer;
	private Node2D _enemiesRoot;
	private Path2D _path2D;
	
	public override void _Ready()
	{
	   _enemiesRoot = GetNode<Node2D>(MapEnemiesPath);
	   _path2D = GetNode<Path2D>(Path2DPath);
	}

	public override void _Process(double delta)
	{
		_timer += (float)delta;
		if (_timer < SpawnEvery) return;
		_timer = 0f;

		if (EnemyScene == null) return;

		// Spawn enemy on the Enemies node
		 var enemy = (Enemy)EnemyScene.Instantiate();
		_enemiesRoot.AddChild(enemy);

		enemy.SetPath(_path2D);

	}
}
