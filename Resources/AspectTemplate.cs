using Godot;
using System;

public partial class AspectTemplate : Resource
{

    [Export] ModifierType modifierType;

    //Create Data Structure for this, should be able to store:
        //What it is changing
        //How it is changing it (modifier type)
        //How much its changing it by (a range - likely 2 floats)


    public AspectTemplate() { }

}
