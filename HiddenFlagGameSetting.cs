public class HiddenFlagGameSetting : BoolGameSetting
{
	public override bool ShowInUI => false;

	public HiddenFlagGameSetting(string id, bool defaultValue = false)
		: base(id, defaultValue)
	{
	}
}
