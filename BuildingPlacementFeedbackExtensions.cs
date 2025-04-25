public static class BuildingPlacementFeedbackExtensions
{
	public static bool RequiresForce(this BuildingPlacementFeedback placementFeedback)
	{
		return placementFeedback == BuildingPlacementFeedback.WontBePlacedBecauseAltersFactory;
	}
}
