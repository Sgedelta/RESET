using Godot;
using System;

[GlobalClass]
public partial class ModifierInfo : Resource
{

    private static Godot.Collections.Dictionary<StatType, Godot.Collections.Array<float>> StatValues;

    private static RandomNumberGenerator modRNG;

    //see Aspect.cs for StatType and ModifierType declarations
    [Export] public StatType ModifiedStat;
    [Export] public ModifierType ModifierType;
    //turn on for anything we want back end to snap to an int
    //  the application of things that NEED to be ints - chain targets, poison ticks, piercing amount
    //  are handled on application side, but some displays just show the value.
    [Export] public bool SnapToInt = false; 
    /// <summary>
    /// If the stat should be increased or decreased. 
    /// For additive, this determines if the change is positive or negative.
    /// For multiplicative, this determines if the change is >1 or <1 (but always >0)
    /// </summary>
    [Export(PropertyHint.Enum, "Increase:1, Decrease:-1")] public int IncreaseOrDecrease = 1;
    /// <summary>
    /// The relative increase - 0 is a small increase, 9 is a very large increase
    /// </summary>
    [Export(PropertyHint.Range, "0,9")] public int RelativeChange = 0;

    /// <summary>
    /// If this stat should be slightly randomized. Most of the time, this should be true, but sometimes we want to set it to an exact value
    /// </summary>
    [Export] public bool AllowRandomization = true;

    // Sometimes (i.e. "setting spread to max") we need an exact value to set something to.
    //	Use this in that case, but USE SPARINGLY in general.
    //  Still allows use of randomization
    [Export] public bool UseExactValue = false;
    [Export] public float ExactValue = 0;

    public virtual float GetStat(float level = 1)
    {
        //get the data
        Godot.Collections.Array<float> stats = StatValues[ModifiedStat];
        //calculate random things, we can throw them in when we return
        float randomAmount = StatValues[ModifiedStat][11];
        float randomMult = 1; //if we aren't doing stuff with random, this won't change
        if(AllowRandomization)
        {
            //randomize the multiplier
            randomMult = modRNG.RandfRange(1 - randomAmount, 1 + randomAmount);
        }
        //if we are using an exact value, use that exact value - including randomization
        if (UseExactValue)
        {
            if (SnapToInt)
            {
                return (int)(ExactValue * randomMult);
            }
            return ExactValue * randomMult;
        }

        //set up all stats we'll use
        float minVal = 0; //to be replaced
        float maxVal = 0; //to be replaced
        float levelMult = 1; //to be replaced, except in mult cases

        //multiply only uses min and max - does not scale with level
        //  this is because that would be an exponential increase on a multiplicitive effect on a stat that has 
        //  likely already been increased using an exponential linear increase and... yeah. that gets into the
        //  "out of bounds of floats" territory. Not needed. 
        if(ModifierType == ModifierType.Multiply)
        {
            if (IncreaseOrDecrease == 1)
            {
                minVal = stats[7];
                maxVal = stats[8];
            }
            else if(IncreaseOrDecrease == -1)
            {
                minVal = stats[9];
                maxVal = stats[10];
            }
        }
        else //setting or additive but commonly setting uses exactValue
        {
            if (IncreaseOrDecrease == 1)
            {
                minVal = stats[1];
                maxVal = stats[2];
                levelMult = stats[3];
            }
            else if (IncreaseOrDecrease == -1)
            {
                minVal = stats[4];
                maxVal = stats[5];
                levelMult = stats[6];
            }
        }


        //actually calculate!
            //get base value by lerping between
        float statVal = Mathf.Lerp(minVal, maxVal, RelativeChange/9);

            //scale with level, if we do that.
        if(levelMult != 1)
        {
            statVal *= Mathf.Pow(levelMult, level);
        }


        //return the stat (times the random we calculated up top)
        if (SnapToInt)
        {
            return (int)(statVal * randomMult);
        }

        return statVal * randomMult;
    }



    public ModifierInfo()
    {
        if (modRNG == null)
        {
            modRNG = new RandomNumberGenerator();
        }


        if(StatValues != null)
        {
            //no other setup needed, StatValues has been read and initialized. It does not change during the game
            return;
        }

        //initialize the array
        StatValues = new Godot.Collections.Dictionary<StatType, Godot.Collections.Array<float>>();
        
        if (!FileAccess.FileExists("res://Resources/Modifiers/ModifiersLevelInfo.csv"))
        {   
            //we can't get to the file...
            GD.PrintErr("File Access cannot find ModifiersLevelInfo.csv when creating ModifierInfo!!");
            return;
        }

        //open csv file that stores the data
        FileAccess file = FileAccess.Open("res://Resources/Modifiers/ModifiersLevelInfo.csv", FileAccess.ModeFlags.Read);

        //get the data in a loop
        while (file.GetPosition() < file.GetLength())
        {
            // Read data
            var data = file.GetCsvLine();
            StatType statToEdit;
            switch(data[0])
            {
                case "Damage":
                    statToEdit = StatType.Damage;
                    break;
                case "Firerate":
                    statToEdit = StatType.FireRate;
                    break;
                case "Range":
                    statToEdit = StatType.Range;
                    break;
                case "Spread Angle":
                    statToEdit = StatType.SpreadAngle;
                    break;
                case "Spread Falloff":
                    statToEdit = StatType.SpreadFalloff;
                    break;
                case "Crit Chance":
                    statToEdit = StatType.CritChance;
                    break;
                case "Crit Mult":
                    statToEdit = StatType.CritMult;
                    break;
                case "Splash Coef":
                    statToEdit = StatType.SplashCoef;
                    break;
                case "Splash Radius":
                    statToEdit = StatType.SplashRadius;
                    break;
                case "Poison Damage":
                    statToEdit = StatType.PoisonDamage;
                    break;
                case "Poison Ticks":
                    statToEdit = StatType.PoisonTicks;
                    break;
                case "Chain Targets":
                    statToEdit = StatType.ChainTargets;
                    break;
                case "Chain Distance":
                    statToEdit = StatType.ChainDistance;
                    break;
                case "Piercing Amount":
                    statToEdit = StatType.PiercingAmount;
                    break;
                case "Knockback Amount":
                    statToEdit = StatType.KnockbackAmount;
                    break;
                case "Slowdown Percent":
                    statToEdit = StatType.SlowdownPercent;
                    break;
                case "Slowdown Length":
                    statToEdit = StatType.SlowdownLength;
                    break;
                case "Homing Strength":
                    statToEdit = StatType.HomingStrength;
                    break;
                default:
                    GD.PushError($"Could not find stat type associated with {data[0]}");
                    continue;
            }
        
            if( StatValues.ContainsKey( statToEdit ) )
            {
                GD.PushWarning($"Already has data for {data[0]}, skipping adding this line of data!");
                continue;
            }
            StatValues.Add(statToEdit, new Godot.Collections.Array<float> 
            {
                //additive/setting data
                data[1].ToFloat(), data[2].ToFloat(), data[3].ToFloat(), //positives
                data[4].ToFloat(), data[5].ToFloat(), data[6].ToFloat(), //negatives
                //multiplicative
                data[7].ToFloat(), data[8].ToFloat(), //positives 
                data[9].ToFloat(), data[10].ToFloat(),//negatives
                //random
                data[11].ToFloat()
            });
        }

        //close the file
        file.Close();
    }
}
