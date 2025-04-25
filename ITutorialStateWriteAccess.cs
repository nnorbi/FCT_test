public interface ITutorialStateWriteAccess
{
	bool TryCompleteEntry(ITutorialEntry step);

	bool TryUncompleteEntry(ITutorialEntry step);

	bool TryCompleteFlag(TutorialFlag flag);

	bool TryMarkInteractedWithBuilding(MetaBuildingVariant buildingVariant);
}
