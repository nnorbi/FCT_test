using Unity.Mathematics;
using UnityEngine;

public static class ColorUtils
{
	public static Color ColorFromRGB255(int r, int g, int b, int a = 255)
	{
		return new Color((float)r / 255f, (float)g / 255f, (float)b / 255f, (float)a / 255f);
	}

	public static Color WithAlpha(this Color baseColor, float a = 1f)
	{
		return new Color(baseColor.r, baseColor.g, baseColor.b, a);
	}

	public static Color ColorFromSRGB255(int r, int g, int b, int a = 255)
	{
		return new Color(SRGB_To_Linear((float)r / 255f), SRGB_To_Linear((float)g / 255f), SRGB_To_Linear((float)b / 255f), (float)a / 255f);
	}

	public static float SRGB_To_Linear(float value)
	{
		if (value <= 0.04045f)
		{
			return value / 12.92f;
		}
		return math.pow((value + 0.055f) / 1.055f, 2.4f);
	}

	public static float Linear_To_SRGB(float value)
	{
		return (value <= 0.0031308f) ? (value * 12.92f) : (1.055f * math.pow(value, 5f / 12f) - 0.055f);
	}

	public static Color SRGB_To_Linear(Color v)
	{
		return new Color(SRGB_To_Linear(v.r), SRGB_To_Linear(v.g), SRGB_To_Linear(v.b), 1f);
	}
}
