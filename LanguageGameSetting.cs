using System;
using System.Globalization;
using System.Linq;
using UnityEngine;

public class LanguageGameSetting : GameSetting, IValueListGameSetting
{
	private string Default;

	public string Value { get; private set; }

	public override bool IsModified => Value != Globals.Localization.CurrentTranslator.LanguageCode;

	public override bool RequiresRestart => true;

	public string[] AvailableValueIds => Globals.Localization.AvailableTranslators.Select((ITranslator t) => t.LanguageCode).ToArray();

	public int CurrentValueIndex
	{
		get
		{
			return Array.IndexOf(AvailableValueIds, Value);
		}
		set
		{
			string languageCode = AvailableValueIds[value];
			SetValue(languageCode);
		}
	}

	public LanguageGameSetting(string id, string defaultValue)
		: base(id)
	{
		Default = defaultValue;
		Value = Default;
	}

	public string FormatValueId(string valueId)
	{
		CultureInfo culture = new CultureInfo(valueId);
		return Globals.Localization.GetDisplayLanguageFromCode(valueId).ToUpper(culture);
	}

	public void SetValue(string value)
	{
		if (!AvailableValueIds.Contains(value))
		{
			Debug.LogWarning("Invalid language code value: " + value + " -> Ignoring");
			return;
		}
		Value = value;
		Debug.Log("Language set to " + value);
		Changed.Invoke();
	}

	public override bool Equals(GameSetting other)
	{
		throw new NotImplementedException();
	}

	public override void CopyFrom(GameSetting other)
	{
		throw new NotImplementedException();
	}

	public override string GetValueText()
	{
		return FormatValueId(Value);
	}

	public override void Write()
	{
		PlayerPrefs.SetString(base.FullId, Value);
		Debug.Log("Language SAVED to " + Value);
	}

	public override void Read()
	{
		string languageCode = PlayerPrefs.GetString(base.FullId, Default);
		Value = languageCode;
		Changed.Invoke();
	}

	public override bool TrySetFromString(string value)
	{
		SetValue(value);
		return true;
	}

	public override void ResetToDefault()
	{
		SetValue(Default);
	}
}
