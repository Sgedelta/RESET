using Godot;
using System;


    public enum StatType
    {
        FireRate,
        Damage,
        Range,
        Accuracy,
        Spread
    }

    public enum ModifierType
    {
        Add,
        Multiply,
        Subtract
    }

    public enum UniqueEffect
    {
        None,
        Splash,
        Poison,
        Chain,
        Piercing,
        Knockback,
        Critical,
        Slow,
        Homing
    }

    public class Aspect
    {
        public string Name { get; private set; }
        public StatType Stat { get; private set; }
        public ModifierType Modifier { get; private set; }
        public float Value { get; private set; }
        public UniqueEffect Effect { get; private set; }

        public Aspect(string name, StatType stat, ModifierType modifier, float value, UniqueEffect effect = UniqueEffect.None)
        {
            Name = name;
            Stat = stat;
            Modifier = modifier;
            Value = value;
            Effect = effect;
        }
    }

