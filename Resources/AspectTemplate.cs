using Godot;
using System;
using System.Collections.Generic;


public abstract partial class ModifierInfo : Resource
{
    //see Aspect.cs for StatType and ModifierType declarations
    [Export] public StatType StatType;
    [Export] public ModifierType ModifierType;


    /// <summary>
    /// A method to be implemented in all specfic modifier infos. 
    /// Must return object, because each ModifierInfo exists because
    /// we want them to return different types. 
    /// </summary>
    /// <returns></returns>
    public abstract object GetStat();

}


public partial class AspectTemplate : Resource
{

    [Export] public string _id; //a unique id for this type of Aspect
    [Export] public string DisplayName; //A display name for this aspect, such as "Rapid Fire"

    //ToDo: add information/data/links to resources about sprites and such... once we have them.
        //must be a Godot Array to be exported
    [Export] public Godot.Collections.Array<ModifierInfo> Modifiers;


    public AspectTemplate() { }

}
