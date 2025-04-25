using System;

public class ConsistentRandom
{
	private const int MBIG = int.MaxValue;

	private const int MSEED = 161803398;

	private int inext;

	private int inextp;

	private int[] SeedArray = new int[56];

	public static uint StringToHash(string seed)
	{
		uint hash = 523423u;
		for (int i = 0; i < seed.Length; i++)
		{
			hash += seed[i];
			hash += hash << 10;
			hash ^= hash >> 6;
		}
		hash += hash << 3;
		hash ^= hash >> 11;
		return hash + (hash << 15);
	}

	public ConsistentRandom(int seed)
	{
		int mj = 161803398 - ((seed == int.MinValue) ? int.MaxValue : Math.Abs(seed));
		SeedArray[55] = mj;
		int mk = 1;
		for (int i = 1; i < 55; i++)
		{
			int ii = 21 * i % 55;
			SeedArray[ii] = mk;
			mk = mj - mk;
			if (mk < 0)
			{
				mk += int.MaxValue;
			}
			mj = SeedArray[ii];
		}
		for (int k = 1; k < 5; k++)
		{
			for (int j = 1; j < 56; j++)
			{
				SeedArray[j] -= SeedArray[1 + (j + 30) % 55];
				if (SeedArray[j] < 0)
				{
					SeedArray[j] += int.MaxValue;
				}
			}
		}
		inext = 0;
		inextp = 21;
	}

	public ConsistentRandom(string seed)
		: this((int)StringToHash(seed))
	{
	}

	public double NextDouble()
	{
		return (double)NextInt() * 4.656612875245797E-10;
	}

	public float NextFloat()
	{
		return (float)((double)NextInt() * 4.656612875245797E-10);
	}

	public int NextInt()
	{
		int locINext = inext;
		int locINextp = inextp;
		if (++locINext >= 56)
		{
			locINext = 1;
		}
		if (++locINextp >= 56)
		{
			locINextp = 1;
		}
		int retVal = SeedArray[locINext] - SeedArray[locINextp];
		if (retVal == int.MaxValue)
		{
			retVal--;
		}
		if (retVal < 0)
		{
			retVal += int.MaxValue;
		}
		SeedArray[locINext] = retVal;
		inext = locINext;
		inextp = locINextp;
		return retVal;
	}

	public int Next(int minValue, int maxValue)
	{
		if (minValue > maxValue)
		{
			throw new ArgumentOutOfRangeException("minValue");
		}
		int range = maxValue - minValue;
		return NextInt() % range + minValue;
	}

	public T Choice<T>(T[] array)
	{
		if (array == null || array.Length == 0)
		{
			return default(T);
		}
		int index = Next(0, array.Length);
		return array[index];
	}
}
