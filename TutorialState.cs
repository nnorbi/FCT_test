using System;
using System.Collections.Generic;
using System.Linq;

public class TutorialState : ITutorialState, ITutorialStateReadAccess, ITutorialStateWriteAccess
{
	private HashSet<TutorialFlag> Flags = new HashSet<TutorialFlag>();

	private HashSet<string> CompletedEntryIds = new HashSet<string>();

	private HashSet<string> InteractedBuildingIds = new HashSet<string>();

	public bool IsFlagCompleted(TutorialFlag flag)
	{
		return Flags.Contains(flag);
	}

	public bool IsEntryCompleted(ITutorialEntry step)
	{
		return CompletedEntryIds.Contains(step.Id);
	}

	public bool TryCompleteEntry(ITutorialEntry step)
	{
		if (CompletedEntryIds.Add(step.Id))
		{
			return true;
		}
		return false;
	}

	public bool TryUncompleteEntry(ITutorialEntry step)
	{
		if (CompletedEntryIds.Remove(step.Id))
		{
			return true;
		}
		return false;
	}

	public bool HasInteractedWithBuilding(MetaBuildingVariant buildingVaraint)
	{
		return InteractedBuildingIds.Contains(buildingVaraint.name);
	}

	public TutorialStateSerializedData Serialize()
	{
		return new TutorialStateSerializedData
		{
			Flags = Flags.Select((TutorialFlag f) => f.ToString()).ToArray(),
			CompletedStepIds = CompletedEntryIds.ToArray(),
			InteractedBuildingsId = InteractedBuildingIds.ToArray()
		};
	}

	public void Deserialize(TutorialStateSerializedData data)
	{
		TutorialFlag result;
		HashSet<TutorialFlag> flags = (from flag in data.Flags ?? new string[0]
			select Enum.TryParse<TutorialFlag>(flag, out result) ? new TutorialFlag?(result) : ((TutorialFlag?)null) into s
			where s.HasValue
			select s.Value).ToHashSet();
		Flags = flags;
		CompletedEntryIds = new HashSet<string>(data.CompletedStepIds ?? new string[0]);
		InteractedBuildingIds = new HashSet<string>(data.InteractedBuildingsId ?? new string[0]);
	}

	public bool TryCompleteFlag(TutorialFlag flag)
	{
		if (Flags.Add(flag))
		{
			return true;
		}
		return false;
	}

	public bool TryMarkInteractedWithBuilding(MetaBuildingVariant buildingVariant)
	{
		if (InteractedBuildingIds.Add(buildingVariant.name))
		{
			return true;
		}
		return false;
	}

	public void Reset()
	{
		Flags.Clear();
		CompletedEntryIds.Clear();
		InteractedBuildingIds.Clear();
	}
}
