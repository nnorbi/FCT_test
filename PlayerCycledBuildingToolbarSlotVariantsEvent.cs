public struct PlayerCycledBuildingToolbarSlotVariantsEvent : IPlayerBasedEvent
{
	public Player Player { get; }

	public PlayerCycledBuildingToolbarSlotVariantsEvent(Player player)
	{
		Player = player;
	}
}
