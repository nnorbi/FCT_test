public struct HyperBeltEndpoint
{
	public GlobalChunkCoordinate Position;

	public Grid.Direction InputDirection;

	public Grid.Direction OutputDirection;

	public HyperBeltEndpoint(GlobalChunkCoordinate position, Grid.Direction inputDirection, Grid.Direction outputDirection)
	{
		Position = position;
		InputDirection = inputDirection;
		OutputDirection = outputDirection;
	}
}
