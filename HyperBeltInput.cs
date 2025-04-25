using System;
using Unity.Collections;

public struct HyperBeltInput : IDisposable
{
	public bool StartedPlacement;

	public bool ConfirmPlacement;

	public Grid.Direction StartDirection;

	public Grid.Direction Rotation;

	public NativeList<PathSegment> Segments;

	public GlobalChunkCoordinate StartTile;

	public void Dispose()
	{
		Segments.Dispose();
	}
}
