#define UNITY_ASSERTIONS
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

[UsedImplicitly]
public class TunnelExitOutputPredictor : BuildingOutputPredictor
{
	public override bool OverrideInputDependency(BuildingDescriptor descriptor, IBuildingWorldQuery worldQuery, ManagedFixedBuffer<BuildingDescriptor> fixedBuffer)
	{
		Debug.Assert(descriptor.TryGetDescribedEntity<TunnelExitEntity>(out var _), "Why the tunnel exit prediction is being required for a non tunnel exit building?");
		Debug.Assert(descriptor.Island is TunnelExitIsland);
		TunnelEntranceIsland tunnelEntrance = TunnelEntranceIsland.Tunnels_FindEntrance(descriptor.Island.Map, descriptor.Island.Origin_GC, descriptor.Island.Metadata.LayoutRotation);
		if (tunnelEntrance == null)
		{
			return true;
		}
		fixedBuffer.Add(new BuildingDescriptor(tunnelEntrance.Buildings.Buildings.First()));
		return true;
	}

	public override void Predict(BuildingDescriptor descriptor, SimulationPredictionInputSetCombination predictionInputSet, SimulationPredictionOutputPredictionWriter outputPredictionWriter)
	{
		foreach (SimulationPredictionInputCombinationMap combination2 in predictionInputSet.Combinations)
		{
			foreach (KeyValuePair<int, ShapeItem> predictedItem in combination2.IndexToPredictedItemMap)
			{
				outputPredictionWriter.PushOutputAt(predictedItem.Key, predictedItem.Value);
			}
		}
	}
}
