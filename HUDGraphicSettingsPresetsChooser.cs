using System.Collections.Generic;
using Core.Dependency;
using TMPro;
using UnityEngine;

public class HUDGraphicSettingsPresetsChooser : HUDComponent
{
	[SerializeField]
	private TMP_Dropdown UIDropdown;

	[Construct]
	private void Construct()
	{
		List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>
		{
			new TMP_Dropdown.OptionData("menu.settings.graphic-preset.custom".tr())
		};
		foreach (GraphicPreset option in GraphicSettings.PRESETS)
		{
			options.Add(new TMP_Dropdown.OptionData(option.Title));
		}
		UIDropdown.options = options;
		UpdateDropdownIndex();
		UIDropdown.onValueChanged.AddListener(OnPresetSelected);
		Globals.Settings.Graphics.Changed.AddListener(UpdateDropdownIndex);
	}

	private void UpdateDropdownIndex()
	{
		GraphicPreset preset = FindCurrentPreset();
		if (preset == null)
		{
			UIDropdown.value = 0;
		}
		else
		{
			UIDropdown.value = GraphicSettings.PRESETS.IndexOf(preset) + 1;
		}
	}

	private GraphicPreset FindCurrentPreset()
	{
		GraphicSettings graphicSettings = Globals.Settings.Graphics;
		foreach (GraphicPreset preset in GraphicSettings.PRESETS)
		{
			if (preset.IsActive(graphicSettings))
			{
				return preset;
			}
		}
		return null;
	}

	private void OnPresetSelected(int index)
	{
		if (index != 0)
		{
			GraphicPreset preset = GraphicSettings.PRESETS[index - 1];
			Globals.Settings.Graphics.ApplyGraphicPreset(preset);
			UpdateDropdownIndex();
		}
	}

	protected override void OnDispose()
	{
		UIDropdown.onValueChanged.RemoveListener(OnPresetSelected);
		Globals.Settings.Graphics.Changed.RemoveListener(UpdateDropdownIndex);
	}
}
