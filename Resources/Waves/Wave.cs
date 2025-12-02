using Godot;
using System;



public partial class Wave : Resource
{
	public enum Difficulty { Easy, Medium, Hard }
	
	[Export] public string ID;
	[Export] public float SelectionWeight;
	[Export] public Difficulty WaveDifficulty;


	[Export] public Godot.Collections.Array<Godot.Collections.Array> WaveInfo;

}
