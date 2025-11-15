using Godot;
using System;
using System.Collections;
using System.Linq;

public partial class WaveGenerator : Node
{
	private static int generatedWaveCount = 0;

	private static WaveGenerator _instance;

	[Export] private Godot.Collections.Array<Godot.Collections.Array> enemyWeights;

	private Godot.Collections.Array<float> _weights;
	private Godot.Collections.Array<PackedScene> _enemies;

	private RandomNumberGenerator _rng;

	private bool _initialized = false;

	public WaveGenerator()
	{
		//singleton
		if (_instance != null && _instance != this)
		{
			QueueFree();
			return;
		}
		_instance = this;
	}

	public override void _Ready()
	{
		

		_weights = new Godot.Collections.Array<float>();
		_enemies = new Godot.Collections.Array<PackedScene>();

		foreach(Godot.Collections.Array pair in enemyWeights)
		{
			_weights.Add((float)pair[0]);
			_enemies.Add((PackedScene)pair[1]);
		}

		_rng = new RandomNumberGenerator();
		_initialized = true;
	}


	/// <summary>
	/// Generates and returns a new randomized Wave resource, with a difficulty relative to the basic difficulty
	/// </summary>
	/// <param name="relativeDifficulty"></param>
	/// <returns></returns>
	public static Wave GenerateWave(float relativeDifficulty)
	{
		if(!_instance._initialized)
		{
			GD.PushWarning("Skipping called wave generation because instance is not Initialized!");
			return null;
		}

		Wave wave = new Wave($"GeneratedWave_{generatedWaveCount}");
		generatedWaveCount++;


		//this is quick and dirty and should be improved with better generation!
		//i.e. a system that actually associates weights with difficulty, instead of treating them flat
		//or includes things like regions effecting them
		//but it's 2 am and I want this in
		// -sam

		//for now, generate 3 times the relativeDifficulty enemies
		for(int i = 0; i < relativeDifficulty * 3; i++)
		{
			Godot.Collections.Array enemyTime = new Godot.Collections.Array();
		   
			//get an enemy
			enemyTime.Add(_instance.GetRandomEnemy());
			//temp random timing thing? anywhere from 3 seconds to near instant, with higher difficulties being faster
			float delay = Mathf.Clamp(2f / (GD.Randf() * relativeDifficulty), 0.1f, 2f);

			enemyTime.Add(delay);

			wave.WaveInfo.Add(enemyTime);
		}

		return wave;
	}


	private PackedScene GetRandomEnemy()
	{

		int index = (int)_rng.RandWeighted(_weights.ToArray());

		GD.Print("Enemy Selected for Wave is: " + index);

		return _enemies[index];


	}

}
