using System;

public class SeededRandomUtils
{
	private Random Rng;

	public SeededRandomUtils(int seed)
	{
		Rng = new Random(seed);
	}

	public int NextInt(int max)
	{
		return Rng.Next(max);
	}

	public float NextRange(float min, float max)
	{
		return (float)Rng.NextDouble() * (max - min) + min;
	}
}
