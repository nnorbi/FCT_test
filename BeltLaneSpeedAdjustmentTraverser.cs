using System.Collections.Generic;
using UnityEngine;

public class BeltLaneSpeedAdjustmentTraverser : IBeltLaneTraverser
{
	public struct SpeedDelta
	{
		public int OldSpeed;

		public int NewSpeed;
	}

	protected HashSet<BeltLane> SeenLanes = new HashSet<BeltLane>();

	protected Dictionary<MetaResearchSpeed, SpeedDelta> Adjustments;

	public BeltLaneSpeedAdjustmentTraverser(Dictionary<MetaResearchSpeed, SpeedDelta> adjustments)
	{
		Adjustments = adjustments;
	}

	public void Traverse(BeltLane lane)
	{
		if (SeenLanes.Contains(lane))
		{
			Debug.LogWarning("BeltLaneSpeedAdjustmentTraverser:: Double traversal of " + lane.Definition.Name);
			return;
		}
		SeenLanes.Add(lane);
		if (lane.HasItem && Adjustments.TryGetValue(lane.Definition.Speed, out var adjustment))
		{
			long progressOld_T = lane.Progress_T;
			long progressNew_T = progressOld_T * adjustment.NewSpeed / adjustment.OldSpeed;
			lane.Progress_T = (int)progressNew_T;
		}
	}
}
