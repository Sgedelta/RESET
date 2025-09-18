using Godot;
using System;

public partial class Enemy : Node2D
{
	[Export] public float MaxHp = 30f;
	public float HP;

	public override void _Ready()
	{
		HP = MaxHp;
	}

	public void TakeDamage(float dmg)
	{
		HP -= dmg;
		if (HP <= 0f)
			QueueFree();
	}
	
	public void SetPath(Path2D path)
	{
		var follower = GetNode<PathFollower>("PathFollower");
		follower.SetPath(path);
	}
}
