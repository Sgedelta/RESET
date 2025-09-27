using Godot;
using System;

public abstract class ModifierUnit
{
    public ModifierType Type;
    public StatType Stat;



}


public class FloatModifierUnit : ModifierUnit
{
    public float Value;
}
