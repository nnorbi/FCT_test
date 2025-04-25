using UnityEngine;
using UnityEngine.Rendering;

public static class MeshPreviewGenerator
{
	public static Texture2D GenerateMeshPreview(Mesh mesh, Material material, int dimensions, float bounds)
	{
		RenderTexture rt = RenderTexture.GetTemporary(dimensions, dimensions, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB, 8, RenderTextureMemoryless.None, VRTextureUsage.None, useDynamicScale: false);
		rt.autoGenerateMips = false;
		CommandBuffer buffer = new CommandBuffer();
		buffer.ClearRenderTarget(clearDepth: true, clearColor: true, new Color(0.02f, 0.025f, 0.03f).WithAlpha(0f));
		buffer.DrawMesh(mesh, Matrix4x4.identity, material, 0, -1, null);
		RenderTexture previous = RenderTexture.active;
		RenderTexture.active = rt;
		Matrix4x4 projection = Matrix4x4.Ortho(0f - bounds, bounds, 0f - bounds, bounds, -100f, 100f);
		GL.LoadProjectionMatrix(GL.GetGPUProjectionMatrix(projection, renderIntoTexture: true));
		GL.modelview = new Matrix4x4(new Vector4(1f, 0f, 0f, 0f), new Vector4(0f, 0f, -1f, 0f), new Vector4(0f, -1f, 0f, 0f), new Vector4(0f, 0f, 0f, 1f));
		Graphics.ExecuteCommandBuffer(buffer);
		buffer.Clear();
		RenderTexture.active = previous;
		Texture2D tex = ConvertRtToTexture(rt);
		RenderTexture.ReleaseTemporary(rt);
		return tex;
	}

	private static Texture2D ConvertRtToTexture(RenderTexture rTex)
	{
		Texture2D tex = new Texture2D(rTex.width, rTex.height, TextureFormat.ARGB32, mipChain: true, linear: false);
		tex.wrapMode = TextureWrapMode.Clamp;
		tex.mipMapBias = -2f;
		tex.anisoLevel = 16;
		tex.filterMode = FilterMode.Trilinear;
		tex.name = rTex.name + "-tex";
		RenderTexture oldRt = RenderTexture.active;
		try
		{
			RenderTexture.active = rTex;
			tex.ReadPixels(new Rect(0f, 0f, rTex.width, rTex.height), 0, 0);
			tex.Apply();
		}
		finally
		{
			RenderTexture.active = oldRt;
		}
		return tex;
	}
}
