public interface ITutorialStateReadAccess
{
	bool IsFlagCompleted(TutorialFlag flag);

	bool IsEntryCompleted(ITutorialEntry step);

	bool HasInteractedWithBuilding(MetaBuildingVariant variant);
}
