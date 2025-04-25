public struct PlayerUsedAreaDeleteEvent
{
	public readonly Player Player;

	public readonly int Count;

	public PlayerUsedAreaDeleteEvent(Player player, int count)
	{
		Player = player;
		Count = count;
	}
}
