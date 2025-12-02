using Godot;
using System;
using System.Collections.Generic;

public enum Rarity
{
	Common = 0,
	Rare = 1,
	Epic = 2,
	Legendary = 3
}

[GlobalClass]
public partial class AspectTemplate : Resource
{

	[Export] public string _id; //a unique id for this type of Aspect
	[Export] public string DisplayName; //A display name for this aspect, such as "Rapid Fire"
	[Export] public Texture2D AspectSprite;

	//ToDo: add information/data/links to resources about sprites and such... once we have them.
		//must be a Godot Array to be exported
	[Export] public Godot.Collections.Array<ModifierInfo> Modifiers;
	[Export] public Rarity Rarity = Rarity.Common;
	[Export] public int ScrapAmount;
	[Export] public int ManaAmount;



	public AspectTemplate() { }




}
