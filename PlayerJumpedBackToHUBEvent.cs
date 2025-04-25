public struct PlayerJumpedBackToHUBEvent : IPlayerBasedEvent
{
	public Player Player { get; }

	public PlayerJumpedBackToHUBEvent(Player player)
	{
		Player = player;
	}
}
