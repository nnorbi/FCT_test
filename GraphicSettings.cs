using System;
using System.Collections.Generic;
using HorizonBasedAmbientOcclusion.Universal;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GraphicSettings : GameSettingsGroup
{
	public static readonly List<GraphicPreset> PRESETS = new List<GraphicPreset>
	{
		new GraphicPreset
		{
			Id = "minimal",
			AntialiasingQuality = GraphicsAntialiasingQuality.Off,
			AmbientOcclusion = GraphicsAOQuality.Off,
			ShadowQuality = GraphicsShadowQuality.Off,
			BackgroundDetails = GraphicsBackgroundDetails.Minimum,
			IslandDetails = GraphicsIslandDetails.Minimum,
			ShaderQuality = GraphicsShaderQuality.Minimum,
			BuildingDetails = GraphicsBuildingDetails.Minimum,
			AnisotropicFiltering = false
		},
		new GraphicPreset
		{
			Id = "low",
			AntialiasingQuality = GraphicsAntialiasingQuality.Smaa,
			AmbientOcclusion = GraphicsAOQuality.Low,
			ShadowQuality = GraphicsShadowQuality.Low,
			BackgroundDetails = GraphicsBackgroundDetails.Low,
			IslandDetails = GraphicsIslandDetails.Low,
			ShaderQuality = GraphicsShaderQuality.Low,
			BuildingDetails = GraphicsBuildingDetails.Low,
			AnisotropicFiltering = true
		},
		new GraphicPreset
		{
			Id = "medium",
			AntialiasingQuality = GraphicsAntialiasingQuality.MSAA2,
			AmbientOcclusion = GraphicsAOQuality.Medium,
			ShadowQuality = GraphicsShadowQuality.Medium,
			BackgroundDetails = GraphicsBackgroundDetails.Medium,
			IslandDetails = GraphicsIslandDetails.Medium,
			ShaderQuality = GraphicsShaderQuality.Medium,
			BuildingDetails = GraphicsBuildingDetails.Medium,
			AnisotropicFiltering = true
		},
		new GraphicPreset
		{
			Id = "high",
			AntialiasingQuality = GraphicsAntialiasingQuality.MSAA4,
			AmbientOcclusion = GraphicsAOQuality.High,
			ShadowQuality = GraphicsShadowQuality.High,
			BackgroundDetails = GraphicsBackgroundDetails.High,
			IslandDetails = GraphicsIslandDetails.High,
			ShaderQuality = GraphicsShaderQuality.High,
			BuildingDetails = GraphicsBuildingDetails.High,
			AnisotropicFiltering = true
		},
		new GraphicPreset
		{
			Id = "extreme",
			AntialiasingQuality = GraphicsAntialiasingQuality.MSAA4,
			AmbientOcclusion = GraphicsAOQuality.Extreme,
			ShadowQuality = GraphicsShadowQuality.Extreme,
			BackgroundDetails = GraphicsBackgroundDetails.High,
			IslandDetails = GraphicsIslandDetails.High,
			ShaderQuality = GraphicsShaderQuality.Extreme,
			BuildingDetails = GraphicsBuildingDetails.High,
			AnisotropicFiltering = true
		}
	};

	public EnumGameSetting<GraphicsAntialiasingQuality> Antialiasing = new EnumGameSetting<GraphicsAntialiasingQuality>("antialiasing", GraphicsAntialiasingQuality.MSAA4);

	public EnumGameSetting<GraphicsShaderQuality> ShaderQuality = new EnumGameSetting<GraphicsShaderQuality>("shader-quality", GraphicsShaderQuality.High);

	public EnumGameSetting<GraphicsAOQuality> AmbientOcclusion = new EnumGameSetting<GraphicsAOQuality>("ambient-occlusion", GraphicsAOQuality.Medium);

	public EnumGameSetting<GraphicsShadowQuality> ShadowQuality = new EnumGameSetting<GraphicsShadowQuality>("shadow-quality", GraphicsShadowQuality.High);

	public EnumGameSetting<GraphicsBackgroundDetails> BackgroundDetails = new EnumGameSetting<GraphicsBackgroundDetails>("background-details", GraphicsBackgroundDetails.High);

	public EnumGameSetting<GraphicsIslandDetails> IslandDetails = new EnumGameSetting<GraphicsIslandDetails>("island-details", GraphicsIslandDetails.High);

	public EnumGameSetting<GraphicsBuildingDetails> BuildingDetails = new EnumGameSetting<GraphicsBuildingDetails>("building-details", GraphicsBuildingDetails.High);

	public BoolGameSetting AnisotropicFiltering = new BoolGameSetting("anisotropic-filtering", defaultValue: true);

	private bool OngoingBulkChange = false;

	public GraphicSettings(bool saveOnChange)
		: base("graphics", saveOnChange)
	{
		Register(Antialiasing);
		Register(AmbientOcclusion);
		Register(ShaderQuality);
		Register(BackgroundDetails);
		Register(IslandDetails);
		Register(BuildingDetails);
		Register(ShadowQuality);
		Register(AnisotropicFiltering);
	}

	protected override void OnSettingChanged()
	{
		if (!OngoingBulkChange)
		{
			base.OnSettingChanged();
		}
	}

	public void ApplyGraphicPreset(GraphicPreset preset)
	{
		if (OngoingBulkChange)
		{
			throw new Exception("Two bulk changes not possible in parallel");
		}
		OngoingBulkChange = true;
		try
		{
			Antialiasing.SetValue(preset.AntialiasingQuality);
			AmbientOcclusion.SetValue(preset.AmbientOcclusion);
			ShadowQuality.SetValue(preset.ShadowQuality);
			BackgroundDetails.SetValue(preset.BackgroundDetails);
			IslandDetails.SetValue(preset.IslandDetails);
			ShaderQuality.SetValue(preset.ShaderQuality);
			BuildingDetails.SetValue(preset.BuildingDetails);
			AnisotropicFiltering.SetValue(preset.AnisotropicFiltering);
		}
		catch (Exception ex)
		{
			Debug.LogError("Failed to apply graphics preset: " + ex);
		}
		finally
		{
			OngoingBulkChange = false;
			OnSettingChanged();
		}
	}

	public void Apply(Camera cam, UniversalRenderPipelineAsset rendererAsset)
	{
		if (OngoingBulkChange)
		{
			return;
		}
		Debug.Log("GraphicSettings:: Applying");
		VolumeProfile profile = cam.GetComponent<Volume>()?.profile;
		if (profile != null)
		{
			profile.TryGet<HBAO>(out var hbao);
			if (hbao != null)
			{
				GraphicsQualityUtils.ApplyAmbientOcclusion(hbao, AmbientOcclusion);
			}
		}
		else
		{
			Debug.LogWarning("Camera " + cam.name + " has no Volume component");
		}
		UniversalAdditionalCameraData cameraData = cam.GetComponent<UniversalAdditionalCameraData>();
		GraphicsQualityUtils.ApplyAntialiasing(cameraData, rendererAsset, cam, Antialiasing);
		GraphicsQualityUtils.ApplyShadows(rendererAsset, ShadowQuality);
		QualitySettings.anisotropicFiltering = (AnisotropicFiltering ? UnityEngine.AnisotropicFiltering.Enable : UnityEngine.AnisotropicFiltering.Disable);
	}
}
