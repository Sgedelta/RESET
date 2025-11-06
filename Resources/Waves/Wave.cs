using Godot;
using System;

public partial class Wave : Resource
{
	[Export] public string ID;
	[Export] public float SelectionWeight;

	[Export] public Godot.Collections.Array<Godot.Collections.Array> WaveInfo;

}
