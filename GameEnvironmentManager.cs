using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public static class GameEnvironmentManager
{
	public static string BUILD_DATE { get; private set; } = "?";

	public static string VERSION => Application.version;

	public static bool IS_DEMO => EDITION == GameEdition.Demo;

	public static GameEdition EDITION => GameEdition.Full;

	public static GameEnvironment ENVIRONMENT
	{
		get
		{
			if (Application.isEditor)
			{
				return GameEnvironment.Dev;
			}
			return GameEnvironment.Stage;
		}
	}

	public static GameStore STORE => GameStore.Steam;

	public static string DETAILED_VERSION => VERSION + (IS_DEMO ? " [demo]" : "") + " | " + SystemInfo.operatingSystem + " | " + SystemInfo.graphicsDeviceType.ToString() + " | " + STORE.ToString() + " | " + BUILD_DATE + (ModsLoader.Patched ? $" | Patched | {ModsLoader.LoadedModsCount} mods" : "") + (FLAG_SAFE_MODE ? " | [Safe Mode]" : "");

	public static string DATA_PATH => Application.persistentDataPath;

	public static string SAVEGAME_PATH => Path.Join(DATA_PATH, "savegames");

	public static string BLUEPRINT_LIBRARY_PATH => Path.Join(DATA_PATH, "blueprints");

	public static string PATCHERS_PATH => Path.Join(DATA_PATH, "patchers");

	public static string PATCHERS_DEPENDENCIES_PATH => Path.Join(PATCHERS_PATH, "deps");

	public static string MODS_PATH => Path.Join(DATA_PATH, "mods");

	public static bool FLAG_SAFE_MODE => IsCommandLineFlagSet("--safe-mode");

	public static bool FLAG_IGNORE_HARDWARE_CHECKS => IsCommandLineFlagSet("--ignore-hw-checks");

	public static bool FLAG_CLEAN_START => IsCommandLineFlagSet("--reset-all-prefs");

	public static bool FLAG_DISABLE_STORE_SDK => IsCommandLineFlagSet("--disable-store-sdk");

	public static bool FLAG_CUSTOM_TRANSLATION => IsCommandLineFlagSet("--custom-translation");

	public static void ReadBuildDate()
	{
		try
		{
			string date = Resources.Load<TextAsset>("BuildDate")?.text ?? "??";
			date = Regex.Replace(date, "[^\\u0020-\\u007E]", string.Empty).Trim();
			BUILD_DATE = date;
			Debug.Log("Environment:: Build date: " + BUILD_DATE);
		}
		catch (Exception ex)
		{
			Debug.LogError("Failed to read build date: " + ex);
			BUILD_DATE = "???";
		}
	}

	private static bool IsCommandLineFlagSet(string flag)
	{
		string[] flags = Environment.GetCommandLineArgs();
		return flags.Any((string f) => f.ToLower().Trim().StartsWith(flag.ToLower()));
	}
}
