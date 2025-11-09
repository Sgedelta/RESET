using Godot;

public partial class FireSFX : Node2D
{
	[Export] public float Radius = 96f;
	[Export] public float DurationSeconds = 4.0f;

	private float _life;
	private float _pulseT;
	private float _alpha = 1f;

	public override void _Ready()
	{
		_life = DurationSeconds;
		SetProcess(true);
	}

	public override void _Process(double deltaD)
	{
		float delta = (float)deltaD;

		_life -= delta;
		if (_life <= 0f)
		{
			QueueFree();
			return;
		}

		if (_life < 0.4f)
			_alpha = Mathf.Clamp(_life / 0.4f, 0f, 1f);

		_pulseT += delta;
		QueueRedraw();
	}

	public override void _Draw()
	{
		float aBase = 0.35f + 0.10f * Mathf.Sin(_pulseT * 8.0f);
		float a = Mathf.Clamp(aBase * _alpha, 0f, 1f);
		float r = Radius + 3f * Mathf.Sin(_pulseT * 6.3f);

		var edge = new Color(0.95f, 0.25f, 0.05f, Mathf.Clamp(a + 0.1f, 0f, 1f));
		for (int i = 0; i < 3; i++)
			DrawCircle(Vector2.Zero, r + 2f + i * 2f, edge);

		DrawCircle(Vector2.Zero, r * 0.95f, new Color(1.0f, 0.45f, 0.1f, a));
		DrawCircle(Vector2.Zero, r * 0.70f, new Color(1.0f, 0.65f, 0.2f, a * 0.8f));
		DrawCircle(Vector2.Zero, r * 0.45f, new Color(1.0f, 0.85f, 0.3f, a * 0.6f));
	}
}
