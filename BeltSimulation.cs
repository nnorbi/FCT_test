using System;
using UnityEngine;

public static class BeltSimulation
{
	public static bool UpdateLane(TickOptions options, BeltLane lane)
	{
		return UpdateLane(lane, options.DeltaTicks_T);
	}

	public static bool UpdateLane(BeltLane lane, int delta_T)
	{
		if (delta_T < 0)
		{
			throw new Exception("Negative delta: " + delta_T + " for " + lane);
		}
		int maxStep_S = lane.ComputeMaxStepWhenEmptyINTERNAL_S();
		if (lane.Item == null)
		{
			lane.MaxStep_S = maxStep_S;
			return false;
		}
		BeltLaneDefinition definition = lane.Definition;
		BeltLane nextLane = lane.NextLane;
		if (maxStep_S < 0)
		{
			Debug.LogError("maxStep_S < 0 but have item! Lane=" + lane?.ToString() + " next=" + nextLane?.ToString() + " maxStep_S=" + maxStep_S);
			lane.Progress_T = 0;
			lane.MaxStep_S = 0;
			return false;
		}
		int laneLength_S = definition.Length_S;
		int laneDuration_T = definition.Duration_T;
		if (maxStep_S >= laneLength_S && nextLane != null && !nextLane.HasItem)
		{
			lane.Progress_T += delta_T;
			if (lane.Progress_T >= laneDuration_T)
			{
				if (nextLane.HasItem)
				{
					throw new Exception("Next lane has item but announced space: Lane=" + lane?.ToString() + " Next=" + nextLane?.ToString() + " maxStep_S=" + maxStep_S);
				}
				if (TransferToLane(lane.Item, nextLane, lane.Progress_T - laneDuration_T))
				{
					lane.ClearLane();
					return true;
				}
				lane.Progress_T = laneDuration_T - 1;
				UpdateLaneMaxProgressWithItem(lane);
				return false;
			}
			UpdateLaneMaxProgressWithItem(lane);
			return false;
		}
		int maxTicks_T = FastMath.Min(lane.Definition.T_From_S(maxStep_S), laneDuration_T - 1);
		if (lane.Progress_T > maxTicks_T)
		{
			if (lane.NextLane == null)
			{
				lane.ClearLane();
				return false;
			}
			lane.ClearLane();
			return false;
		}
		lane.Progress_T = FastMath.Min(lane.Progress_T + delta_T, maxTicks_T);
		UpdateLaneMaxProgressWithItem(lane);
		return false;
	}

	public static void UpdateLaneMaxProgressWithItem(BeltLane lane)
	{
		int steps_S = lane.Progress_T * lane.Definition.StepsPerTick_S;
		lane.MaxStep_S = FastMath.Min(0, steps_S - 50000);
	}

	public static bool TransferToLane(BeltItem itemToTransfer, BeltLane nextLane, int remainingTicks_T)
	{
		if (nextLane.HasItem)
		{
			return false;
		}
		if (itemToTransfer == null)
		{
			throw new Exception("Item to transfer to " + nextLane?.ToString() + " but lane is empty!");
		}
		if (nextLane.MaxStep_S < 0)
		{
			throw new Exception("Can't transfer to lane, MaxStep_S < 0" + nextLane);
		}
		BeltLaneDefinition nextDefinition = nextLane.Definition;
		if (!nextDefinition.CheckFilters(itemToTransfer))
		{
			return false;
		}
		BeltLane.PreAcceptHookDelegate preAcceptHook = nextLane.PreAcceptHook;
		if (preAcceptHook != null)
		{
			itemToTransfer = preAcceptHook(itemToTransfer);
			if (itemToTransfer == null)
			{
				return false;
			}
		}
		nextLane.Item = itemToTransfer;
		nextLane.Progress_T = 0;
		nextLane.MaxStep_S = -2121212121;
		if (nextLane.PostAcceptHook != null)
		{
			int newRemainingTicks = remainingTicks_T;
			nextLane.PostAcceptHook(nextLane, ref newRemainingTicks);
			if (newRemainingTicks > remainingTicks_T)
			{
				throw new Exception("Post accept hook must not increase remaining ticks: " + newRemainingTicks + " from " + remainingTicks_T + " on " + nextLane);
			}
			if (newRemainingTicks < 0)
			{
				throw new Exception("Post accept hook must not decrease remaining ticks below 0: " + newRemainingTicks + " from " + remainingTicks_T + " on " + nextLane);
			}
			remainingTicks_T = newRemainingTicks;
		}
		if (nextLane.HasItem)
		{
			UpdateLane(nextLane, remainingTicks_T);
		}
		if (nextLane.MaxStep_S == -2121212121)
		{
			Debug.LogError("Lane " + nextLane?.ToString() + " MaxProgress_W has not been updated after accepting item! This is most likely caused by the post accept hook consuming the item but not setting the MaxProgress_W property afterwards. ");
		}
		return true;
	}
}
