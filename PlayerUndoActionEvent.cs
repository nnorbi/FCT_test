public struct PlayerUndoActionEvent : IPlayerBasedEvent
{
	public Player Player { get; }

	public PlayerUndoActionEvent(Player player)
	{
		Player = player;
	}
}
