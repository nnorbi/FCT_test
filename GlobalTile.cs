using System;

public struct GlobalTile : IEquatable<GlobalTile>
{
	public GlobalTileCoordinate Tile_G;

	public IslandTileCoordinate Tile_I;

	public Island Island;

	public bool Equals(GlobalTile other)
	{
		return Tile_G.Equals(other.Tile_G) && Tile_I.Equals(other.Tile_I) && object.Equals(Island, other.Island);
	}

	public override bool Equals(object obj)
	{
		return obj is GlobalTile other && Equals(other);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Tile_G, Tile_I, Island);
	}
}
