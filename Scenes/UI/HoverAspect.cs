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

		_parentCtrl.MouseEntered += OnMouseEntered;
		_parentCtrl.MouseExited  += OnMouseExited;
	}

	private void OnMouseEntered()
	{
		var aspect  = ResolveAspect();
		var tooltip = GetTooltip();

		if (aspect == null || tooltip == null || _parentCtrl == null)
			return;

		// Decide anchor based on where this aspect/token is
		AspectHoverMenu.MenuAnchor anchor;
		Vector2 offset = Vector2.Zero;

		// If the hovered control is a token, we can read its placement directly
		if (_parentCtrl is AspectToken tok)
		{
			if (tok.Place == TokenPlace.Bar)
			{
				// BAR: menu above token, bottom-left of menu -> top-left of token
				anchor = AspectHoverMenu.MenuAnchor.AboveLeft;
				offset = new Vector2(0, -2);
			}
			else
			{
				// SLOT: menu to the left, top-right of menu -> top-left of token
				anchor = AspectHoverMenu.MenuAnchor.LeftTop;
				offset = new Vector2(-6, 0);
			}
		}
		else if (_parentCtrl is AspectSlot)
		{
			// Hovering the slot itself: treat as tower menu placement
			anchor = AspectHoverMenu.MenuAnchor.LeftTop;
			offset = new Vector2(-6, 0);
		}
		else
		{
			// Fallback (safe default)
			anchor = AspectHoverMenu.MenuAnchor.AboveLeft;
			offset = new Vector2(0, -2);
		}

		tooltip.ShowAspectAtControl(aspect, _parentCtrl, anchor, offset);
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
		if (_parentCtrl is AspectToken token)
			return token.Aspect;

		if (_parentCtrl is AspectSlot slot)
		{
			var tower = slot.Pullout?.ActiveTower;
			return tower?.GetAspectInSlot(slot.Index);
		}

		return null;
	}
}
