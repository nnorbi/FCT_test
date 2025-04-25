#define UNITY_ASSERTIONS
using System;
using System.Linq;
using UnityEngine;

public class Savegame
{
	[Serializable]
	public class Metadata
	{
		public int Version;

		public string AppSourceVersion;

		public string AppSourceEnvironment;

		public string AppSourceStore;

		public bool BinaryDataCheckpoints;

		public float TotalPlaytime;

		public float ResearchProgress;

		public int StructureCount;

		public bool CheatsUsed;

		public DateTime LastSaved;

		public string ToDebugString()
		{
			return $"Version {Version}\nSourceVersion {AppSourceVersion}\nSourceEnvironment {AppSourceEnvironment}\nSourceStore {AppSourceStore}\nCheckpoints {BinaryDataCheckpoints}\nLastSaved {LastSaved}\nTotalPlaytime {TotalPlaytime}\nResearchProgress {ResearchProgress}\nStructureCount {StructureCount}";
		}
	}

	[Serializable]
	public class SerializedMetadata : Metadata
	{
		public GameModeConfig.SerializedData GameMode;
	}

	public static int VERSION;

	public static int LOWEST_SUPPORTED_VERSION;

	public static string META_FILENAME;

	[NonSerialized]
	public Metadata Meta;

	[NonSerialized]
	public GameModeConfig ModeConfig;

	static Savegame()
	{
		VERSION = 1031;
		LOWEST_SUPPORTED_VERSION = 1029;
		META_FILENAME = "savegame.json";
		Debug.Assert(VERSION >= LOWEST_SUPPORTED_VERSION, "lowest version > version");
	}

	public static bool IsCompatible(SerializedMetadata metadata)
	{
		if (metadata.Version < LOWEST_SUPPORTED_VERSION || metadata.Version > VERSION)
		{
			return false;
		}
		if (Globals.Resources.SupportedGameModes.All((MetaGameMode mode) => mode.name != metadata.GameMode.GameModeId))
		{
			return false;
		}
		return true;
	}

	public static Savegame CreateNew(GameModeConfig mode)
	{
		Metadata meta = new Metadata
		{
			Version = VERSION,
			AppSourceVersion = GameEnvironmentManager.VERSION,
			AppSourceEnvironment = GameEnvironmentManager.ENVIRONMENT.ToString(),
			AppSourceStore = GameEnvironmentManager.STORE.ToString(),
			BinaryDataCheckpoints = true,
			LastSaved = DateTime.Now,
			TotalPlaytime = 0f,
			CheatsUsed = false
		};
		return new Savegame(meta, mode);
	}

	public static Savegame CreateFromReader(SavegameBlobReader reader)
	{
		return new Savegame(reader.Metadata, GameModeConfig.Deserialize(reader.Metadata.GameMode));
	}

	public static SerializedMetadata DeserializeMetadata(SavegameBlobReader reader)
	{
		return reader.ReadObjectFromJson<SerializedMetadata>(META_FILENAME);
	}

	public Savegame(Metadata meta, GameModeConfig mode)
	{
		Meta = meta;
		ModeConfig = mode;
	}

	public void SerializeMetadata(SavegameBlobWriter writer, SavegameSerializerBase.GameContext context)
	{
		writer.WriteObjectAsJson(META_FILENAME, new SerializedMetadata
		{
			Version = Meta.Version,
			AppSourceVersion = Meta.AppSourceVersion,
			AppSourceEnvironment = Meta.AppSourceEnvironment,
			AppSourceStore = Meta.AppSourceStore,
			GameMode = ModeConfig.Serialize(),
			BinaryDataCheckpoints = Meta.BinaryDataCheckpoints,
			LastSaved = DateTime.Now,
			TotalPlaytime = context.LocalPlayer.TotalPlaytime,
			ResearchProgress = Singleton<GameCore>.G.Research.Progress.ComputeProgress(),
			StructureCount = context.Maps.GetMapById(GameMap.ID_MAIN).ComputeTotalBuildingCount(),
			CheatsUsed = Meta.CheatsUsed
		});
	}
}
