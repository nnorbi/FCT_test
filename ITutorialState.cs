public interface ITutorialState : ITutorialStateReadAccess, ITutorialStateWriteAccess
{
	TutorialStateSerializedData Serialize();

	void Deserialize(TutorialStateSerializedData data);

	void Reset();
}
