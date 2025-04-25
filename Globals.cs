using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public static class Globals
{
	public static Keybindings Keybindings;

	public static LocalizationManager Localization;

	public static GameSettings Settings;

	public static IStorePlatformSDK StoreSDK;

	public static GameAnalytics Analytics;

	public static GameStartOptions CurrentGameStartOptionsPassOver;

	public static UISounds UISounds;

	public static GameResources Resources;

	public static ISavegameManager Savegames;

	private static bool Terminated = false;

	public static UnityEvent OnInitialized = new UnityEvent();

	public static bool Initialized { get; private set; } = false;

	private static void LogDefines()
	{
		Debug.LogWarning("Core:: DEVELOPMENT_BUILD: ENABLED");
		Debug.Log("Core:: UNITY_EDITOR: DISABLED");
		Debug.Log("Core:: SPZ_PLATFORM_STEAM: ENABLED");
		Debug.Log("Core:: SPZ_EDITION_FULL: ENABLED");
		Debug.Log("Core:: SPZ_EDITION_DEMO: DISABLED");
	}

	public static void InitSettings()
	{
		if (Settings == null)
		{
			Settings = new GameSettings();
			Settings.InitAndLoad();
		}
	}

	public static IEnumerable<string> Init()
	{
		if (Initialized)
		{
			yield break;
		}
		yield return "Loading defines";
		LogDefines();
		Application.quitting += OnApplicationQuit;
		Initialized = true;
		yield return "Init DOTWeen";
		DOTween.Init();
		yield return "Reading build date";
		GameEnvironmentManager.ReadBuildDate();
		yield return "Preparing statics";
		BlueprintSerializer.InitJsonConfig();
		IslandChunk.InitShaderInputs();
		yield return "Preparing HUDTheme";
		HUDTheme.Init();
		yield return "Init keybindings";
		Keybindings = new Keybindings();
		yield return "Init savegames";
		Savegames = new SavegameManager();
		yield return "Creating savegames folder";
		Savegames.CreateSavegamesFolder();
		yield return "Loading settings";
		InitSettings();
		yield return "Creating localization";
		Localization = new LocalizationManager();
		yield return "Loading translations";
		Localization.LoadTranslators();
		yield return "Loading language";
		Localization.TryLoadLanguage(Settings.General.Language.Value);
		yield return "Setting language '" + Localization.CurrentTranslator.LanguageTitle + "' as current";
		Settings.General.Language.SetValue(Localization.CurrentTranslator.LanguageCode);
		yield return "Init analytics";
		Analytics = new GameAnalytics();
		CurrentGameStartOptionsPassOver = null;
		yield return "Loading global resources";
		Resources = UnityEngine.Resources.Load<GameResources>("GameResources");
		yield return "Loading global scene";
		SceneManager.LoadScene("Globals", LoadSceneMode.Additive);
		yield return "Global scene loaded";
		GameObject globalObject = GameObject.Find("$Globals");
		yield return "Loading UI sound manager";
		UISounds = globalObject.transform.Find("$UISounds")?.GetComponent<UISounds>();
		if (UISounds == null)
		{
			throw new Exception("UI Sounds not found.");
		}
		yield return "Loading resources preloader";
		GameResourcesPreloader preloader = globalObject.transform.Find("$GameResourcesPreloader")?.GetComponent<GameResourcesPreloader>();
		if (preloader == null)
		{
			throw new Exception("GameResourcesPreloader not found.");
		}
		yield return "Preloading resources";
		foreach (string item in preloader.Preload())
		{
			yield return item;
		}
		Resources = preloader.GameResources;
		yield return "Init building animations";
		BuildingAnimations.InitMaterialBlocks(Resources);
		yield return "Globals initialized.";
		OnInitialized.InvokeAndClear();
		OnInitialized = null;
	}

	private static void OnApplicationQuit()
	{
		if (!Terminated)
		{
			Terminated = true;
			StoreSDK?.Shutdown();
		}
	}
}
