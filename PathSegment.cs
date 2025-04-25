public struct PathSegment
{
	public GlobalChunkCoordinate Start;

	public GlobalChunkCoordinate End;

	public bool PreferHorizontalAxis;

	public PathSegment(GlobalChunkCoordinate start, GlobalChunkCoordinate end, bool preferHorizontalAxis)
	{
		Start = start;
		End = end;
		PreferHorizontalAxis = preferHorizontalAxis;
	}
}
