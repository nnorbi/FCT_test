using System;
using System.Reflection;
using HorizonBasedAmbientOcclusion.Universal;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public static class GraphicsQualityUtils
{
	public static void ApplyAmbientOcclusion(HBAO hbao, GraphicsAOQuality quality)
	{
		hbao.multiBounceInfluence.Override(0.2f);
		hbao.useMultiBounce.Override(x: true);
		hbao.colorBleedingEnabled.Override(x: false);
		hbao.resolution.Override(HBAO.Resolution.Full);
		hbao.bias.Override(0.03f);
		hbao.maxDistance.Override(1000f);
		switch (quality)
		{
		default:
			hbao.active = false;
			break;
		case GraphicsAOQuality.Low:
			hbao.active = true;
			hbao.quality.Override(HBAO.Quality.Lowest);
			hbao.radius.Override(4f);
			hbao.maxRadiusPixels.Override(20f);
			hbao.intensity.Override(0.9f);
			hbao.blurType.Override(HBAO.BlurType.Medium);
			break;
		case GraphicsAOQuality.Medium:
			hbao.active = true;
			hbao.quality.Override(HBAO.Quality.Low);
			hbao.radius.Override(4f);
			hbao.maxRadiusPixels.Override(30f);
			hbao.intensity.Override(0.9f);
			hbao.blurType.Override(HBAO.BlurType.Medium);
			break;
		case GraphicsAOQuality.High:
			hbao.active = true;
			hbao.quality.Override(HBAO.Quality.Medium);
			hbao.radius.Override(5f);
			hbao.maxRadiusPixels.Override(35f);
			hbao.intensity.Override(0.9f);
			hbao.blurType.Override(HBAO.BlurType.Medium);
			break;
		case GraphicsAOQuality.Extreme:
			hbao.active = true;
			hbao.quality.Override(HBAO.Quality.Highest);
			hbao.radius.Override(5f);
			hbao.maxRadiusPixels.Override(50f);
			hbao.intensity.Override(0.9f);
			hbao.blurType.Override(HBAO.BlurType.Medium);
			break;
		}
	}

	public static void ApplyAntialiasing(UniversalAdditionalCameraData cameraData, UniversalRenderPipelineAsset rendererAsset, Camera camera, GraphicsAntialiasingQuality quality)
	{
		Debug.Log("Apply Antialiasing Quality: " + quality);
		switch (quality)
		{
		default:
			cameraData.antialiasing = AntialiasingMode.None;
			rendererAsset.msaaSampleCount = 1;
			camera.allowMSAA = false;
			break;
		case GraphicsAntialiasingQuality.Smaa:
			cameraData.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
			cameraData.antialiasingQuality = AntialiasingQuality.High;
			rendererAsset.msaaSampleCount = 1;
			camera.allowMSAA = false;
			break;
		case GraphicsAntialiasingQuality.MSAA2:
			cameraData.antialiasing = AntialiasingMode.None;
			cameraData.antialiasingQuality = AntialiasingQuality.High;
			rendererAsset.msaaSampleCount = 2;
			camera.allowMSAA = true;
			break;
		case GraphicsAntialiasingQuality.MSAA4:
			cameraData.antialiasing = AntialiasingMode.None;
			cameraData.antialiasingQuality = AntialiasingQuality.High;
			rendererAsset.msaaSampleCount = 4;
			camera.allowMSAA = true;
			break;
		}
	}

	public static float GetShadowDistance(GraphicsShadowQuality quality)
	{
		if (1 == 0)
		{
		}
		int num = quality switch
		{
			GraphicsShadowQuality.Off => 0, 
			GraphicsShadowQuality.Low => 50, 
			GraphicsShadowQuality.Medium => 70, 
			GraphicsShadowQuality.High => 140, 
			GraphicsShadowQuality.Extreme => 170, 
			_ => throw new ArgumentOutOfRangeException("quality", quality, null), 
		};
		if (1 == 0)
		{
		}
		return num;
	}

	public static void ApplyShadows(UniversalRenderPipelineAsset rendererAsset, GraphicsShadowQuality quality)
	{
		FieldInfo supportsMainLightShadows = rendererAsset.GetType().GetField("m_MainLightShadowsSupported", BindingFlags.Instance | BindingFlags.NonPublic);
		FieldInfo shadowResolution = rendererAsset.GetType().GetField("m_MainLightShadowmapResolution", BindingFlags.Instance | BindingFlags.NonPublic);
		FieldInfo softShadows = rendererAsset.GetType().GetField("m_SoftShadowsSupported", BindingFlags.Instance | BindingFlags.NonPublic);
		if (quality == GraphicsShadowQuality.Off)
		{
			shadowResolution.SetValue(rendererAsset, UnityEngine.Rendering.Universal.ShadowResolution._256);
			softShadows.SetValue(rendererAsset, false);
			supportsMainLightShadows.SetValue(rendererAsset, false);
			rendererAsset.shadowDistance = 0f;
			return;
		}
		supportsMainLightShadows.SetValue(rendererAsset, true);
		softShadows.SetValue(rendererAsset, true);
		switch (quality)
		{
		case GraphicsShadowQuality.Low:
			shadowResolution.SetValue(rendererAsset, UnityEngine.Rendering.Universal.ShadowResolution._512);
			rendererAsset.shadowCascadeCount = 1;
			break;
		case GraphicsShadowQuality.Medium:
			shadowResolution.SetValue(rendererAsset, UnityEngine.Rendering.Universal.ShadowResolution._2048);
			rendererAsset.shadowCascadeCount = 2;
			break;
		case GraphicsShadowQuality.High:
			shadowResolution.SetValue(rendererAsset, UnityEngine.Rendering.Universal.ShadowResolution._2048);
			rendererAsset.shadowCascadeCount = 3;
			break;
		case GraphicsShadowQuality.Extreme:
			shadowResolution.SetValue(rendererAsset, UnityEngine.Rendering.Universal.ShadowResolution._4096);
			rendererAsset.shadowCascadeCount = 4;
			break;
		}
		float chunkDiameterMax3D = math.length(new float3(9.5f, 9.5f, Singleton<GameCore>.G.Mode.MaxLayer + 1));
		float additionalPadding = 2f;
		if (quality >= GraphicsShadowQuality.High)
		{
			additionalPadding += 8f;
		}
		rendererAsset.shadowDistance = GetShadowDistance(quality) - chunkDiameterMax3D - additionalPadding;
	}
}
