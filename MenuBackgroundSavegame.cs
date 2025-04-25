using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public class MenuBackgroundSavegame
{
	private static string INGAME_SCENE_NAME = "Ingame";

	[SerializeField]
	private MetaGameMode MenuFallbackMode;

	[SerializeField]
	private TextAsset MenuSavegame;

	public IEnumerator Load()
	{
		PrepareConfig();
		AsyncOperation sceneLoad = SceneManager.LoadSceneAsync(INGAME_SCENE_NAME, LoadSceneMode.Additive);
		sceneLoad.completed += OnSceneLoaded;
		bool loaded = false;
		yield return new WaitUntil(() => loaded);
		void OnSceneLoaded(AsyncOperation op)
		{
			Debug.Log("Background savegame: Scene Loaded");
			Singleton<GameCore>.G.OnGameInitialized.AddListener(delegate
			{
				Debug.Log("Background savegame: Game core loaded");
				loaded = true;
			});
		}
	}

	private void PrepareConfig()
	{
		if (GameEnvironmentManager.FLAG_SAFE_MODE)
		{
			PrepareCleanMenuSavegame();
			return;
		}
		try
		{
			PrepareExistingMenuSavegame();
			return;
		}
		catch (Exception ex)
		{
			Debug.LogWarning("Failed to load background savegame: " + ex);
		}
		PrepareCleanMenuSavegame();
	}

	private void PrepareExistingMenuSavegame()
	{
		using MemoryStream stream = new MemoryStream(MenuSavegame.bytes);
		SavegameBlobReader reader = new SavegameSerializer().ReadFromStream(stream);
		if (reader.Metadata.Version < Savegame.LOWEST_SUPPORTED_VERSION)
		{
			throw new Exception("Menu savegame version " + reader.Metadata.Version + " is below minimum supported version (" + Savegame.LOWEST_SUPPORTED_VERSION + ")");
		}
		Globals.CurrentGameStartOptionsPassOver = new GameStartOptionsContinueExisting
		{
			MenuMode = true,
			SavegameReader = reader,
			UID = "menu"
		};
	}

	private void PrepareCleanMenuSavegame()
	{
		Globals.CurrentGameStartOptionsPassOver = new GameStartOptionsStartNew
		{
			Config = new GameModeConfig(MenuFallbackMode, 0),
			MenuMode = true
		};
	}
}
