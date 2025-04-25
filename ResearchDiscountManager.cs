using System;

public class ResearchDiscountManager : IDisposable
{
	private readonly ResearchProgress Progress;

	public int BlueprintBuildingDiscount { get; private set; }

	public ResearchDiscountManager(ResearchProgress researchProgress)
	{
		Progress = researchProgress;
		Progress.OnChanged.AddListener(ComputeBlueprintBuildingDiscount);
	}

	public void Dispose()
	{
		Progress.OnChanged.RemoveListener(ComputeBlueprintBuildingDiscount);
	}

	public void Initialize()
	{
		ComputeBlueprintBuildingDiscount();
	}

	private void ComputeBlueprintBuildingDiscount()
	{
		BlueprintBuildingDiscount = 0;
		foreach (MetaResearchable researchable in Progress.UnlockedResearchables)
		{
			BlueprintBuildingDiscount += (int)researchable.BlueprintDiscount;
		}
	}
}
