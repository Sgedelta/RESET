using Godot;
using System;

public partial class WallObject : Node2D
{
	[Export] public float DurationSeconds = 3.0f;


	//  - Thickness is along the path (small depth)
	[Export] public float Length = 140f;     // across the path (normal)
	[Export] public float Thickness = 28f;

	[Export] public float ReapplyInterval = 0.05f;
	[Export] public float ApproachPadding = 18f;

	private float _life;
	private float _tick;

	private Vector2 _tangent = Vector2.Right;
	private Vector2 _normal  = Vector2.Down;

	private Vector2[] _corners = Array.Empty<Vector2>();

	public override void _Ready()
	{
		_life = DurationSeconds;
		_tick = 0f;

		ComputeBasisFromPath();
		RebuildCorners();

		SetProcess(true);
	}

	public override void _Process(double deltaD)
	{
		float delta = (float)deltaD;

		_life -= delta;
		if (_life <= 0f) { QueueFree(); return; }

		_tick -= delta;
		if (_tick <= 0f)
		{
			_tick = ReapplyInterval;

			var enemies = GameManager.Instance.WaveDirector.ActiveEnemies;
			float halfLen = Length * 0.5f;
			float halfThk = Thickness * 0.5f;

			for (int i = enemies.Count - 1; i >= 0; i--)
			{
				var e = enemies[i];
				if (!GodotObject.IsInstanceValid(e)) continue;

				Vector2 d = e.GlobalPosition - GlobalPosition;
				float perp  = d.Dot(_normal);
				float along = d.Dot(_tangent);

				// Inside gate width across the path?
				bool insideWidth = MathF.Abs(perp) <= halfLen;

				bool beforeWallFace = (along >= -ApproachPadding) && (along <= halfThk);

				if (insideWidth && beforeWallFace)
				{
					e.ApplySlow(1f, ReapplyInterval + 0.05f);
				}
			}
		}
	}

	private void ComputeBasisFromPath()
	{
		var wave  = GameManager.Instance?.WaveDirector;
		var path  = wave?.Path;
		var curve = wave?.Curve;

		if (path == null || curve == null)
		{
			_tangent = Vector2.Right;
			_normal  = new Vector2(-_tangent.Y, _tangent.X);
			return;
		}

		var baked = curve.GetBakedPoints();
		if (baked == null || baked.Length < 2)
		{
			_tangent = Vector2.Right;
			_normal  = new Vector2(-_tangent.Y, _tangent.X);
			return;
		}

		Vector2 worldPos = GlobalPosition;
		int bestIndex = 0;
		float bestDist = float.MaxValue;

		for (int i = 0; i < baked.Length; i++)
		{
			Vector2 wp = path.ToGlobal(baked[i]);
			float d2 = worldPos.DistanceSquaredTo(wp);
			if (d2 < bestDist) { bestDist = d2; bestIndex = i; }
		}

		int prev = Math.Max(bestIndex - 1, 0);
		int next = Math.Min(bestIndex + 1, baked.Length - 1);

		Vector2 pPrev = path.ToGlobal(baked[prev]);
		Vector2 pNext = path.ToGlobal(baked[next]);

		_tangent = (pNext - pPrev).Normalized();
		if (_tangent.LengthSquared() < 0.0001f) _tangent = Vector2.Right;

		_normal = new Vector2(-_tangent.Y, _tangent.X);
	}

	private void RebuildCorners()
	{
		float hl = Length * 0.5f;
		float ht = Thickness * 0.5f;

		Vector2 a =  _normal * hl + _tangent * ht;
		Vector2 b = -_normal * hl + _tangent * ht;
		Vector2 c = -_normal * hl - _tangent * ht;
		Vector2 d =  _normal * hl - _tangent * ht;

		_corners = new[] { a, b, c, d };
	}

	public override void _Draw()
	{
		var fill = new Color(0.08f, 0.08f, 0.1f, 0.35f);
		DrawPolygon(_corners, new[] { fill, fill, fill, fill });

		var edge = new Color(0, 0, 0, 0.95f);
		DrawPolyline(_corners, edge, 3.0f, true);
		DrawPolyline(_corners, edge, 1.5f, true);
	}
}
