using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public class BeltLaneDefinition
{
	public enum ItemFilter : uint
	{
		None = 0u,
		ShapeItem = 10u,
		ShapeOrFluidCrate = 30u,
		ShapeCrate = 40u,
		FluidCrate = 50u
	}

	public const int STEPS_PER_UNIT = 100000;

	public const int ITEM_SPACING_S = 50000;

	public const int ITEM_HALF_SPACING_S = 25000;

	public string Name;

	[Tooltip("Duration in seconds, how long it takes for an item to pass this lane")]
	public float Duration = 0.5f;

	[Tooltip("Length of the lane in world space units")]
	public float Length_W = 0.5f;

	public float3 ItemStartPos_L;

	public float3 ItemEndPos_L;

	public ItemFilter Filter = ItemFilter.None;

	public MetaResearchSpeed Speed = null;

	[NonSerialized]
	public int Length_S;

	[NonSerialized]
	public int Duration_T;

	[NonSerialized]
	public int StepsPerTick_S;

	public float ScaledDuration_NonDeterministic => Duration / ((float)Singleton<GameCore>.G.Research.SpeedManager.GetSpeedValue(Speed) / 100f);

	public void ComputeMetrics(IReadOnlyDictionary<MetaResearchSpeed, int> speeds)
	{
		if (Speed == null)
		{
			throw new Exception("Speed for belt lane " + Name + " is null");
		}
		double length_S = (double)Length_W * 100000.0;
		if (length_S % 1.0 != 0.0)
		{
		}
		Length_S = (int)length_S;
		if (!speeds.TryGetValue(Speed, out var currentSpeed))
		{
			throw new Exception("Speed " + Speed.name + " is not contained in the research tree but required for " + Name);
		}
		double durationSeconds = (double)Duration / ((double)currentSpeed / 100.0);
		double duration_T = durationSeconds * (double)IslandSimulator.UPS;
		Duration_T = (int)duration_T;
		if (Length_S == 0)
		{
			StepsPerTick_S = 0;
			return;
		}
		double stepsPerTick_S = (double)Length_S / (double)Duration_T;
		if (stepsPerTick_S % 1.0 != 0.0)
		{
		}
		StepsPerTick_S = (int)stepsPerTick_S;
	}

	public override string ToString()
	{
		return "{" + Name + "}";
	}

	public int T_From_S(int steps_S)
	{
		if (Length_W == 0f)
		{
			return (steps_S >= 0) ? Duration_T : (-1);
		}
		if (steps_S < 0)
		{
			return -1;
		}
		if (steps_S >= Length_S)
		{
			return Duration_T;
		}
		return steps_S / StepsPerTick_S;
	}

	public int S_From_T(int ticks_T)
	{
		return ticks_T * StepsPerTick_S;
	}

	public float3 GetPosFromTicks_L(int ticks)
	{
		if (ticks < 0 || ticks >= Duration_T)
		{
			throw new Exception("DEV ISSUE: Invalid progress value on " + this?.ToString() + ": " + ticks);
		}
		return math.lerp(ItemStartPos_L, ItemEndPos_L, TicksToProgress_UNSAFE(ticks));
	}

	public bool CheckFilters(BeltItem item)
	{
		return Filter switch
		{
			ItemFilter.ShapeItem => item is ShapeItem, 
			ItemFilter.ShapeOrFluidCrate => item is ShapeCrateItem || item is FluidCrateItem, 
			ItemFilter.ShapeCrate => item is ShapeCrateItem, 
			ItemFilter.FluidCrate => item is FluidCrateItem, 
			ItemFilter.None => true, 
			_ => throw new Exception("Invalid filter on lane " + Name + ":" + Filter), 
		};
	}

	public static float StepsToWorld_UNSAFE(int steps)
	{
		return (float)steps / 100000f;
	}

	public int ProgressToTicks_UNSAFE(float progress)
	{
		return (int)(progress * (float)Duration_T);
	}

	public float TicksToSeconds_UNSAFE(int ticks)
	{
		return (float)ticks / (float)Duration_T * Duration;
	}

	public float TicksToProgress_UNSAFE(int steps)
	{
		return (float)steps / (float)Duration_T;
	}
}
