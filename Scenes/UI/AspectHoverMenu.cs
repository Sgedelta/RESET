using Godot;
using System;
using System.Text;
using System.Globalization;

public partial class AspectHoverMenu : Control
{
	[Export] public NodePath NamePath   = "Panel/Margin/VBox/Name";
	[Export] public NodePath RarityPath = "Panel/Margin/VBox/Rarity";
	[Export] public NodePath StatsPath  = "Panel/Margin/VBox/stats";

	private Label _name;
	private Label _rarity;
	private RichTextLabel _stats;

	// Offset from mouse when using ShowAspect(..., mousePos)
	[Export] public Vector2 MouseOffset = new Vector2(14, -40);

	// Screen padding when clamping
	[Export] public float ClampPadding = 8f;

	public override void _Ready()
	{
		_name   = GetNode<Label>(NamePath);
		_rarity = GetNode<Label>(RarityPath);
		_stats  = GetNode<RichTextLabel>(StatsPath);

		Visible = false;
		AddToGroup("AspectTooltip");
	}

	// =========================================================
	// 1) Simple: mouse-based position, clamped to screen
	// =========================================================
	public void ShowAspect(Aspect aspect, Vector2 globalMousePos)
	{
		if (aspect == null) return;

		FillText(aspect);

		// Defer until sizes are resolved this frame
		CallDeferred(nameof(PlaceMenuAtMouseClamped), globalMousePos);
		Show();
	}

	private void PlaceMenuAtMouseClamped(Vector2 mousePos)
	{
		var desired = mousePos + MouseOffset;
		GlobalPosition = ClampToViewport(desired, GetMenuSize());
	}

	// =========================================================
	// 2) Optional: control-anchored placement (for later use)
	//    Example: Show above-left of the token, clamped
	// =========================================================
	public enum MenuAnchor
	{
		AboveLeft,   // bottom-left of menu aligns to top-left of target
		AboveRight,  // bottom-right of menu aligns to top-right of target
		LeftCenter,  // right-center of menu aligns to left-center of target
		RightCenter, // left-center of menu aligns to right-center of target
		BelowLeft,   // top-left of menu aligns to bottom-left of target
		BelowRight   // top-right of menu aligns to bottom-right of target
	}

	public void ShowAspectAtControl(Aspect aspect, Control target, MenuAnchor anchor = MenuAnchor.AboveLeft, Vector2 extraOffset = default)
	{
		if (aspect == null || target == null) return;

		FillText(aspect);

		CallDeferred(nameof(PlaceMenuAtControlClamped), target.GetPath(), (int)anchor, extraOffset);
		Show();
	}

	private void PlaceMenuAtControlClamped(NodePath targetPath, int anchorRaw, Vector2 extraOffset)
	{
		var target = GetNodeOrNull<Control>(targetPath);
		if (target == null) return;

		var targetRect = target.GetGlobalRect();
		var size = GetMenuSize();
		var anchor = (MenuAnchor)anchorRaw;

		Vector2 desired = anchor switch
		{
			MenuAnchor.AboveLeft   => new Vector2(targetRect.Position.X, targetRect.Position.Y - size.Y),
			MenuAnchor.AboveRight  => new Vector2(targetRect.End.X - size.X, targetRect.Position.Y - size.Y),
			MenuAnchor.LeftCenter  => new Vector2(targetRect.Position.X - size.X, targetRect.Position.Y + targetRect.Size.Y * 0.5f - size.Y * 0.5f),
			MenuAnchor.RightCenter => new Vector2(targetRect.End.X,         targetRect.Position.Y + targetRect.Size.Y * 0.5f - size.Y * 0.5f),
			MenuAnchor.BelowLeft   => new Vector2(targetRect.Position.X, targetRect.End.Y),
			MenuAnchor.BelowRight  => new Vector2(targetRect.End.X - size.X, targetRect.End.Y),
			_ => targetRect.End    // fallback
		};

		desired += extraOffset;
		GlobalPosition = ClampToViewport(desired, size);
	}

