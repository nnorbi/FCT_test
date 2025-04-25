public class InterfaceGameSettings : GameSettingsGroup
{
	public EnumGameSettingLegacy<float> UIScale = new EnumGameSettingLegacy<float>("ui-scale", "100", new DynamicEnumGameSetting<float>.Entry[6]
	{
		new DynamicEnumGameSetting<float>.Entry("50", 0.5f),
		new DynamicEnumGameSetting<float>.Entry("60", 0.6f),
		new DynamicEnumGameSetting<float>.Entry("70", 0.7f),
		new DynamicEnumGameSetting<float>.Entry("80", 0.8f),
		new DynamicEnumGameSetting<float>.Entry("90", 0.9f),
		new DynamicEnumGameSetting<float>.Entry("100", 1f)
	});

	public BoolGameSetting AutoCopyBpToClipboard = new BoolGameSetting("copy-bp-to-clipboard", defaultValue: true);

	public BoolGameSetting MenuTransitions = new BoolGameSetting("menu-transitions", defaultValue: true);

	public InterfaceGameSettings(bool saveOnChange)
		: base("interface-settings", saveOnChange)
	{
		Register(UIScale);
		Register(AutoCopyBpToClipboard);
		Register(MenuTransitions);
	}
}
