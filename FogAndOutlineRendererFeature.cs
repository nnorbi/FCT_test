using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[Serializable]
public class FogAndOutlineRendererFeature : ScriptableRendererFeature
{
	public Material Material;

	private FogAndOutlinePostProcessPass Pass;

	public override void Create()
	{
		Pass = new FogAndOutlinePostProcessPass(Material);
	}

	public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
	{
		if (renderingData.cameraData.cameraType == CameraType.Game)
		{
			renderer.EnqueuePass(Pass);
		}
	}

	public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
	{
		if (renderingData.cameraData.cameraType == CameraType.Game)
		{
			Pass.Setup(in renderingData, renderer.cameraColorTargetHandle);
			Pass.ConfigureInput(ScriptableRenderPassInput.Depth | ScriptableRenderPassInput.Color);
		}
	}
}
