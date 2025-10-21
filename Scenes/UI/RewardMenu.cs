using Godot;
using System;
using System.Collections.Generic;

public partial class RewardMenu : Control
{
	[Signal] public delegate void ChoicePickedEventHandler(AspectTemplate picked);

	[Export] public NodePath TitlePath = "Panel/VBox/Title";
	[Export] public NodePath RowPath   = "Panel/VBox/Row";
	[Export] public NodePath Btn0Path  = "Panel/VBox/Row/Btn0";
	[Export] public NodePath Btn1Path  = "Panel/VBox/Row/Btn1";
	[Export] public NodePath Btn2Path  = "Panel/VBox/Row/Btn2";

	private Label _title;
	private Button[] _buttons = new Button[3];
	private List<AspectTemplate> _currentChoices;

	public override void _Ready()
	{
		_title = GetNode<Label>(TitlePath);
		_buttons[0] = GetNode<Button>(Btn0Path);
		_buttons[1] = GetNode<Button>(Btn1Path);
		_buttons[2] = GetNode<Button>(Btn2Path);

		// Button handlers
		_buttons[0].Pressed += () => OnButton(0);
		_buttons[1].Pressed += () => OnButton(1);
		_buttons[2].Pressed += () => OnButton(2);

		Hide();
	}

	public void ShowChoices(int waveNumber, List<AspectTemplate> choices)
	{
		_currentChoices = choices ?? new List<AspectTemplate>();

		// Title
		if (_title != null)
			_title.Text = $"Wave {waveNumber} cleared!\nChoose one Aspect";

		// Populate buttons
		for (int i = 0; i < _buttons.Length; i++)
		{
			if (i < _currentChoices.Count)
			{
				var t = _currentChoices[i];
				_buttons[i].Text = $"{t.DisplayName}\n({t.Rarity})";
				_buttons[i].Disabled = false;
				_buttons[i].Visible = true;

				// Optional: color by rarity
				_buttons[i].Modulate = RarityColor(t.Rarity);
			}
			else
			{
				_buttons[i].Visible = false;
			}
		}

		Show();
	}

	private void OnButton(int index)
	{
		GD.Print($"[RewardMenu] Button {index} pressed");

		if (_currentChoices == null)
		{
			GD.Print("[RewardMenu] _currentChoices is NULL");
			return;
		}
		if (index < 0 || index >= _currentChoices.Count)
		{
			GD.Print($"[RewardMenu] Index {index} out of range ({_currentChoices.Count})");
			return;
		}

		var picked = _currentChoices[index];
		GD.Print($"[RewardMenu] Emitting pick: {picked?._id}");
		EmitSignal(SignalName.ChoicePicked, picked);
		// NOTE: Don't Hide() here – let GameManager hide after acquisition
	}

	private static Color RarityColor(Rarity r) => r switch
	{
		Rarity.Common    => new Color(0.8f, 0.8f, 0.8f),
		Rarity.Rare      => new Color(0.4f, 0.6f, 1.0f),
		Rarity.Epic      => new Color(0.75f, 0.4f, 0.95f),
		Rarity.Legendary => new Color(1.0f, 0.85f, 0.3f),
		_ => Colors.White
	};
}
