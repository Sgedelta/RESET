using Godot;

public partial class BarToggle : Button
{
	[Export] public NodePath AspectBarPath;   
	[Export] public bool StartVisible = false; 

	private Control _bar;
	private bool _isVisible;

	public override void _Ready()
	{
		FocusMode = FocusModeEnum.None; 
		Flat = true;

		_bar = GetNodeOrNull<Control>(AspectBarPath);
		if (_bar == null)
		{
			GD.PushError("BarToggleButton: AspectBarPath not set or invalid.");
			return;
		}

		_isVisible = StartVisible;
		_bar.Visible = _isVisible;

		UpdateButtonText();

		Pressed += OnTogglePressed;
	}

	private void OnTogglePressed()
	{
		_isVisible = !_isVisible;
		_bar.Visible = _isVisible;
		float offsetY = _isVisible ? -150 :0;

		// Update offsets directly
		OffsetTop = offsetY - Size.Y;
		OffsetBottom = offsetY;
		
		UpdateButtonText();
	}

	private void UpdateButtonText()
	{
		Text = _isVisible ? "Hide Aspects" : "Show Aspects";
	}
}