	// =========================================================
	// Utility
	// =========================================================
	private void FillText(Aspect aspect)
	{
		_name.Text   = aspect.Template.DisplayName ?? "Aspect";
		_rarity.Text = aspect.Template.Rarity.ToString();

		_stats.Clear();
		_stats.PushMono();
		_stats.AppendText(BuildLines(aspect));
		_stats.Pop();
	}

	private Vector2 GetMenuSize()
	{
		// Use the greater of current Size and minimum to be robust
		var min = GetCombinedMinimumSize();
		return new Vector2(Mathf.Max(Size.X, min.X), Mathf.Max(Size.Y, min.Y));
	}

	private Vector2 ClampToViewport(Vector2 desiredGlobalPos, Vector2 menuSize)
	{
		var vp = GetViewport().GetVisibleRect(); // (0,0)-(w,h) for visible area
		float pad = ClampPadding;

		float minX = vp.Position.X + pad;
		float minY = vp.Position.Y + pad;
		float maxX = vp.End.X - pad - menuSize.X;
		float maxY = vp.End.Y - pad - menuSize.Y;

		return new Vector2(
			Mathf.Clamp(desiredGlobalPos.X, minX, Mathf.Max(minX, maxX)),
			Mathf.Clamp(desiredGlobalPos.Y, minY, Mathf.Max(minY, maxY))
		);
	}

	public void HideTooltip() => Hide();

	// =========================================================
	// Text building (unchanged)
	// =========================================================
	private string BuildLines(Aspect aspect)
	{
		var sb = new StringBuilder();
		foreach (var mod in aspect.Modifiers)
		{
			string statName = StatDisplayName(mod.Stat);
			string line = FormatModifier(statName, mod);
			if (!string.IsNullOrEmpty(line))
				sb.AppendLine(line);
		}
		return sb.ToString();
	}

	private static string StatDisplayName(StatType stat)
	{
		return stat switch
		{
			StatType.FireRate        => "Fire Rate",
			StatType.Damage          => "Damage",
			StatType.Range           => "Range",
			StatType.Accuracy        => "Accuracy",
			StatType.CritChance      => "Crit Chance",
			StatType.CritMult        => "Crit Mult",
			StatType.SpreadAngle     => "Spread Angle",
			StatType.SpreadFalloff   => "Spread Falloff",
			StatType.SplashCoef      => "Splash Coef",
			StatType.SplashRadius    => "Splash Radius",
			StatType.PoisonDamage    => "Poison Damage",
			StatType.PoisonTicks     => "Poison Ticks",
			StatType.ChainTargets    => "Chain Targets",
			StatType.ChainDistance   => "Chain Distance",
			StatType.PiercingAmount  => "Piercing",
			StatType.KnockbackAmount => "Knockback",
			StatType.SlowdownPercent => "Slowdown %",
			StatType.SlowdownLength  => "Slowdown Length",
			StatType.HomingStrength  => "Homing Strength",
			_ => stat.ToString()
		};
	}

	private static string FormatModifier(string statName, ModifierUnit unit)
	{
		bool isInt = unit is IntModifierUnit;
		double val = isInt
			? ((IntModifierUnit)unit).Value
			: ((FloatModifierUnit)unit).Value;

		return unit.Type switch
		{
			ModifierType.Add      => $"{statName} + {Trim(val)}",
			ModifierType.Subtract => $"{statName} - {Trim(val)}",
			ModifierType.Multiply => $"{statName} X {Trim(val)}",
			_ => null
		};
	}

	private static string Trim(double v)
	{
		if (Math.Abs(v - Math.Round(v)) < 0.0001)
			return ((int)Math.Round(v)).ToString(CultureInfo.InvariantCulture);
		return v.ToString("0.###", CultureInfo.InvariantCulture);
	}
}
