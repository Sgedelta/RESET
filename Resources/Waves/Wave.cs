using Godot;
using System;

public partial class Wave : Resource
{
	[Export] string _id;
	public string ID
	{
		get { return _id; }
	}
	[Export] float selectionWeight;
	public float SelectionWeight { get { return selectionWeight; } }

	[Export] public Godot.Collections.Array<Godot.Collections.Array> WaveInfo;


	public Wave(string id)
	{
		_id = id;
		WaveInfo = new Godot.Collections.Array<Godot.Collections.Array>();
	}

	//default for Resource Loading
	public Wave()
	{

	}
}
