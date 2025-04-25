using System;

public static class ShapeResourceSourceUtils
{
	public static char SelectCode(ShapeLayer shapeLayer, int index)
	{
		int indexDistance = 0;
		bool positive = false;
		do
		{
			index += (positive ? indexDistance : (shapeLayer.Parts.Length - indexDistance));
			index %= shapeLayer.Parts.Length;
			if ((bool)shapeLayer.Parts[index].Shape)
			{
				return shapeLayer.Parts[index].Shape.Code;
			}
			positive = !positive;
		}
		while (++indexDistance < shapeLayer.Parts.Length);
		throw new InvalidOperationException($"Could not valid shape part in shape layer '{shapeLayer}'");
	}
}
