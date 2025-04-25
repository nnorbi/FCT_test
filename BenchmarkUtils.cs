using System.Linq;

public static class BenchmarkUtils
{
	public static GameStartOptions NewGame()
	{
		return new GameStartOptionsStartNew
		{
			Config = new GameModeConfig(Globals.Resources.SupportedGameModes.First(), 0),
			MenuMode = true,
			UID = Globals.Savegames.GenerateNewUID()
		};
	}
}
