using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

public class RefreshRateGameSetting : SimpleGameSetting<RefreshRate>, ITypedValueListGameSetting<RefreshRate>
{
	public RefreshRateGameSetting(string id)
		: base(id, Screen.currentResolution.refreshRateRatio)
	{
	}

	public IList<RefreshRate> GenerateAvailableValues()
	{
		Resolution currentResolution = Screen.currentResolution;
		HashSet<RefreshRate> result = new HashSet<RefreshRate> { base.Value, currentResolution.refreshRateRatio };
		Resolution[] resolutions = Screen.resolutions;
		foreach (Resolution resolution in resolutions)
		{
			result.Add(resolution.refreshRateRatio);
		}
		List<RefreshRate> entries = result.Where((RefreshRate r) => r.denominator != 0).ToList();
		if (entries.Count == 0)
		{
			entries.Add(new RefreshRate
			{
				numerator = 60u,
				denominator = 1u
			});
		}
		entries.Sort();
		return entries;
	}

	public string FormatValue(RefreshRate value)
	{
		return value.value.ToString("0.##", CultureInfo.InvariantCulture);
	}

	public override void Write()
	{
		PlayerPrefs.SetInt(base.FullId + ".numerator", (int)base.Value.numerator);
		PlayerPrefs.SetInt(base.FullId + ".denominator", (int)base.Value.denominator);
	}

	public override void Read()
	{
		RefreshRate fallbackValue = Screen.currentResolution.refreshRateRatio;
		uint numerator = (uint)PlayerPrefs.GetInt(base.FullId + ".numerator", (int)fallbackValue.numerator);
		uint denominator = (uint)PlayerPrefs.GetInt(base.FullId + ".denominator", (int)fallbackValue.denominator);
		SetValue(new RefreshRate
		{
			numerator = numerator,
			denominator = denominator
		});
	}
}
