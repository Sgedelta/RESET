using Godot;

public partial class StartMenu : Control
{
	[Export] public Button StartButton;
	[Export] public Button QuitButton;

	[Export] public string GameScenePath = "res://Scenes/Run.tscn";

	public override void _Ready()
	{

		StartButton.Pressed += OnStartPressed;
		QuitButton.Pressed  += OnQuitPressed;

		GetTree().Paused = false;
	}

	private void OnStartPressed()
	{
		GetTree().ChangeSceneToFile(GameScenePath);
	}

	private void OnQuitPressed()
	{
		GetTree().Quit();
	}
}
