using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class GameManager : Node
{
	[Export] public NodePath WaveDirectorPath;
	[Export] public Path2D EnemiesRoot;
	[Export(PropertyHint.Dir)] public string WaveFolderPath = "res://Resources/Waves";
	[Export] public NodePath AspectBarPath;	
	[Export] public NodePath gameOverTextPath;
	[Export] public NodePath RewardMenuPath;

	private AspectBar _aspectBar;
	private Label _gameOverText;
	private RewardMenu _rewardMenu; 
	private WaveDirector _waveDirector;

	// Singleton
	public static GameManager Instance;

	public AspectInventory Inventory { get; private set; }
	public WaveDirector WaveDirector => _waveDirector;

	private Dictionary<string, Wave> _waveLibrary = new();
	private int _currentWave = 0;
	private int _enemiesRemaining = 0;
	private float _duration = 0;

	private HashSet<Aspect> _lastOffered = new();

	public override void _Ready()
	{
		//Singleton
		if(Instance != null)
		{
			QueueFree();
			return;
		}

		Instance = this;

		_gameOverText = GetNode<Label>(gameOverTextPath);
		_gameOverText.Visible = false;

		_waveDirector = GetNode<WaveDirector>(WaveDirectorPath);
		Inventory = new AspectInventory();
		_waveDirector.SetGameManager(this);

		_aspectBar = GetNodeOrNull<AspectBar>(AspectBarPath);

		_rewardMenu = GetNodeOrNull<RewardMenu>(RewardMenuPath);
		if (_rewardMenu != null)
		{
				_rewardMenu.ProcessMode = Node.ProcessModeEnum.WhenPaused; // Godot 4
				_rewardMenu.Hide();
				_rewardMenu.ChoicePicked += OnAspectTemplatePicked;  // <-- SUBSCRIBE!
				GD.Print($"[GM] Subscribed to RewardMenu at {_rewardMenu.GetPath()}");
		}

		LoadAllWaves();
		StartNextWave();
	}

	private void LoadAllWaves()
	{
		_waveLibrary.Clear();

		var dir = DirAccess.Open(WaveFolderPath);
		if (dir == null)
		{
			GD.PrintErr($"[GM] Failed to open wave folder: {WaveFolderPath}");
			return;
		}

		dir.ListDirBegin();
		string fileName = dir.GetNext();
		while (!string.IsNullOrEmpty(fileName))
		{
			if (!dir.CurrentIsDir() && fileName.EndsWith(".tres"))
			{
				string filePath = $"{WaveFolderPath}/{fileName}";
				var wave = ResourceLoader.Load<Wave>(filePath);
				if (wave != null)
				{
					string key = wave.ID;
					_waveLibrary[key] = wave;
					GD.Print($"[GM] Loaded wave: {key}");
				}
				else
				{
					GD.PrintErr($"[GM] Failed to load wave {filePath}");
				}
			}
			fileName = dir.GetNext();
		}

		dir.ListDirEnd();

		GD.Print($"[GM] Loaded {_waveLibrary.Count} waves");
	}

	private void StartNextWave()
	{
		_currentWave++;

		if (_waveLibrary.Count == 0)
		{
			GD.PrintErr("[GM] No more waves");
			return;
		}

		Wave wave = GetWave();
		if (wave == null)
		{
			GD.PrintErr("[GM] Failed to select a wave");
			return;
		}

		GD.Print($"[GM] Starting wave {_currentWave}: {wave.ID}");
		_waveDirector.StartWave(wave);
		_enemiesRemaining = wave.WaveInfo.Count;
	}

	/// <summary>
	/// returns a random Wave from the loaded wave library
	/// </summary>
	public Wave GetWave()
	{
		if (_waveLibrary.Count == 0)
			return null;

		var targetDifficulty = GetTargetDifficulty();

		// filter library to difficulty
		var candidates = _waveLibrary.Values
			.Where(w => w.WaveDifficulty == targetDifficulty)
			.ToList();

		// no matches, fall back to anything
		if (candidates.Count == 0)
		{
			GD.PrintErr($"[GM] No waves with difficulty {targetDifficulty}");
			candidates = _waveLibrary.Values.ToList();
		}

		float totalWeight = 0f;
		foreach (var wave in candidates)
			totalWeight += Mathf.Max(wave.SelectionWeight, 0f);

		if (totalWeight <= 0f)
			return candidates.Last(); // fallback

		var rng = new Random();
		float choice = (float)(rng.NextDouble() * totalWeight);

		foreach (var wave in candidates)
		{
			choice -= Mathf.Max(wave.SelectionWeight, 0f);
			if (choice <= 0)
				return wave;
		}

		return candidates.Last();
	}


	// helper function -- placeholder, will later be based on map graph
	private Wave.Difficulty GetTargetDifficulty()
	{
		if (_currentWave < 3)
			return Wave.Difficulty.Easy;
		else if (_currentWave < 6)
			return Wave.Difficulty.Medium;
		else
			return Wave.Difficulty.Hard;
	}


	public void OnEnemyDied(Enemy enemy)
	{
		_enemiesRemaining--;

		if (_enemiesRemaining <= 0)
		{
			GD.Print($"WAVE {_currentWave} CLEAR");
			OfferEndOfRoundRewards();
		}

		_waveDirector.RemoveActiveEnemy(enemy);
	}

	public void OnPlayerDefeated()
	{
		_gameOverText.Visible = true;
		GetTree().Paused = true;
	}

	public Enemy GetNearestEnemyToPoint(Vector2 point, List<Enemy> exclude)
	{
		if (_enemiesRemaining <= 0) return null;

		var filterEnemies = _waveDirector.ActiveEnemies.Where(e => !exclude.Contains(e));

		float closestSqDist = float.MaxValue;
		Enemy nearest = null;

		foreach (Enemy e in filterEnemies)
		{
			float dist = e.GlobalPosition.DistanceSquaredTo(point);
			if (dist < closestSqDist)
			{
				nearest = e;
				closestSqDist = dist;
			}
		}
		return nearest;
	}

	public Enemy GetNearestEnemyToPoint(Vector2 point)
	{
		return GetNearestEnemyToPoint(point, new List<Enemy>());	
	}

	private void OfferEndOfRoundRewards()
	{
		if (_rewardMenu == null)
		{
			GD.PrintErr("[GM] RewardMenuPath not set");
			StartNextWave();
			return;
		}

		var choices = AspectLibrary.RollTemplates(3, _ => true);
		if (choices == null || choices.Count == 0)
		{
			StartNextWave();
			return;
		}

		_rewardMenu.ShowChoices(_currentWave, choices);
		GetTree().Paused = true;
	}

	private void OnAspectTemplatePicked(AspectTemplate pickedTemplate)
	{
		GD.Print($"[GM] Picked {pickedTemplate?._id}");

		var instance = Inventory.AcquireFromTemplate(pickedTemplate);
		GD.Print($"[GM] Inventory now has {Inventory.BagAspects().Count()} aspects total");

		_aspectBar?.Refresh(); 

		_rewardMenu.Hide();
		GetTree().Paused = false;
		StartNextWave();
	}
}
