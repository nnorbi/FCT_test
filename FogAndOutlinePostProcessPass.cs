using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[Serializable]
public class FogAndOutlinePostProcessPass : ScriptableRenderPass
{
	protected int MSAASamples;

	protected Material FogAndOutline;

	private RTHandle CameraColorTarget;

	public FogAndOutlinePostProcessPass(Material andOutline)
	{
		base.renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
		FogAndOutline = andOutline;
	}

	public void Setup(in RenderingData renderingData, RTHandle cameraColorTarget)
	{
		CameraColorTarget = cameraColorTarget;
		RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
		MSAASamples = descriptor.msaaSamples;
	}

	public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
	{
		ConfigureTarget(CameraColorTarget);
	}

	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		if (renderingData.cameraData.isSceneViewCamera || !Singleton<GameCore>.HasInstance || !Singleton<GameCore>.G.Initialized)
		{
			return;
		}
		Camera mainCam = Singleton<GameCore>.G.LocalPlayer.Viewport.MainCamera;
		if (renderingData.cameraData.camera != mainCam)
		{
			return;
		}
		CommandBuffer cmd = CommandBufferPool.Get("Custom Post Processing");
		cmd.Clear();
		VolumeStack stack = VolumeManager.instance.stack;
		FogAndOutline customEffect = stack.GetComponent<FogAndOutline>();
		if (customEffect.IsActive())
		{
			cmd.SetGlobalFloat(Shader.PropertyToID("_OutlineFactor"), customEffect.outlineFactor.value);
			Matrix4x4 matrixCameraToWorld = mainCam.cameraToWorldMatrix;
			Matrix4x4 matrixProjectionInverse = GL.GetGPUProjectionMatrix(mainCam.projectionMatrix, renderIntoTexture: false).inverse;
			Matrix4x4 matrixHClipToWorld = matrixCameraToWorld * matrixProjectionInverse;
			cmd.SetGlobalMatrix(Shader.PropertyToID("_CameraIVP"), matrixHClipToWorld);
			float near = mainCam.nearClipPlane;
			float far = mainCam.farClipPlane;
			float4 zBufferParams = new float4(near, far, (1f - far / near) / far, far / near / far);
			if (SystemInfo.usesReversedZBuffer)
			{
				zBufferParams = new float4(near, far, (-1f + far / near) / far, 1f / far);
			}
			cmd.SetGlobalVector(Shader.PropertyToID("_MainZBufferP"), zBufferParams);
			cmd.SetGlobalFloat(Shader.PropertyToID("_Orthographic"), mainCam.orthographic ? 1f : 0f);
			switch (MSAASamples)
			{
			default:
				cmd.EnableShaderKeyword("MSAA_NO");
				cmd.DisableShaderKeyword("MSAA_2");
				cmd.DisableShaderKeyword("MSAA_4");
				cmd.DisableShaderKeyword("MSAA_8");
				cmd.DisableShaderKeyword("MSAA_16");
				break;
			case 2:
				cmd.DisableShaderKeyword("MSAA_NO");
				cmd.EnableShaderKeyword("MSAA_2");
				cmd.DisableShaderKeyword("MSAA_4");
				cmd.DisableShaderKeyword("MSAA_8");
				cmd.DisableShaderKeyword("MSAA_16");
				break;
			case 4:
				cmd.DisableShaderKeyword("MSAA_NO");
				cmd.DisableShaderKeyword("MSAA_2");
				cmd.EnableShaderKeyword("MSAA_4");
				cmd.DisableShaderKeyword("MSAA_8");
				cmd.DisableShaderKeyword("MSAA_16");
				break;
			case 8:
				cmd.DisableShaderKeyword("MSAA_NO");
				cmd.DisableShaderKeyword("MSAA_2");
				cmd.DisableShaderKeyword("MSAA_4");
				cmd.EnableShaderKeyword("MSAA_8");
				cmd.DisableShaderKeyword("MSAA_16");
				break;
			case 16:
				cmd.DisableShaderKeyword("MSAA_NO");
				cmd.DisableShaderKeyword("MSAA_2");
				cmd.DisableShaderKeyword("MSAA_4");
				cmd.DisableShaderKeyword("MSAA_8");
				cmd.EnableShaderKeyword("MSAA_16");
				break;
			}
			using (new ProfilingScope(cmd, base.profilingSampler))
			{
				Blitter.BlitCameraTexture(cmd, CameraColorTarget, CameraColorTarget, FogAndOutline, 0);
			}
			context.ExecuteCommandBuffer(cmd);
			cmd.Clear();
			CommandBufferPool.Release(cmd);
		}
	}

	public override void OnCameraCleanup(CommandBuffer cmd)
	{
		CameraColorTarget = null;
	}
}
