using System;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "RTDefaultResearchTree", menuName = "Metadata/Research/Research Tree")]
public class MetaResearchTree : ScriptableObject
{
	[Serializable]
	public struct SideGoalDefinition
	{
		public MetaResearchable SideGoal;

		public ResearchUnlockCost Cost;
	}

	[Serializable]
	public struct LevelDefinition
	{
		[Space(20f)]
		public MetaResearchable Level;

		[Tooltip("The cost to unlock the *next* level.")]
		public ResearchUnlockCost NextLevelCost;

		public SideGoalDefinition[] SideGoals;
	}

	[Tooltip("All speeds, including their initial speed")]
	public EditorDict<MetaResearchSpeed, int> Speeds;

	[Header("Tree")]
	[Space(20f)]
	[SerializeField]
	[RequiredListLength(1, null)]
	public LevelDefinition[] Levels;

	public void OnValidate()
	{
		LevelDefinition[] levels = Levels;
		for (int i = 0; i < levels.Length; i++)
		{
			LevelDefinition level = levels[i];
			level.NextLevelCost.Validate();
			SideGoalDefinition[] sideGoals = level.SideGoals;
			for (int j = 0; j < sideGoals.Length; j++)
			{
				SideGoalDefinition sideGoal = sideGoals[j];
				sideGoal.Cost.Validate();
			}
		}
		ResearchTreeHandle.FromResearchTree(this);
	}
}
