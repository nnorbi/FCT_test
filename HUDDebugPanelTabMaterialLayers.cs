using System;
using Core.Dependency;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

public class HUDDebugPanelTabMaterialLayers : HUDDebugPanelTab, IDisposable
{
	[UsedImplicitly(ImplicitUseTargetFlags.Members)]
	public enum DebugUberMaterialLayer
	{
		Accent,
		FakeEmissive,
		MetalBrushed,
		MetalNoise,
		MetalCoat,
		Plastic,
		BeltRubber,
		Greeble
	}

	[SerializeField]
	protected HUDDebugMaterialEntry UIMaterialOverlayPrefab;

	[SerializeField]
	protected Color[] MaterialDebugColors;

	[SerializeField]
	protected RectTransform UIMaterialOverlaysParent;

	[SerializeField]
	protected Button UIDisableAllButton;

	[SerializeField]
	protected Button UIEnableAllButton;

	protected int ActiveMask;

	protected HUDDebugMaterialEntry[] DebugMaterialEntries;

	protected Texture2D LookupTexture;

	protected int GlobalOverride = -1;

	public void Dispose()
	{
		UIDisableAllButton.onClick.RemoveListener(HideAllOverlays);
		UIEnableAllButton.onClick.RemoveListener(ShowAllOverlays);
		UnityEngine.Object.Destroy(LookupTexture);
		Shader.SetGlobalFloat(GlobalShaderInputs.MaterialDebugGlobalOverride, 0f);
	}

	[Construct]
	private void Construct()
	{
		UIMaterialOverlaysParent.RemoveAllChildren();
		DebugUberMaterialLayer[] materialTypes = (DebugUberMaterialLayer[])Enum.GetValues(typeof(DebugUberMaterialLayer));
		if (MaterialDebugColors.Length != materialTypes.Length)
		{
			throw new Exception("Material colors arrays does not match the amount of enumeration values in DebugMaterialType. Considering updating the color list");
		}
		DebugMaterialEntries = new HUDDebugMaterialEntry[materialTypes.Length];
		for (int i = 0; i < materialTypes.Length; i++)
		{
			DebugUberMaterialLayer type = materialTypes[i];
			DebugMaterialEntries[i] = UnityEngine.Object.Instantiate(UIMaterialOverlayPrefab, UIMaterialOverlaysParent);
			int iCopy = i;
			DebugMaterialEntries[i].Setup(("debug.material." + Enum.GetName(typeof(DebugUberMaterialLayer), type)).tr(), MaterialDebugColors[i], delegate
			{
				OnOverlayTogglePressed(iCopy);
			}, delegate
			{
				OnElementPressed(iCopy);
			}, delegate
			{
				OnSetGlobalOverride(iCopy);
			});
		}
		UIDisableAllButton.onClick.AddListener(HideAllOverlays);
		UIEnableAllButton.onClick.AddListener(ShowAllOverlays);
		SetActiveMask(0);
		Shader.SetGlobalInteger(GlobalShaderInputs.MaterialDebugPulseBitMask, 0);
		Shader.SetGlobalFloat(GlobalShaderInputs.MaterialDebugPulseStartTime, 0f);
		CreateMaterialOverlayLookupTexture();
	}

	protected void SetActiveMask(int mask)
	{
		ActiveMask = mask;
		Shader.SetGlobalInteger(GlobalShaderInputs.MaterialDebugMask, mask);
	}

	protected void CreateMaterialOverlayLookupTexture()
	{
		int size = MaterialDebugColors.Length;
		LookupTexture = new Texture2D(size, 1, TextureFormat.RGBA32, mipChain: false)
		{
			filterMode = FilterMode.Point
		};
		for (int i = 0; i < MaterialDebugColors.Length; i++)
		{
			LookupTexture.SetPixel(i, 0, MaterialDebugColors[i]);
		}
		LookupTexture.Apply();
		Shader.SetGlobalTexture(GlobalShaderInputs.MaterialDebugLookupTexture, LookupTexture);
	}

	protected void OnOverlayTogglePressed(int index)
	{
		SetActiveMask(ActiveMask ^ (1 << index));
		DebugMaterialEntries[index].ToggleVisibility();
	}

	protected static void OnElementPressed(int index)
	{
		Shader.SetGlobalInteger(GlobalShaderInputs.MaterialDebugPulseBitMask, 1 << index);
		Shader.SetGlobalFloat(GlobalShaderInputs.MaterialDebugPulseStartTime, Time.time);
	}

	private void OnSetGlobalOverride(int index)
	{
		if (GlobalOverride >= 0)
		{
			DebugMaterialEntries[GlobalOverride].HideGlobalOverrideIndicator();
		}
		if (index != GlobalOverride)
		{
			GlobalOverride = index;
			DebugMaterialEntries[index].ShowGlobalOverrideIndicator();
		}
		else
		{
			GlobalOverride = -1;
		}
		Shader.SetGlobalFloat(GlobalShaderInputs.MaterialDebugGlobalOverride, GlobalOverride + 1);
	}

	protected void ShowAllOverlays()
	{
		SetActiveMask((1 << MaterialDebugColors.Length) - 1);
		HUDDebugMaterialEntry[] debugMaterialEntries = DebugMaterialEntries;
		foreach (HUDDebugMaterialEntry materialEntry in debugMaterialEntries)
		{
			materialEntry.Show();
		}
	}

	protected void HideAllOverlays()
	{
		SetActiveMask(0);
		HUDDebugMaterialEntry[] debugMaterialEntries = DebugMaterialEntries;
		foreach (HUDDebugMaterialEntry materialEntry in debugMaterialEntries)
		{
			materialEntry.Hide();
		}
	}
}
