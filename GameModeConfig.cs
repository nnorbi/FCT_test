using System;
using System.Linq;

public class GameModeConfig
{
	public struct SerializedData
	{
		public string GameModeId;

		public int Seed;
	}

	public readonly MetaGameMode Base;

	public readonly int Seed;

	public static GameModeConfig Deserialize(SerializedData data)
	{
		MetaGameMode mode = Globals.Resources.SupportedGameModes.First((MetaGameMode metaGameMode) => metaGameMode.name == data.GameModeId);
		if (mode == null)
		{
			throw new Exception("Game mode " + data.GameModeId + " not found or not supported.");
		}
		return new GameModeConfig(mode, data.Seed);
	}

	public GameModeConfig(MetaGameMode baseMode, int seed)
	{
		Base = baseMode;
		Seed = seed;
	}

	public SerializedData Serialize()
	{
		return new SerializedData
		{
			GameModeId = Base.name,
			Seed = Seed
		};
	}
}
