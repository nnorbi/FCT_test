public struct PlayerMirroredRegularBuildingEvent : IPlayerBasedEvent
{
	public Player Player { get; }

	public PlayerMirroredRegularBuildingEvent(Player player)
	{
		Player = player;
	}
}
