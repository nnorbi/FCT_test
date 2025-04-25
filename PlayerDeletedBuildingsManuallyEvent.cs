public struct PlayerDeletedBuildingsManuallyEvent
{
	public readonly Player Player;

	public readonly int Count;

	public PlayerDeletedBuildingsManuallyEvent(Player player, int count)
	{
		Player = player;
		Count = count;
	}
}
