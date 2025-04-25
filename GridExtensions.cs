using System;

public static class GridExtensions
{
	public static string FormatAsDirection(this Grid.Direction direction)
	{
		if (1 == 0)
		{
		}
		string result = direction switch
		{
			Grid.Direction.Right => "Right", 
			Grid.Direction.Bottom => "Bottom", 
			Grid.Direction.Left => "Left", 
			Grid.Direction.Top => "Top", 
			_ => throw new ArgumentOutOfRangeException("direction", direction, null), 
		};
		if (1 == 0)
		{
		}
		return result;
	}

	public static string FormatAsRotation(this Grid.Direction direction)
	{
		if (1 == 0)
		{
		}
		string result = direction switch
		{
			Grid.Direction.Right => "NoRotate", 
			Grid.Direction.Bottom => "RotateCW", 
			Grid.Direction.Left => "Rotate180", 
			Grid.Direction.Top => "RotateCCW", 
			_ => throw new ArgumentOutOfRangeException("direction", direction, null), 
		};
		if (1 == 0)
		{
		}
		return result;
	}
}
