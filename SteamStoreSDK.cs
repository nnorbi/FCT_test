using Steamworks;
using Steamworks.Data;
using UnityEngine;

public class SteamStoreSDK : IStorePlatformSDK
{
	protected const uint STEAM_GAME_ID = 2162800u;

	public void Init()
	{
		SteamClient.Init(2162800u);
	}

	public void UnlockAchievement(string achievementId)
	{
		Achievement achievement = new Achievement(achievementId);
		if (achievement.Trigger())
		{
			Debug.Log($"Achievement {achievement} unlocked successfully");
		}
		else
		{
			Debug.LogError($"Something went wrong when unlocking the achievement {achievement}");
		}
	}

	public bool HasSupportForCommunityContent()
	{
		return true;
	}

	public void Shutdown()
	{
		SteamClient.Shutdown();
	}
}
