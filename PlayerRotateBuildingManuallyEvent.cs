public struct PlayerRotateBuildingManuallyEvent : IPlayerBasedEvent
{
	public Player Player { get; }

	public PlayerRotateBuildingManuallyEvent(Player player)
	{
		Player = player;
	}
}
