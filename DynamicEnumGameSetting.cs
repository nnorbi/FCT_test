using System;
using System.Linq;
using UnityEngine;

public class DynamicEnumGameSetting<T> : GameSetting
{
	public class Entry
	{
		public string ValueId;

		public T Value;

		public Entry(string id, T value)
		{
			ValueId = id;
			Value = value;
		}
	}

	protected delegate(string, Entry[]) AvailableValuesGetter();

	protected AvailableValuesGetter ValuesGetter;

	public Entry Current { get; private set; }

	protected Entry[] Values { get; private set; }

	protected Entry Default { get; private set; }

	public override bool IsModified => !Current.ValueId.Equals(Default.ValueId);

	protected DynamicEnumGameSetting(string id, AvailableValuesGetter valuesGetter)
		: base(id)
	{
		ValuesGetter = valuesGetter;
		UpdateAvailableValues();
		Current = Default;
	}

	public Entry[] GetAvailableEntries()
	{
		return ValuesGetter().Item2;
	}

	public void UpdateAvailableValues()
	{
		(string, Entry[]) tuple = ValuesGetter();
		string defaultValue = tuple.Item1;
		Entry[] values = tuple.Item2;
		Values = values;
		if (Values.Length == 0)
		{
			throw new Exception("Values for enum '" + Id + "' can not be empty");
		}
		Default = FindEntryById(defaultValue);
		if (Default == null)
		{
			Debug.LogWarning("Value '" + defaultValue + "' is not a valid default value for '" + Id + "' -> Resetting to first");
			Default = Values[0];
		}
		if (Current != null && FindEntryById(Current.ValueId) == null)
		{
			Debug.LogWarning("Current value '" + Current?.ToString() + "' is not a valid value for '" + Id + "' -> Resetting to first");
		}
	}

	public override void Write()
	{
		PlayerPrefs.SetString(base.FullId, Current.ValueId);
	}

	public override void Read()
	{
		string valueId = PlayerPrefs.GetString(base.FullId, Default.ValueId);
		if (!SetValue(valueId))
		{
			Debug.LogWarning("Saved value '" + valueId + "' is not a valid value for setting, resetting to default");
			Current = Default;
			Changed.Invoke();
		}
	}

	public override bool TrySetFromString(string value)
	{
		return SetValue(value);
	}

	public override void ResetToDefault()
	{
		SetValue(Default.ValueId);
	}

	public bool SetValue(string valueId)
	{
		Entry newValue = FindEntryById(valueId);
		if (newValue == null)
		{
			return false;
		}
		if (newValue != Current)
		{
			Current = newValue;
			Changed.Invoke();
		}
		return true;
	}

	private Entry FindEntryById(string valueId)
	{
		valueId = valueId.ToLower().Trim();
		return Values.FirstOrDefault((Entry e) => e.ValueId.ToLower() == valueId);
	}

	public override string GetHelpText()
	{
		T value = Current.Value;
		return value?.ToString() + " (" + string.Join('|', Values.Select((Entry v) => v.ValueId)) + ")";
	}

	public override string GetValueText()
	{
		return GetValueText(Current);
	}

	protected virtual string GetValueText(Entry entry)
	{
		return ("menu.setting." + Id + "." + entry.ValueId).tr();
	}

	public string GetValueText(string valueId)
	{
		Entry entry = FindEntryById(valueId);
		if (entry == null)
		{
			Debug.LogWarning("Entry with id " + valueId + " not found");
			return "???";
		}
		return GetValueText(entry);
	}

	public override bool Equals(GameSetting other)
	{
		if (!(other is DynamicEnumGameSetting<T> otherDynamicEnum))
		{
			return false;
		}
		ref T value = ref Current.Value;
		object obj = otherDynamicEnum.Current.Value;
		return value.Equals(obj);
	}

	public override void CopyFrom(GameSetting other)
	{
		if (!(other is DynamicEnumGameSetting<T> otherDynamicEnum))
		{
			throw new Exception("Trying to assign incompatible setting: " + other.GetType().Name + " to " + GetType().Name);
		}
		if (!other.Equals(this))
		{
			Current = otherDynamicEnum.Current;
			Changed.Invoke();
		}
	}
}
