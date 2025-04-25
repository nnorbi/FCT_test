public class GeneralGameSettings : GameSettingsGroup
{
	public LanguageGameSetting Language = new LanguageGameSetting("language", "autodetect");

	public BoolGameSetting Tutorial = new BoolGameSetting("tutorial", defaultValue: true);

	public FloatGameSetting SFXVolume = new FloatGameSetting("sfx-volume", 0.5f);

	public FloatGameSetting MusicVolume = new FloatGameSetting("music-volume", 0.5f);

	public EnumGameSettingLegacy<float> AutosaveInterval = new EnumGameSettingLegacy<float>("autosave-interval", "5", new DynamicEnumGameSetting<float>.Entry[7]
	{
		new DynamicEnumGameSetting<float>.Entry("2", 2f),
		new DynamicEnumGameSetting<float>.Entry("5", 5f),
		new DynamicEnumGameSetting<float>.Entry("10", 10f),
		new DynamicEnumGameSetting<float>.Entry("15", 15f),
		new DynamicEnumGameSetting<float>.Entry("30", 30f),
		new DynamicEnumGameSetting<float>.Entry("60", 60f),
		new DynamicEnumGameSetting<float>.Entry("off", -1f)
	});

	public EnumGameSettingLegacy<int> SavegameBackupCount = new EnumGameSettingLegacy<int>("savegame-backup-count", "25", new DynamicEnumGameSetting<int>.Entry[5]
	{
		new DynamicEnumGameSetting<int>.Entry("5", 5),
		new DynamicEnumGameSetting<int>.Entry("15", 15),
		new DynamicEnumGameSetting<int>.Entry("25", 25),
		new DynamicEnumGameSetting<int>.Entry("50", 50),
		new DynamicEnumGameSetting<int>.Entry("unlimited", -1)
	});

	public BoolGameSetting Telemetry = new BoolGameSetting("telemetry", defaultValue: true);

	public HiddenFlagGameSetting PreloadIntroShown = new HiddenFlagGameSetting("preload-intro-shown");

	public HiddenStringGameSetting LastChangelogEntry = new HiddenStringGameSetting("last-changelog-entry");

	public GeneralGameSettings(bool saveOnChange)
		: base("general-settings", saveOnChange)
	{
		Register(Language);
		Register(AutosaveInterval);
		Register(SFXVolume);
		Register(MusicVolume);
		Register(SavegameBackupCount);
		Register(Telemetry);
		Register(Tutorial);
		Register(PreloadIntroShown);
		Register(LastChangelogEntry);
	}
}
