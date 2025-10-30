using Godot;
using System;

public partial class EnemyInfo : Resource
{
	[Export] public PackedScene EnemyScene;
	[Export] public float HealthMultiplier = 1.0f;
	[Export] public float SpeedMultiplier = 1.0f;
	[Export] public float Delay = 1.0f;
}
