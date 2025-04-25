using System;
using Unity.Mathematics;
using UnityEngine;

public struct DisplayResolution : IEquatable<DisplayResolution>, IComparable<DisplayResolution>
{
	public static DisplayResolution UNINITIALIZED = new DisplayResolution(-1, -1);

	public int Width;

	public int Height;

	public int2 Dimensions => new int2(Width, Height);

	public DisplayResolution(int width, int height)
	{
		Width = width;
		Height = height;
	}

	public DisplayResolution(Resolution baseResolution)
	{
		Width = baseResolution.width;
		Height = baseResolution.height;
	}

	public int CompareTo(DisplayResolution other)
	{
		int widthComparison = Width.CompareTo(other.Width);
		if (widthComparison != 0)
		{
			return widthComparison;
		}
		return Height.CompareTo(other.Height);
	}

	public bool Equals(DisplayResolution other)
	{
		return Width == other.Width && Height == other.Height;
	}

	public override bool Equals(object obj)
	{
		return obj is DisplayResolution other && Equals(other);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Width, Height);
	}

	public override string ToString()
	{
		return $"{Width} x {Height}";
	}
}
