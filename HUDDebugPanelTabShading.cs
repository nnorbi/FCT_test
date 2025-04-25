using System;
using System.Collections.Generic;
using Core.Dependency;
using UnityEngine;
using UnityEngine.UI;

public class HUDDebugPanelTabShading : HUDDebugPanelTab, IDisposable
{
	[Serializable]
	public struct ViewOverride
	{
		[SerializeField]
		public Button AssociatedButton;

		[SerializeField]
		public Material BuildingsOverride;

		[SerializeField]
		public Material PlayingFieldOverride;

		[SerializeField]
		public Material SpaceStationOverride;
	}

	[SerializeField]
	protected ViewOverride[] ViewOverrides;

	[SerializeField]
	protected Button DefaultSettingsButton;

	[SerializeField]
	protected Shader OriginalSpaceStationShader;

	protected Material OriginalBuildingMaterial;

	protected Material OriginalIslandFramesMaterial;

	protected Material OriginalPlayingFieldMaterial;

	protected List<MeshRenderer> SpaceStationMeshes = new List<MeshRenderer>();

	protected List<Material> SpaceStationOriginalMaterials = new List<Material>();

	private VisualTheme VisualTheme;

	public void Dispose()
	{
		DefaultSettingsButton.onClick.RemoveListener(SetDefault);
		ResetViews();
	}

	protected void RegisterSpaceStationMaterials()
	{
		MeshRenderer[] array = UnityEngine.Object.FindObjectsOfType<MeshRenderer>();
		foreach (MeshRenderer meshRenderer in array)
		{
			if (meshRenderer.material.shader.Equals(OriginalSpaceStationShader))
			{
				SpaceStationMeshes.Add(meshRenderer);
				SpaceStationOriginalMaterials.Add(meshRenderer.material);
			}
		}
	}

	[Construct]
	private void Construct(VisualTheme visualTheme)
	{
		VisualTheme = visualTheme;
		DefaultSettingsButton.onClick.AddListener(SetDefault);
		ViewOverride[] viewOverrides = ViewOverrides;
		for (int i = 0; i < viewOverrides.Length; i++)
		{
			ViewOverride viewOverride = viewOverrides[i];
			viewOverride.AssociatedButton.onClick.AddListener(delegate
			{
				OnButtonClick(viewOverride);
			});
		}
		OriginalBuildingMaterial = VisualTheme.BaseResources.BuildingMaterial;
		OriginalPlayingFieldMaterial = VisualTheme.BaseResources.PlayingfieldMaterial;
		OriginalIslandFramesMaterial = VisualTheme.BaseResources.IslandFramesMaterial;
		RegisterSpaceStationMaterials();
	}

	protected void SetDefault()
	{
		ResetViews();
		for (int i = 0; i < SpaceStationMeshes.Count; i++)
		{
			SpaceStationMeshes[i].material = SpaceStationOriginalMaterials[i];
		}
	}

	protected void ResetViews()
	{
		VisualTheme.BaseResources.BuildingMaterial = OriginalBuildingMaterial;
		VisualTheme.BaseResources.PlayingfieldMaterial = OriginalPlayingFieldMaterial;
		VisualTheme.BaseResources.IslandFramesMaterial = OriginalIslandFramesMaterial;
	}

	protected void OnButtonClick(ViewOverride viewOverride)
	{
		VisualTheme.BaseResources.BuildingMaterial = viewOverride.BuildingsOverride;
		VisualTheme.BaseResources.PlayingfieldMaterial = viewOverride.PlayingFieldOverride;
		VisualTheme.BaseResources.IslandFramesMaterial = viewOverride.SpaceStationOverride;
		ChangeMaterialsInScene(viewOverride.SpaceStationOverride);
	}

	protected void ChangeMaterialsInScene(Material viewOverrideSpaceStationOverride)
	{
		foreach (MeshRenderer meshRenderer in SpaceStationMeshes)
		{
			meshRenderer.material = viewOverrideSpaceStationOverride;
		}
	}
}
