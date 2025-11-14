using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class GameManager : Node
{
	[Export] public NodePath WaveDirectorPath;
	[Export] public Path2D EnemiesRoot;
	[Export] public string StartScreenPath;
	
	// Folder containing Wave resource files
	[Export(PropertyHint.Dir)] public string WaveFolderPath = "res://Resources/Waves";
	[Export] public NodePath AspectBarPath;	
	[Export] public NodePath gameOverTextPath;
	[Export] public NodePath RewardMenuPath;


	private AspectBar _aspectBar;
	private Label _gameOverText;
	private RewardMenu _rewardMenu; 
	private WaveDirector _waveDirector;
	
	public int Mana { get; private set; } = 0;
	[Export] public Label ManaLabel;

	// Singleton
	public static GameManager Instance;

	public AspectInventory Inventory { get; private set; }
	public WaveDirector WaveDirector => _waveDirector;

	private Godot.Collections.Dictionary<string, Wave> _waveLibrary;
	private int _currentWave = 0;
	private int _enemiesRemaining = 0;
	private float _duration = 0;
	
	[Export] public NodePath PauseMenuPath;
	private PauseMenu _pauseMenu;
	[Export] public Button PauseButton;
	[Export] public Button ResumeButton;
	[Export] public Button MainMenuButton;

	private HashSet<Aspect> _lastOffered = new();

	public GameManager()
	{
		//Singleton
		if (Instance != null)
		{

			QueueFree();
			return;
		}

		Instance = this;
	}

	public override void _Ready()
	{
		
		GetTree().Paused = false;
		ManaLabel.Text = $"Mana: {Mana}";


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
		_pauseMenu = GetNodeOrNull<PauseMenu>(PauseMenuPath);
		ResumeButton.Pressed += OnGameResume;
		MainMenuButton.Pressed  += OnMainMenu;
		PauseButton.Pressed  += OnGamePaused;

		_waveLibrary = new Godot.Collections.Dictionary<string, Wave>();

		LoadAllWaves();

		StartNextWave();


	}

	private void LoadAllWaves()
	{

		if (_waveLibrary.Count > 0)
		{
			_waveLibrary.Clear();
		}

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
			if (!dir.CurrentIsDir() && (fileName.EndsWith(".tres") || fileName.EndsWith(".res")))
			{

				string filePath = $"{WaveFolderPath}/{fileName}";
				var wave = ResourceLoader.Load<Wave>(filePath);
				if (wave != null)
				{
					string key = wave.ID;
					_waveLibrary.Add(key, wave);
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

		Wave wave = GetWave();
		if (wave == null)
		{
			GD.PrintErr("[GM] Failed to select a wave, attempting again next frame");
			StartWaveNextFrame();
			return;
		}

		GD.Print($"[GM] Starting wave {_currentWave}: {wave.ID}");
		_waveDirector.StartWave(wave);
		_enemiesRemaining = wave.WaveInfo.Count;
	}

	private async void StartWaveNextFrame()
	{
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		StartNextWave();
	}

	/// <summary>
	/// returns a random Wave from the loaded wave library
	/// </summary>
	public Wave GetWave()
	{
		//if no waves, get random wave
		//TODO for Build, put in wave count fallback as well, just so we don't keep getting easy waves
		//if (_waveLibrary.Count == 0 || _currentWave > 3)
		//return WaveGenerator.GenerateWave(_currentWave * _currentWave);

		//TODO, switch to Godot's RandomNumberGenerator randWeighted

		// compute total weight
		float totalWeight = 0;
		foreach (var wave in _waveLibrary.Values)
		{
			totalWeight += Mathf.Max(wave.SelectionWeight, 0.0f);

		}



		var rng = new RandomNumberGenerator();
		float choice = (float)(rng.Randf() * totalWeight);

		foreach (var wave in _waveLibrary.Values)
		{
			choice -= Mathf.Max(wave.SelectionWeight, 0.0f);
			if (choice <= 0)
			{
				return wave;

			}
		}

		// fallback to random
		return WaveGenerator.GenerateWave(_currentWave * _currentWave);
	}



	public void OnEnemyDied(Enemy enemy)
	{
		 Mana += 10;
		ManaLabel.Text = $"Mana: {Mana}";


		_enemiesRemaining--;

		if (_enemiesRemaining <= 0)
		{
			GD.Print($"WAVE {_currentWave} CLEAR");
			OfferEndOfRoundRewards();
		}

		_waveDirector.RemoveActiveEnemy(enemy);
	}
	public bool TrySpendMana(int amount)
	{
		if (Mana < amount)
			return false;

		Mana -= amount;
		ManaLabel.Text = $"Mana: {Mana}";

		return true;
	}

	public void OnPlayerDefeated()
	{
		_gameOverText.Visible = true;
		GetTree().Paused = true;
		GetTree().ChangeSceneToFile(StartScreenPath);
		GetTree().Paused = false;
	}
	public void OnGamePaused()
	{
		_pauseMenu.Visible = true;
		GetTree().Paused = true;
	}
	public void OnGameResume()
	{
		_pauseMenu.Visible = false;
		GetTree().Paused = false;
	}
	public void OnMainMenu()
	{
		GetTree().ChangeSceneToFile(StartScreenPath);
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
