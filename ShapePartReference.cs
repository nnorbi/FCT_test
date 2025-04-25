public class ShapePartReference
{
	public int LayerIndex;

	public int PartIndex;

	public ShapePart Part;

	public ShapePartReference()
	{
	}

	public ShapePartReference(ShapePartReference other)
	{
		LayerIndex = other.LayerIndex;
		PartIndex = other.PartIndex;
		Part = other.Part;
	}

	public override string ToString()
	{
		return "{Layer " + LayerIndex + " Part " + PartIndex + " Shape " + Part.Shape.Code + "/" + Part.Color?.Code + "}";
	}
}
