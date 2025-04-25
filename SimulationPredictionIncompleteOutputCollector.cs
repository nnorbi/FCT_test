using System.Collections.Generic;

public class SimulationPredictionIncompleteOutputCollector : ILazyQueryNodeCollector<BuildingDescriptor>
{
	private readonly IBuildingWorldQuery WorldQuery;

	public HashSet<ContextualBuildingOutput> ContextualBuildingOutputs = new HashSet<ContextualBuildingOutput>();

	public HashSet<BuildingDescriptor> Buildings = new HashSet<BuildingDescriptor>();

	public SimulationPredictionIncompleteOutputCollector(IBuildingWorldQuery worldQuery)
	{
		WorldQuery = worldQuery;
	}

	public void Add(BuildingDescriptor building, int childrenCount)
	{
		if (building.InternalVariant.BeltOutputs.Length == childrenCount)
		{
			return;
		}
		MetaBuildingInternalVariant.BeltIO[] beltOutputs = building.InternalVariant.BeltOutputs;
		foreach (MetaBuildingInternalVariant.BeltIO output in beltOutputs)
		{
			if (!SimulationWorldQueryUtils.TryGetBuildingThatIsReceivingMyOutput(building, output, WorldQuery, out var _))
			{
				ContextualBuildingOutputs.Add(new ContextualBuildingOutput(building, output));
				Buildings.Add(building);
			}
		}
	}
}
