using System;

public struct HyperBeltNode : IEquatable<HyperBeltNode>
{
	public HyperBeltPart Part;

	public GlobalChunkCoordinate Position;

	public Grid.Direction Direction;

	public HyperBeltNode(HyperBeltPart part, GlobalChunkCoordinate position, Grid.Direction direction)
	{
		Position = position;
		Direction = direction;
		Part = part;
	}

	public bool Equals(HyperBeltNode other)
	{
		return Part == other.Part && Position.Equals(other.Position) && Direction == other.Direction;
	}

	public override bool Equals(object obj)
	{
		return obj is HyperBeltNode other && Equals(other);
	}

	public override int GetHashCode()
	{
		int hash = 17;
		hash = hash * 31 + Part.GetHashCode();
		hash = hash * 31 + Position.GetHashCode();
		return hash * 31 + Direction.GetHashCode();
	}
}
