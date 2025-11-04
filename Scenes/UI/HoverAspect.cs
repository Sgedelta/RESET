using Godot;

public partial class HoverAspect : Node
{
	private Control _parentCtrl;

	public override void _Ready()
	{
		_parentCtrl = GetParentOrNull<Control>();
		if (_parentCtrl == null)
		{
			GD.PushWarning("[HoverAspect] Parent is not a Control; tooltip will not show.");
			return;
		}

		// Connect hover signals on the parent control
		_parentCtrl.MouseEntered += OnMouseEntered;
		_parentCtrl.MouseExited  += OnMouseExited;
	}

	private void OnMouseEntered()
	{
		var aspect  = ResolveAspect();
		var tooltip = GetTooltip();

		if (aspect == null || tooltip == null || _parentCtrl == null)
			return;

		// Anchor the tooltip to the parent control (token or slot)
		tooltip.ShowAspectAtControl(
			aspect,
			_parentCtrl,
			AspectHoverMenu.MenuAnchor.AboveLeft,   // tweak if you prefer another anchor
			new Vector2(0, -6)                      // small extra offset
		);
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
		// If we're on an AspectToken, use its Aspect directly
		if (_parentCtrl is AspectToken token)
			return token.Aspect;

		// If we're on an AspectSlot, ask its tower for the aspect at that index
		if (_parentCtrl is AspectSlot slot)
		{
			var tower = slot.Pullout?.ActiveTower;
			return tower?.GetAspectInSlot(slot.Index);
		}

		return null;
	}
}
