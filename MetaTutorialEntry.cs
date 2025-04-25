using System;
using UnityEngine;
using UnityEngine.Video;

public abstract class MetaTutorialEntry : ScriptableObject
{
	[Space(20f)]
	public VideoClip Video;

	[Space(20f)]
	public bool SpecificForLevel = false;

	public MetaResearchable Level;

	[Space(20f)]
	public MetaResearchable[] DependentResearch;

	public EditorClassIDSingleton<ITutorialCondition>[] ShowConditions = new EditorClassIDSingleton<ITutorialCondition>[0];

	[Space(20f)]
	public EditorClassIDSingleton<ITutorialCondition>[] CompleteConditions = new EditorClassIDSingleton<ITutorialCondition>[0];

	public bool CanInvalidateAfterCompletion = false;

	[Space(20f)]
	public bool CompleteWhenResearchComplete = false;

	public MetaResearchable CompleteWhenResearch;

	[Space(20f)]
	public MetaBuildingVariant[] HighlightedBuildingVariants = new MetaBuildingVariant[0];

	public MetaIslandLayout[] HighlightedLayouts = new MetaIslandLayout[0];

	[ValidateKeybinding]
	public string[] HighlightedKeybindings = new string[0];

	[NonSerialized]
	[TextArea(12, 40)]
	[Space(40f)]
	private string _Summary;

	public void OnValidate()
	{
		string result = "";
		result += "SHOWN IF ALL APPLY: \n";
		bool anyShowCondition = false;
		if (SpecificForLevel)
		{
			string levelName = Level?.name ?? "INVALID LEVEL";
			if (!(this is MetaTutorialGroup))
			{
				result += "- Parent group is shown & not completed.\n";
			}
			result = result + "- Level '" + levelName + "' is unlocked\n";
			result = result + "- The level after '" + levelName + "' is the current level\n";
			anyShowCondition = true;
		}
		EditorClassIDSingleton<ITutorialCondition>[] showConditions = ShowConditions;
		foreach (EditorClassIDSingleton<ITutorialCondition> condition in showConditions)
		{
			result = result + "- " + condition.ClassID + "\n";
			anyShowCondition = true;
		}
		MetaResearchable[] dependentResearch = DependentResearch;
		foreach (MetaResearchable research in dependentResearch)
		{
			result = result + "- " + research.name + " is unlocked\n";
			anyShowCondition = true;
		}
		if (!anyShowCondition)
		{
			result += "- Always (until completed)\n";
		}
		result += "\nCOMPLETED WHEN ONE OF THIS APPLIES:\n";
		bool anyCompleteCondition = false;
		if (CompleteWhenResearchComplete)
		{
			result = result + "- " + (CompleteWhenResearch?.name ?? "INVALID RESEARCH") + " is unlocked\n";
			anyCompleteCondition = true;
		}
		if (SpecificForLevel)
		{
			string levelName2 = Level?.name ?? "INVALID LEVEL";
			result = result + "- The level after '" + levelName2 + "' is completed\n";
			anyCompleteCondition = true;
		}
		if (CompleteConditions.Length != 0)
		{
			result += "- All of the following conditions are fullfilled:\n";
			anyCompleteCondition = true;
			EditorClassIDSingleton<ITutorialCondition>[] completeConditions = CompleteConditions;
			foreach (EditorClassIDSingleton<ITutorialCondition> condition2 in completeConditions)
			{
				result = result + "   - " + condition2.ClassID + "\n";
			}
		}
		if (!anyCompleteCondition)
		{
			result = ((!(this is MetaTutorialGroup)) ? (result + "- Can never be completed, but will be hidden if parent group is hidden.") : (result + "- Can never be completed (!!)"));
		}
		if (CanInvalidateAfterCompletion)
		{
			result += "\n\nCan invalidate after completion.";
		}
		_Summary = result;
	}
}
