using Godot;
using System;

public partial class Wave : Resource
{
	[Export] string ID;
	[Export] float selectionWeight;

	[Export] public Godot.Collections.Array<Godot.Collections.Array> WaveInfo;

}
