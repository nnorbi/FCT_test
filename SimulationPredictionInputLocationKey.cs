using System;

public struct SimulationPredictionInputLocationKey : IEquatable<SimulationPredictionInputLocationKey>
{
	public GlobalTileCoordinate DestinationTile;

	public Grid.Direction Direction;

	public SimulationPredictionInputLocationKey(GlobalTileCoordinate destinationTile, Grid.Direction direction)
	{
		DestinationTile = destinationTile;
		Direction = direction;
	}

	public bool Equals(SimulationPredictionInputLocationKey other)
	{
		return DestinationTile.Equals(other.DestinationTile) && Direction == other.Direction;
	}

	public override bool Equals(object obj)
	{
		return obj is SimulationPredictionInputLocationKey other && Equals(other);
	}

	public override int GetHashCode()
	{
		int hash = 17;
		hash = hash * 31 + DestinationTile.GetHashCode();
		return hash * 31 + (int)Direction;
	}

	public override string ToString()
	{
		return $"{DestinationTile} in direction {Direction.FormatAsDirection()}";
	}
}
