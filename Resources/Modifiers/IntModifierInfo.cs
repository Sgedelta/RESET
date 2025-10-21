using Godot;
using System;

public partial class IntModifierInfo : ModifierInfo
{
    //default constructor to do nothing, as this is mostly just a data holder, but just in case it helps godot seralize it
    public IntModifierInfo() { }


    [Export] public int statMin;
    [Export] public int statMax;
    [Export] public bool linearRandom;
    [Export] public int meanVal;
    [Export] public int stdDev;

    public override object GetStat()
    {
        RandomNumberGenerator rng = new RandomNumberGenerator();
        //note: rng.Randomize(); could be used here to set a seed.

        //if we're getting a random something linearly...
        if (linearRandom)
        {
            return rng.RandiRange(statMin, statMax);
        }

        //if we're doing it gaussian

        return (int)Mathf.Clamp(Mathf.Round(rng.Randfn(meanVal, stdDev)), statMin, statMax);

    }
}
