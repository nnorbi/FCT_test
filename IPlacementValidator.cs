public interface IPlacementValidator<in T>
{
	bool CanPlace(T hyperBelt);
}
