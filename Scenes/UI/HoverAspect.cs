using Godot;

public partial class HoverAspect : Node
{
	private Control parentNode;

	public override void _Ready()
	{
		parentNode = GetParent<Control>();

		// connect signals on parent control
		parentNode.MouseEntered += OnMouseEntered;
		parentNode.MouseExited  += OnMouseExited;
	}

	private void OnMouseEntered()
	{
		var aspect = ResolveAspect();
		var tooltip = GetTooltip();
		tooltip.ShowAspect(aspect, parentNode.GetViewport().GetMousePosition());
	}

	private void OnMouseExited()
	{
		var tooltip = GetTooltip();
		tooltip?.HideTooltip();
	}

	private AspectHoverMenu GetTooltip()
	{
		var list = GetTree().GetNodesInGroup("AspectTooltip");
		return list.Count > 0 ? list[0] as AspectHoverMenu : null;
	}

	private Aspect ResolveAspect()
	{
		// AspectToken
		if (parentNode is AspectToken token)
			return token.Aspect;

		//  TowerSlot
		if (parentNode is AspectSlot slot)
		{
			var pullout = GetParent<AspectSlot>().Pullout;
			var tower = pullout?.ActiveTower;
			return tower?.GetAspectInSlot(slot.Index);
		}

		return null;
	}
}
