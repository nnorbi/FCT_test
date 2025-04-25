public interface IDiscreteCoordinate<T>
{
	T NeighbourChunk(Grid.Direction direction);

	int HorizontalDistance(T other);

	int VerticalDistance(T other);
}
