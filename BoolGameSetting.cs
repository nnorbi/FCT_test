using UnityEngine;

public class BoolGameSetting : SimpleGameSetting<bool>
{
	public static implicit operator bool(BoolGameSetting setting)
	{
		return setting.Value;
	}

	public BoolGameSetting(string id, bool defaultValue)
		: base(id, defaultValue)
	{
	}

	public override void Write()
	{
		PlayerPrefs.SetInt(base.FullId, base.Value ? 1 : 0);
	}

	public override void Read()
	{
		int storedValue = PlayerPrefs.GetInt(base.FullId, base.Default ? 10 : (-10));
		SetValue(storedValue > 0);
	}

	public override bool TrySetFromString(string value)
	{
		switch (value.ToLower())
		{
		case "1":
		case "yes":
		case "y":
		case "true":
		case "on":
			SetValue(value: true);
			return true;
		case "0":
		case "no":
		case "n":
		case "false":
		case "off":
			SetValue(value: false);
			return true;
		default:
			return false;
		}
	}

	public override string GetHelpText()
	{
		return base.Value + " (0|1)";
	}
}
