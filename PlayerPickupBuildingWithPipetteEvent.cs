public struct PlayerPickupBuildingWithPipetteEvent : IPlayerBasedEvent
{
	public readonly MetaBuildingVariant Variant;

	public Player Player { get; }

	public PlayerPickupBuildingWithPipetteEvent(Player player, MetaBuildingVariant variant)
	{
		Player = player;
		Variant = variant;
	}
}
