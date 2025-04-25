using Unity.Mathematics;

public static class MathematicsRandomUtils
{
	private const int SOME_PRIME_NUMBER = 3337333;

	public static Random SafeRandom<T>(T seedGenerator) where T : struct
	{
		uint u = (uint)seedGenerator.GetHashCode();
		if (u == uint.MaxValue)
		{
			u = 3337333u;
		}
		return Random.CreateFromIndex(u);
	}
}
