using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class GameManager : Node
{
	[Export] public NodePath WaveDirectorPath;
	[Export] public Path2D EnemiesRoot;
	[Export] public int StartingWaveSize = 5;
	[Export] public float StartingWaveDuration = 15f;
	[Export] public NodePath AspectBarPath;
	
	private AspectBar _aspectBar;

	//Singleton
	public static GameManager Instance;
	
	public AspectInventory Inventory { get; private set; }
	
	private WaveDirector _waveDirector;
	public WaveDirector WaveDirector {  get { return _waveDirector; } }

	private int _currentWave = 0;
	private int _enemiesRemaining = 0;
	private float _duration = 0;
	
	[Export] public NodePath gameOverTextPath;
	private Label _gameOverText;
	
	[Export] public NodePath RewardMenuPath;
	private RewardMenu _rewardMenu; 

	private HashSet<Aspect> _lastOffered = new();
	

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
		
		_aspectBar = GetNodeOrNull<AspectBar>(AspectBarPath);
		
			_rewardMenu = GetNodeOrNull<RewardMenu>(RewardMenuPath);
			if (_rewardMenu != null)
			{
				_rewardMenu.ProcessMode = Node.ProcessModeEnum.WhenPaused; // Godot 4
				_rewardMenu.Hide();
				_rewardMenu.ChoicePicked += OnAspectTemplatePicked;  // <-- SUBSCRIBE!
				GD.Print($"[GM] Subscribed to RewardMenu at {_rewardMenu.GetPath()}");
			}

		StartNextWave();
	}

	private void StartNextWave()
	{
		_currentWave++;
		_enemiesRemaining = StartingWaveSize * (_currentWave +1);
		_duration = StartingWaveDuration - (_currentWave * .5f);
		_waveDirector.StartWave(_enemiesRemaining, _duration);

		GD.Print($"start wave {_currentWave} : {_enemiesRemaining} enemies");
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
		//there are no enemies, get out
		if (_enemiesRemaining <= 0) return null;

		var filterEnemies = _waveDirector.ActiveEnemies.Where(e => !exclude.Contains(e));

		// unfortunely a little slow but this is the best way to do it for our purposes
		// this can be better if we quad tree it but that's more overhead and work for us
		// so. No! we'll stick with squared distance and then just retarget less frequently.
		float closestSqDist = float.MaxValue;
		Enemy nearest = null;
		foreach (Enemy e in filterEnemies)
		{
			if(closestSqDist > e.GlobalPosition.DistanceSquaredTo(point))
			{
				nearest = e;
				closestSqDist = e.GlobalPosition.DistanceSquaredTo(point);
			}
		}
		return nearest;
	}

	public Enemy GetNearestEnemyToPoint(Vector2 point)
	{
		return GetNearestEnemyToPoint(point, new List<Enemy>());	
	}
	
	//aspect rewards
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
