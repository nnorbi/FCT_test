using UnityEngine;

public static class MaterialPropertyHelpers
{
	public static int SHADER_ID_Alpha = Shader.PropertyToID("_Alpha");

	private static MaterialPropertyBlock Block_Alpha = new MaterialPropertyBlock();

	public static int SHADER_ID_BaseColor = Shader.PropertyToID("_BaseColor");

	private static MaterialPropertyBlock Block_BaseColor = new MaterialPropertyBlock();

	public static MaterialPropertyBlock CreateAlphaBlock(float alpha)
	{
		Block_Alpha.SetFloat(SHADER_ID_Alpha, alpha);
		return Block_Alpha;
	}

	public static MaterialPropertyBlock CreateBaseColorBlock(Color baseColor)
	{
		Block_BaseColor.SetColor(SHADER_ID_BaseColor, baseColor);
		return Block_BaseColor;
	}
}
