public struct ShapeLayer
{
	public ShapePart[] Parts;

	public static ShapeLayer Empty(int n)
	{
		ShapePart[] parts = new ShapePart[n];
		for (int i = 0; i < n; i++)
		{
			parts[i] = ShapePart.EMPTY;
		}
		return new ShapeLayer
		{
			Parts = parts
		};
	}

	public ShapeLayer(ShapePart[] parts)
	{
		Parts = parts;
	}

	public ShapeLayer Clone()
	{
		return new ShapeLayer
		{
			Parts = (ShapePart[])Parts.Clone()
		};
	}

	public void Clear()
	{
		for (int i = 0; i < Parts.Length; i++)
		{
			Parts[i] = ShapePart.EMPTY;
		}
	}

	public override string ToString()
	{
		string result = string.Empty;
		ShapePart[] parts = Parts;
		for (int i = 0; i < parts.Length; i++)
		{
			ShapePart shapePart = parts[i];
			result += shapePart.ToString();
		}
		return result;
	}
}
