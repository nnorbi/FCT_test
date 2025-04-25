using Core.Dependency;
using UnityEngine;

public class HUDDebugPanelTabLOD : HUDDebugPanelTab
{
	[SerializeField]
	protected HUDDebugToggleableIntegerSlider UILevelOfDetailOverrideSlider;

	[SerializeField]
	protected HUDDebugToggleableIntegerSlider UILevelOfDetailMaxSlider;

	[SerializeField]
	protected GameObject UIMaxSliderIsIgnoredWarning;

	[Construct]
	private void Construct()
	{
		UILevelOfDetailOverrideSlider.Setup(OnOverrideSliderChange, OnOverrideSliderEnable, OnOverrideSliderDisable);
		UILevelOfDetailMaxSlider.Setup(OnMaxSliderChange, OnMaxSliderEnable, OnMaxSliderDisable);
		CheckOverrideOverMaxPrecedence();
	}

	private void OnOverrideSliderEnable()
	{
		LODManager.DEBUG_OVERRIDE_LOD = true;
		CheckOverrideOverMaxPrecedence();
	}

	private void OnOverrideSliderDisable()
	{
		LODManager.DEBUG_OVERRIDE_LOD = false;
		CheckOverrideOverMaxPrecedence();
	}

	private void OnMaxSliderEnable()
	{
		LODManager.DEBUG_LIMIT_MAX_LOD = true;
		CheckOverrideOverMaxPrecedence();
	}

	private void OnMaxSliderDisable()
	{
		LODManager.DEBUG_LIMIT_MAX_LOD = false;
		CheckOverrideOverMaxPrecedence();
	}

	protected void OnOverrideSliderChange(float arg)
	{
		int lod = (int)arg;
		LODManager.DEBUG_OVERRIDE_LOD_VALUE = lod;
		CheckOverrideOverMaxPrecedence();
	}

	private void CheckOverrideOverMaxPrecedence()
	{
		bool maxSliderIsIgnored = UILevelOfDetailOverrideSlider.IsEnabled && UILevelOfDetailMaxSlider.IsEnabled && UILevelOfDetailOverrideSlider.Value > UILevelOfDetailMaxSlider.Value;
		UIMaxSliderIsIgnoredWarning.SetActive(maxSliderIsIgnored);
	}

	protected void OnMaxSliderChange(float arg)
	{
		int lod = (int)arg;
		LODManager.DEBUG_MAX_LOD_VALUE = lod;
		CheckOverrideOverMaxPrecedence();
	}
}
