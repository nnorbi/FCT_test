public struct PlayerOpenedWikiEvent : IPlayerBasedEvent
{
	public Player Player { get; }

	public PlayerOpenedWikiEvent(Player player)
	{
		Player = player;
	}
}
