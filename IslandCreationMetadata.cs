public class IslandCreationMetadata
{
	public MetaIslandLayout Layout;

	public Grid.Direction LayoutRotation;

	public IslandCreationMetadata()
	{
	}

	public IslandCreationMetadata(IslandCreationMetadata other)
	{
		Layout = other.Layout;
		LayoutRotation = other.LayoutRotation;
	}
}
