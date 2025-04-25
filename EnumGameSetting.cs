using System;
using UnityEngine;

public class EnumGameSetting<TEnum> : GameSetting, IValueListGameSetting where TEnum : struct, Enum
{
	public TEnum Default { get; }

	public TEnum Value { get; private set; }

	public override bool IsModified => !Value.Equals(Default);

	public string[] AvailableValueIds { get; }

	public int CurrentValueIndex
	{
		get
		{
			string valueId = Enum.GetName(typeof(TEnum), Value);
			return Array.IndexOf(AvailableValueIds, valueId);
		}
		set
		{
			string valueId = AvailableValueIds[value];
			TrySetFromString(valueId);
		}
	}

	public static implicit operator TEnum(EnumGameSetting<TEnum> setting)
	{
		return setting.Value;
	}

	public EnumGameSetting(string id, TEnum defaultValue)
		: base(id)
	{
		Default = defaultValue;
		AvailableValueIds = Enum.GetNames(typeof(TEnum));
		Value = defaultValue;
	}

	public string FormatValueId(string valueId)
	{
		return ("menu.setting." + Id + "." + valueId).tr();
	}

	public void SetValue(TEnum value)
	{
		if (!Value.Equals(value))
		{
			Value = value;
			Changed.Invoke();
		}
	}

	public override bool TrySetFromString(string valueId)
	{
		if (!Enum.TryParse<TEnum>(valueId, out var parsed))
		{
			Debug.LogWarning("Not a valid enum value: " + valueId);
			return false;
		}
		SetValue(parsed);
		return true;
	}

	public override bool Equals(GameSetting other)
	{
		if (!(other is EnumGameSetting<TEnum> { Value: var value }))
		{
			return false;
		}
		return value.Equals(Value);
	}

	public override void CopyFrom(GameSetting other)
	{
		if (!(other is EnumGameSetting<TEnum> otherEnum))
		{
			throw new ArgumentException("Is not an EnumGameSetting", "other");
		}
		SetValue(otherEnum.Value);
	}

	public override string GetValueText()
	{
		return Value.ToString();
	}

	public override void Write()
	{
		PlayerPrefs.SetString(base.FullId, Value.ToString());
	}

	public override void Read()
	{
		string value = PlayerPrefs.GetString(base.FullId, Default.ToString());
		if (!TrySetFromString(value))
		{
			Debug.LogWarning("Invalid stored enum value for " + base.FullId + ": " + value);
			ResetToDefault();
		}
	}

	public override void ResetToDefault()
	{
		SetValue(Default);
	}
}
