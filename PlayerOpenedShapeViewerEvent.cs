public struct PlayerOpenedShapeViewerEvent : IPlayerBasedEvent
{
	public Player Player { get; }

	public PlayerOpenedShapeViewerEvent(Player player)
	{
		Player = player;
	}
}
