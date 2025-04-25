public class DevSettings : GameSettingsGroup
{
	public BoolGameSetting DetailedBuildingEfficiency = new BoolGameSetting("detailed-building-efficiency", defaultValue: false);

	public EnumGameSetting<IslandSimulatorMode> SimulationMode = new EnumGameSetting<IslandSimulatorMode>("simulation-mode", IslandSimulatorMode.Hybrid);

	public BoolGameSetting StartNewGamesInDemoEditorOnly = new BoolGameSetting("start-new-games-in-demo-editor-only", defaultValue: false);

	public DevSettings(bool saveOnChange)
		: base("dev-settings", saveOnChange)
	{
		Register(DetailedBuildingEfficiency);
		Register(SimulationMode);
	}

	public override void Load()
	{
		base.Load();
	}
}
