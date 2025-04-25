public interface IStorePlatformSDK
{
	void Init();

	void UnlockAchievement(string achievementId);

	bool HasSupportForCommunityContent();

	void Shutdown();
}
