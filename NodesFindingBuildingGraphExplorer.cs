using System.Collections.Generic;

internal class NodesFindingBuildingGraphExplorer : IDirectedGraphExplorer<BuildingDescriptor>
{
	private readonly IDirectedGraphExplorer<BuildingDescriptor> BaseExplorer;

	private readonly HashSet<BuildingDescriptor> NodesToBeFound;

	public NodesFindingBuildingGraphExplorer(IDirectedGraphExplorer<BuildingDescriptor> baseExplorer, HashSet<BuildingDescriptor> nodesToBeFound)
	{
		BaseExplorer = baseExplorer;
		NodesToBeFound = nodesToBeFound;
	}

	public void GetAllIncomingNodes(BuildingDescriptor current, ManagedFixedBuffer<BuildingDescriptor> fixedBuffer)
	{
		NodesToBeFound.Remove(current);
		if (NodesToBeFound.Count != 0)
		{
			BaseExplorer.GetAllIncomingNodes(current, fixedBuffer);
		}
	}

	public void GetAllOutgoingNodes(BuildingDescriptor current, ManagedFixedBuffer<BuildingDescriptor> fixedBuffer)
	{
		NodesToBeFound.Remove(current);
		if (NodesToBeFound.Count != 0)
		{
			BaseExplorer.GetAllOutgoingNodes(current, fixedBuffer);
		}
	}
}
