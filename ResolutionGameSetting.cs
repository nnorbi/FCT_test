using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ResolutionGameSetting : SimpleGameSetting<DisplayResolution>, ITypedValueListGameSetting<DisplayResolution>
{
	public ResolutionGameSetting(string id)
		: base(id, DisplayResolution.UNINITIALIZED)
	{
	}

	public IList<DisplayResolution> GenerateAvailableValues()
	{
		Resolution currentResolution = Screen.currentResolution;
		HashSet<DisplayResolution> result = new HashSet<DisplayResolution>
		{
			new DisplayResolution(currentResolution)
		};
		Resolution[] resolutions = Screen.resolutions;
		foreach (Resolution resolution in resolutions)
		{
			result.Add(new DisplayResolution(resolution));
		}
		List<DisplayResolution> entries = result.Where((DisplayResolution r) => r.Width > 128 && r.Height > 128).ToList();
		entries.Sort();
		return entries;
	}

	public string FormatValue(DisplayResolution value)
	{
		return value.ToString();
	}

	public override void Write()
	{
		PlayerPrefs.SetInt(base.FullId + ".width", base.Value.Width);
		PlayerPrefs.SetInt(base.FullId + ".height", base.Value.Height);
	}

	public override void Read()
	{
		DisplayResolution fallbackValue = DisplayResolution.UNINITIALIZED;
		int width = PlayerPrefs.GetInt(base.FullId + ".width", fallbackValue.Width);
		int height = PlayerPrefs.GetInt(base.FullId + ".height", fallbackValue.Height);
		SetValue(new DisplayResolution(width, height));
	}
}
