using Unity.Mathematics;

public static class ManhattanExtensions
{
	public static uint DistanceManhattan<T>(this T a, T b) where T : IDiscreteCoordinate<T>
	{
		return (uint)(math.abs(a.HorizontalDistance(b)) + math.abs(a.VerticalDistance(b)));
	}
}
