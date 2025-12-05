using Godot;

public partial class LaserSFX : Node2D
{
	[Signal] public delegate void ImpactStartedEventHandler();

	private enum Phase { Growing, Flashing, Holding, Fading }
	private Phase _phase = Phase.Growing;

	// --- NEW: Exposed so the ability can set the scaled radius ---
	private float _radius = 150f;
	public float Radius
	{
		get => _radius;
		set => _radius = value;
	}

	private float _currentRadius = 1f;
	private float _growSpeed = 180f; // pixels per second
	private float _outlineAlpha = 0f;

	private int _growFlashCount = 10;
	private int _growFlashIndex = 0;
	private bool _flashOn = true;
	private float _flashTimer = 0.15f;

	private int _finalFlashCount = 6;
	private int _finalFlashIndex = 0;

	private float _holdTimer = 0f;
	private float _fadeTimer = 0f;

	private Color _outlineColor = new Color(0, 0, 0, 0f);
	private Color _whiteColor = new Color(1, 1, 1, 1f);
	private float _alpha = 1f;

	public override void _Ready()
	{
		SetProcess(true);
	}

	public override void _Process(double deltaD)
	{
		float delta = (float)deltaD;

		switch (_phase)
		{
			case Phase.Growing:
				_currentRadius += _growSpeed * delta;

				// Clamp to scaled radius
				if (_currentRadius >= _radius)
				{
					_currentRadius = _radius;
					_phase = Phase.Flashing;
					_finalFlashIndex = 0;
					_flashTimer = 0.08f;
					break;
				}

				_flashTimer -= delta;
				if (_flashTimer <= 0f)
				{
					_flashOn = !_flashOn;
					_flashTimer = 0.08f;

					if (!_flashOn)
					{
						_outlineAlpha = Mathf.Clamp(_outlineAlpha + 0.08f, 0f, 0.7f);
						_outlineColor.A = _outlineAlpha;
						_growFlashIndex++;
					}
				}
				break;

			case Phase.Flashing:
				_flashTimer -= delta;
				if (_flashTimer <= 0f)
				{
					_flashOn = !_flashOn;
					_flashTimer = 0.06f;

					if (!_flashOn)
					{
						_outlineAlpha = Mathf.Clamp(_outlineAlpha + 0.05f, 0f, 1f);
						_outlineColor.A = _outlineAlpha;
						_finalFlashIndex++;
					}
				}

				if (_finalFlashIndex >= _finalFlashCount * 2)
				{
					EmitSignal(SignalName.ImpactStarted);
					_phase = Phase.Holding;
					_holdTimer = 1.0f;
					_flashOn = true;
				}
				break;

			case Phase.Holding:
				_holdTimer -= delta;
				if (_holdTimer <= 0f)
				{
					_phase = Phase.Fading;
					_fadeTimer = 1.0f;
				}
				break;

			case Phase.Fading:
				_fadeTimer -= delta;
				_alpha = Mathf.Clamp(_fadeTimer / 1.0f, 0f, 1f);
				_outlineColor.A = _outlineAlpha * _alpha;
				_whiteColor.A = _alpha;

				if (_fadeTimer <= 0f)
					QueueFree();
				break;
		}

		QueueRedraw();
	}

	public override void _Draw()
	{
		for (int i = 0; i < 3; i++)
		{
			DrawCircle(Vector2.Zero, _currentRadius + 2f + i * 2f, _outlineColor);
		}

		if (_flashOn)
			DrawCircle(Vector2.Zero, _currentRadius, _whiteColor);
	}
}
