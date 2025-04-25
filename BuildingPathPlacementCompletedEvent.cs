public struct BuildingPathPlacementCompletedEvent
{
	public readonly Player Player;

	public readonly int CheckpointCount;

	public BuildingPathPlacementCompletedEvent(Player player, int checkpointCount)
	{
		Player = player;
		CheckpointCount = checkpointCount;
	}
}
