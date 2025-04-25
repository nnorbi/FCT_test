public struct ShapePart
{
	public static ShapePart EMPTY = new ShapePart(null, null);

	public MetaShapeSubPart Shape;

	public MetaShapeColor Color;

	public bool IsEmpty => Shape == null;

	public bool IsFilled => Shape != null;

	public ShapePart(MetaShapeSubPart shape, MetaShapeColor color)
	{
		Shape = shape;
		Color = color;
	}

	public void Clear()
	{
		Shape = null;
		Color = null;
	}

	public override string ToString()
	{
		return $"{Shape?.Code ?? '-'}{Color?.Code ?? '-'}";
	}
}
