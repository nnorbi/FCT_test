using System.Runtime.CompilerServices;
using Unity.Mathematics;

public static class FastMath
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float Min(float a, float b)
	{
		return (a < b) ? a : b;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float Max(float a, float b)
	{
		return (a > b) ? a : b;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int Min(int a, int b)
	{
		return (a < b) ? a : b;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int Max(int a, int b)
	{
		return (a > b) ? a : b;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int SafeMod(int x, int m)
	{
		int r = x % m;
		return (r < 0) ? (r + m) : r;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float SafeMod(float x, float m)
	{
		return x - m * math.floor(x / m);
	}
}
