public struct Checkpoint<TCoordinate>
{
	public bool PreferHorizontalAxis;

	public TCoordinate Position { get; }

	public Checkpoint(bool preferHorizontalAxis, TCoordinate position)
	{
		PreferHorizontalAxis = preferHorizontalAxis;
		Position = position;
	}
}
