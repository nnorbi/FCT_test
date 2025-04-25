public struct PlayerRedoActionEvent : IPlayerBasedEvent
{
	public Player Player { get; }

	public PlayerRedoActionEvent(Player player)
	{
		Player = player;
	}
}
