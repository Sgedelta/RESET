using Godot;
using System;

public partial class FloatModifierInfo : ModifierInfo
{

	//default constructor to do nothing, as this is mostly just a data holder, but just in case it helps godot seralize it
	public FloatModifierInfo() { }


	[Export] public float statMin;
	[Export] public float statMax;
	[Export] public bool linearRandom = true;
	[Export] public float meanVal;
	[Export] public float stdDev;

	public override object GetStat()
	{
		RandomNumberGenerator rng = new RandomNumberGenerator();
		//note: rng.Randomize(); could be used here to set a seed.
		
		//if we're getting a random something linearly...
		if (linearRandom)
		{
			return (GD.Randf() * (statMax - statMin)) + statMin;
		}

		//if we're doing it gaussian

		return Mathf.Clamp(rng.Randfn(meanVal, stdDev), statMin, statMax);

	}


}
