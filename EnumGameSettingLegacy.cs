using System;

[Obsolete("Use EnumGameSetting instead")]
public class EnumGameSettingLegacy<T> : DynamicEnumGameSetting<T>
{
	public Entry[] AvailableValues;

	public EnumGameSettingLegacy(string id, string defaultValueStringId, Entry[] values)
		: base(id, (AvailableValuesGetter)(() => (defaultValueStringId, values)))
	{
		AvailableValues = values;
	}
}
