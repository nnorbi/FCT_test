#define UNITY_ASSERTIONS
using System;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

[UsedImplicitly]
public class TunnelEntranceOutputPredictor : NoOutputPredictor
{
	public override bool OverrideOutputDependency(BuildingDescriptor descriptor, IBuildingWorldQuery worldQuery, ManagedFixedBuffer<BuildingDescriptor> fixedBuffer)
	{
		Debug.Assert(descriptor.TryGetDescribedEntity<TunnelEntranceEntity>(out var _), "Why the tunnel entrance prediction is being required for a non tunnel entrance building?");
		if (!(descriptor.Island is TunnelEntranceIsland tunnelEntrance))
		{
			throw new Exception();
		}
		fixedBuffer.Add(new BuildingDescriptor(tunnelEntrance.CachedExit.Buildings.Buildings.First()));
		return true;
	}
}
