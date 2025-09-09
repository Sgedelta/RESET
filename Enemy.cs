using Godot;
using System;

public partial class Enemy : PathFollow2D
{
	
	public void Progress() {
		ProgressRatio += .2f;
	}
	
	
}
