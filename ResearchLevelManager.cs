#define UNITY_ASSERTIONS
using System;
using UnityEngine;

public class ResearchLevelManager
{
	private ResearchProgress Progress;

	private ResearchTreeHandle Tree;

	public ResearchLevelHandle CurrentLevel { get; private set; }

	public ResearchLevelManager(ResearchProgress progress, ResearchTreeHandle tree)
	{
		Progress = progress;
		Tree = tree;
	}

	public void Initialize()
	{
		ComputeCurrentLevel();
		Progress.OnChanged.AddListener(ComputeCurrentLevel);
	}

	private void ComputeCurrentLevel()
	{
		CurrentLevel = null;
		ResearchLevelHandle[] levels = Tree.Levels;
		foreach (ResearchLevelHandle node in levels)
		{
			if (!Progress.IsUnlocked(node))
			{
				CurrentLevel = node;
				return;
			}
		}
		Debug.LogWarning("No current level found, falling back to last.");
		CurrentLevel = Tree.Levels[^1];
	}

	public bool IsLevelRightAfter(MetaResearchable level)
	{
		int indexNow = Array.IndexOf(Tree.Levels, CurrentLevel);
		int indexLevel = Array.FindIndex(Tree.Levels, (ResearchLevelHandle f) => f.Meta == level);
		Debug.Assert(indexNow >= 0, "not a level: " + CurrentLevel?.Meta.name);
		Debug.Assert(indexLevel >= 0, "not a level: " + level?.name);
		return indexNow == indexLevel + 1;
	}

	public void EnsureInitialLevelUnlocked()
	{
		Progress.Unlock(Tree.Levels[0]);
	}
}
