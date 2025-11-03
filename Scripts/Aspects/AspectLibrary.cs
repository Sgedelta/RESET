using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using static Aspect;

/// <summary>
/// A Node that loads and can generate new aspects
/// </summary>
public partial class AspectLibrary : Node
{
	[Export] public Godot.Collections.Array<AspectTemplate> AspectTemplates;
	[Export] public Godot.Collections.Array<String> TemplatesFilePathsFromRes;

	public static readonly List<AspectTemplate> AllTemplates = new();
	public static readonly Dictionary<string, AspectTemplate> TemplatesById = new();
	
	private static readonly Dictionary<Rarity, int> RarityWeights = new()
	{
		{ Rarity.Common,    75 },
		{ Rarity.Rare,       20 },
		{ Rarity.Epic,       5 },
		{ Rarity.Legendary,  0 }
	};

	private static RandomNumberGenerator _rng;

	public override void _Ready()
	{
		_rng = new RandomNumberGenerator();
		_rng.Randomize();
		
		AllTemplates.Clear();
		TemplatesById.Clear();

		if (AspectTemplates == null) return;

		foreach (var t in AspectTemplates)
		{
			if (t == null || string.IsNullOrWhiteSpace(t._id))
			{
				GD.PushWarning("AspectTemplate missing or has empty _id");
				continue;
			}
			AllTemplates.Add(t);
			TemplatesById[t._id] = t;
		}
		GD.Print($"AspectLibrary loaded {AllTemplates.Count} templates");
		GD.Print("Loading Templates via Filepaths");
		foreach(string folder in TemplatesFilePathsFromRes)
		{
			string path = $"res://Resources/AspectTemplates/{folder}";

			List<string> files = GetAllFilepathsInAllSubfolders(path);

			//load added files
			foreach(string fullPath in files)
			{
				Resource loaded = ResourceLoader.Load(fullPath);
				if(loaded is not AspectTemplate)
				{
					continue;
				}
				
				AspectTemplate loadedTemplate = (AspectTemplate)loaded;

				//only add if this hasn't been added before
				if(!TemplatesById.ContainsKey(loadedTemplate._id))
				{
                    AllTemplates.Add(loadedTemplate);
                    TemplatesById.Add(loadedTemplate._id, loadedTemplate);
                } 
				else 
				{
					GD.PushWarning($"Skipped adding aspect {loadedTemplate._id} due to it's id already being loaded");
				}
				
			}

        }

	}

	/// <summary>
	/// recursively get all files in basePath and return a list of all complete paths
	/// </summary>
	public List<string> GetAllFilepathsInAllSubfolders(string basePath)
	{
		List<string> paths = new List<string>();

        //get all files in this folder
        foreach (string file in DirAccess.GetFilesAt(basePath))
        {
			paths.Add($"{basePath}/{file}");
        }

		//call on all subfolders
		foreach (string dir in DirAccess.GetDirectoriesAt(basePath))
		{
			paths.AddRange(GetAllFilepathsInAllSubfolders($"{basePath}/{dir}"));
		}

		//return
		return paths;
    }



	public static AspectTemplate GetTemplate(string id) =>
		string.IsNullOrEmpty(id) ? null :
		(TemplatesById.TryGetValue(id, out var t) ? t : null);
		
			private static Rarity RollRarity()
	{
		int total = 0;
		foreach (var kv in RarityWeights) total += kv.Value;
		if (total <= 0) return Rarity.Common;

		int roll = (int)_rng.RandiRange(1, total);
		int acc = 0;
		foreach (var kv in RarityWeights)
		{
			acc += kv.Value;
			if (roll <= acc) return kv.Key;
		}
		return Rarity.Common;
	}

	private static List<AspectTemplate> OfRarity(Rarity r) =>
		AllTemplates.FindAll(t => t != null && t.Rarity == r);

	public static AspectTemplate RollOneTemplate(Func<AspectTemplate, bool> predicate = null, int maxAttempts = 20)
	{
		predicate ??= (_)=>true;

		for (int i = 0; i < maxAttempts; i++)
		{
			var rar = RollRarity();
			var pool = OfRarity(rar);
			if (pool.Count == 0) continue;

			var pick = pool[(int)_rng.RandiRange(0, pool.Count - 1)];
			if (predicate(pick)) return pick;
		}


		var any = AllTemplates.FindAll(new Predicate<AspectTemplate>(predicate));

		if (any.Count > 0)
			return any[(int)_rng.RandiRange(0, any.Count - 1)];

		return null;
	}

	public static List<AspectTemplate> RollTemplates(int count, Func<AspectTemplate, bool> predicate)
	{
		var result = new List<AspectTemplate>(count);
		var used   = new HashSet<string>();

		while (result.Count < count)
		{
			var t = RollOneTemplate(tt => predicate(tt) && !used.Contains(tt._id));
			if (t == null) break;
			used.Add(t._id);
			result.Add(t);
		}
		return result;
	}
}
