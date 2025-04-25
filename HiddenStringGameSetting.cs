using UnityEngine;

public class HiddenStringGameSetting : SimpleGameSetting<string>
{
	public override bool ShowInUI => false;

	public HiddenStringGameSetting(string id, string defaultValue = "")
		: base(id, defaultValue)
	{
	}

	public override void Write()
	{
		PlayerPrefs.SetString(base.FullId, base.Value);
	}

	public override void Read()
	{
		SetValue(PlayerPrefs.GetString(base.FullId, base.Default));
	}

	public override bool TrySetFromString(string value)
	{
		SetValue(value);
		return true;
	}
}
