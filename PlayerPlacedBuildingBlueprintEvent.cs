public struct PlayerPlacedBuildingBlueprintEvent : IPlayerBasedEvent
{
	public readonly IBlueprint Blueprint;

	public Player Player { get; }

	public PlayerPlacedBuildingBlueprintEvent(Player player, IBlueprint blueprint)
	{
		Player = player;
		Blueprint = blueprint;
	}
}
