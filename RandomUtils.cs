using System;
using System.Collections.Generic;

public static class RandomUtils
{
	private static Random RNG = new Random();

	public static int NextInt(int max)
	{
		return RNG.Next(max);
	}

	public static float NextRange(float min, float max)
	{
		return (float)RNG.NextDouble() * (max - min) + min;
	}

	public static void Shuffle<T>(this IList<T> list)
	{
		int count = list.Count;
		while (count > 1)
		{
			count--;
			int index = RNG.Next(count + 1);
			int index2 = index;
			int index3 = count;
			T val = list[count];
			T val2 = list[index];
			T val3 = (list[index2] = val);
			val3 = (list[index3] = val2);
		}
	}
}
