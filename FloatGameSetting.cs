using System;
using System.Globalization;
using Unity.Mathematics;
using UnityEngine;

public class FloatGameSetting : SimpleGameSetting<float>
{
	public readonly float Min;

	public readonly float Max;

	public static implicit operator float(FloatGameSetting setting)
	{
		return setting.Value;
	}

	public FloatGameSetting(string id, float defaultValue, float min = 0f, float max = 1f)
		: base(id, defaultValue)
	{
		Min = min;
		Max = max;
	}

	public override void Write()
	{
		PlayerPrefs.SetFloat(base.FullId, base.Value);
	}

	public override void Read()
	{
		SetValue(math.clamp(PlayerPrefs.GetFloat(base.FullId, base.Default), Min, Max));
	}

	public override bool TrySetFromString(string value)
	{
		try
		{
			float parsed = float.Parse(value, CultureInfo.InvariantCulture);
			SetValue(math.clamp(parsed, Min, Max));
		}
		catch (Exception)
		{
			return false;
		}
		return true;
	}

	public override string GetHelpText()
	{
		return base.Value.ToString("F4", CultureInfo.InvariantCulture) + " (" + Min.ToString("F4", CultureInfo.InvariantCulture) + " ... " + Max.ToString("F4", CultureInfo.InvariantCulture) + ")";
	}
}
