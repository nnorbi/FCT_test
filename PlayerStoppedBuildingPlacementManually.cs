public struct PlayerStoppedBuildingPlacementManually : IPlayerBasedEvent
{
	public Player Player { get; }

	public PlayerStoppedBuildingPlacementManually(Player player)
	{
		Player = player;
	}
}
