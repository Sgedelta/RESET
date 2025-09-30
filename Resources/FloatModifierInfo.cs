using Godot;
using System;

public partial class FloatModifierInfo : ModifierInfo
{

    //Create Data Structure for this, should be able to store:
    //What it is changing
    //How it is changing it (modifier type)
    //How much its changing it by (a range - likely 2 floats)

    //default constructor to do nothing, as this is mostly just a data holder, but just in case it helps godot seralize it
    public FloatModifierInfo() { }


    [Export] public float statMin;
    [Export] public float statMax;
    [Export] public bool linearRandom;
    [Export] public float meanVal;
    [Export] public float stdDev;

    public override object GetStat()
    {
        //if we're getting a random something linearly...
        if(linearRandom)
        {
            return (GD.Randf() * (statMax - statMin)) + statMin;
        }

        //if we're doing it gaussian
        RandomNumberGenerator rng = new RandomNumberGenerator();
        //note: rng.Randomize(); could be used here to set a seed.

        return Mathf.Clamp(rng.Randfn(meanVal, stdDev), statMin, statMax);

    }


}
