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

	// where to place relative to mouse
	private Vector2 _offset = new Vector2(14, -40);

	public override void _Ready()
	{
		_name   = GetNode<Label>(NamePath);
		_rarity = GetNode<Label>(RarityPath);
		_stats  = GetNode<RichTextLabel>(StatsPath);

		Visible = false;
		AddToGroup("AspectTooltip");
	}

	public void ShowAspect(Aspect aspect, Vector2 globalMousePos)
	{
		_name.Text   = aspect.Template.DisplayName ?? "Aspect";
		_rarity.Text = aspect.Template.Rarity.ToString();

		_stats.Clear();
		_stats.PushMono();
		_stats.AppendText(BuildLines(aspect));
		_stats.Pop();

		CallDeferred(nameof(PlaceMenu), globalMousePos);
		Show();
	}

	public void HideTooltip() => Hide();

	private void PlaceMenu(Vector2 mousePos)
	{
		var desired = mousePos + _offset;
		var vp = GetViewport().GetVisibleRect();

		var size = GetCombinedMinimumSize();
		var finalPos = desired;

		if (finalPos.X + size.X > vp.Size.X) finalPos.X = vp.Size.X - size.X - 4;
		if (finalPos.Y + size.Y > vp.Size.Y) finalPos.Y = vp.Size.Y - size.Y - 4;

		GlobalPosition = finalPos;
	}

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
		// int vs float value
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
