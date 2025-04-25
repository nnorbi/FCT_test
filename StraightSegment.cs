#define UNITY_ASSERTIONS
using Unity.Mathematics;
using UnityEngine;

public struct StraightSegment
{
	public GlobalChunkCoordinate Start;

	public GlobalChunkCoordinate End;

	public StraightSegment(GlobalChunkCoordinate start, GlobalChunkCoordinate end)
	{
		Start = start;
		End = end;
		Debug.Assert(start != end);
	}

	public Grid.Direction GetDirection()
	{
		return Grid.OffsetToDirection((int2)(End - Start));
	}
}
