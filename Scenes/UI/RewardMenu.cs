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

	// NEW: TextureRects under each button (Icons)
	[Export] public NodePath Btn0IconPath = "Panel/VBox/Row/Btn0/Icon";
	[Export] public NodePath Btn1IconPath = "Panel/VBox/Row/Btn1/Icon";
	[Export] public NodePath Btn2IconPath = "Panel/VBox/Row/Btn2/Icon";

	private Label _title;
	private Button[] _buttons = new Button[3];
	private TextureRect[] _icons = new TextureRect[3];

	private List<AspectTemplate> _currentChoices;

	public override void _Ready()
	{
		_title = GetNode<Label>(TitlePath);
		_buttons[0] = GetNode<Button>(Btn0Path);
		_buttons[1] = GetNode<Button>(Btn1Path);
		_buttons[2] = GetNode<Button>(Btn2Path);

		_icons[0] = GetNodeOrNull<TextureRect>(Btn0IconPath);
		_icons[1] = GetNodeOrNull<TextureRect>(Btn1IconPath);
		_icons[2] = GetNodeOrNull<TextureRect>(Btn2IconPath);

		// Button click
		_buttons[0].Pressed += () => OnButton(0);
		_buttons[1].Pressed += () => OnButton(1);
		_buttons[2].Pressed += () => OnButton(2);

		// Hover → show tooltip “as if in bar” (AboveLeft)
		_buttons[0].MouseEntered += () => OnHoverEnter(0);
		_buttons[1].MouseEntered += () => OnHoverEnter(1);
		_buttons[2].MouseEntered += () => OnHoverEnter(2);

		_buttons[0].MouseExited += OnHoverExit;
		_buttons[1].MouseExited += OnHoverExit;
		_buttons[2].MouseExited += OnHoverExit;

		// Make icon rects full bleed + untinted
		for (int i = 0; i < _icons.Length; i++)
		{
			if (_icons[i] == null) continue;
			_icons[i].StretchMode = TextureRect.StretchModeEnum.Scale;
			_icons[i].ExpandMode  = TextureRect.ExpandModeEnum.IgnoreSize;
			_icons[i].Modulate    = Colors.White;
		}

		Hide();
	}

	public void ShowChoices(int waveNumber, List<AspectTemplate> choices)
	{
		_currentChoices = choices ?? new List<AspectTemplate>();

		// Title
		if (_title != null)
			_title.Text = $"Wave {waveNumber} cleared!";

		// Populate buttons & icons
		for (int i = 0; i < _buttons.Length; i++)
		{
			bool has = i < _currentChoices.Count;
			_buttons[i].Visible = has;

			if (!has)
				continue;

			var t = _currentChoices[i];

			// Your choice: keep label text or clear it (since icon shows the aspect)
			_buttons[i].Text = $"{t.DisplayName}\n({t.Rarity})";
			_buttons[i].Disabled = false;

			// Put the aspect image on the TextureRect child
			if (_icons[i] != null)
				_icons[i].Texture = t.AspectSprite; // <<— uses template’s icon

			// Remove any rarity tinting – transparent theme will let icon show fully
			_buttons[i].Modulate = Colors.White;
		}

		Show();
	}

	private void OnButton(int index)
	{
		if (_currentChoices == null) return;
		if (index < 0 || index >= _currentChoices.Count) return;

		var picked = _currentChoices[index];
		EmitSignal(SignalName.ChoicePicked, picked);
	}

	// ===== Tooltip on hover (show “as if in the bar” above-left) =====

	private void OnHoverEnter(int index)
	{
		if (_currentChoices == null) return;
		if (index < 0 || index >= _currentChoices.Count) return;

		var t = _currentChoices[index];
		var tooltip = GetTooltip();
		if (tooltip == null) return;

		// Build a temporary Aspect instance from the template for display
		var aspectForTooltip = BuildTempAspect(t);
		if (aspectForTooltip == null) return;

		// Anchor like bar: menu bottom-left = button top-left
		tooltip.ShowAspectAtControl(
			aspectForTooltip,
			_buttons[index],
			AspectHoverMenu.MenuAnchor.AboveLeft,
			new Vector2(0, -2)
		);
	}

	private void OnHoverExit()
	{
		GetTooltip()?.HideTooltip();
	}

	private AspectHoverMenu GetTooltip()
	{
		var list = GetTree().GetNodesInGroup("AspectTooltip");
		return list.Count > 0 ? list[0] as AspectHoverMenu : null;
	}

	// ---- Replace this with your actual “create an Aspect from template” path if different ----
private Aspect BuildTempAspect(AspectTemplate t)
{

	return new Aspect(t);
}

}
