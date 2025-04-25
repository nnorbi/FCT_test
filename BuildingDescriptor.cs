using System;

public readonly struct BuildingDescriptor : IEquatable<BuildingDescriptor>
{
	public readonly MetaBuildingInternalVariant InternalVariant;

	public readonly Island Island;

	public readonly IslandTileCoordinate BaseTile_I;

	public readonly Grid.Direction Rotation_G;

	public GlobalTileCoordinate GlobalTileCoordinate { get; }

	public BuildingDescriptor(MapEntity entity)
		: this(entity.InternalVariant, entity.Island, entity.Tile_I, entity.Rotation_G)
	{
	}

	public BuildingDescriptor(MetaBuildingInternalVariant internalVariant, Island buildingIsland, IslandTileCoordinate baseTile_I, Grid.Direction rotationG)
	{
		InternalVariant = internalVariant;
		Island = buildingIsland;
		BaseTile_I = baseTile_I;
		Rotation_G = rotationG;
		GlobalTileCoordinate = baseTile_I.To_G(buildingIsland);
	}

	public bool Equals(BuildingDescriptor other)
	{
		return GlobalTileCoordinate.Equals(other.GlobalTileCoordinate) && Rotation_G == other.Rotation_G && InternalVariant == other.InternalVariant;
	}

	public IslandTileCoordinate L_To_I(TileDirection tile_L)
	{
		return tile_L.To_I(Rotation_G, in BaseTile_I);
	}

	public GlobalTileCoordinate L_To_G(TileDirection tile_L)
	{
		return tile_L.To_I(Rotation_G, in BaseTile_I).To_G(Island);
	}

	public override bool Equals(object obj)
	{
		return obj is BuildingDescriptor other && Equals(other);
	}

	public override int GetHashCode()
	{
		return GlobalTileCoordinate.GetHashCode();
	}

	public override string ToString()
	{
		return $"{InternalVariant.Implementation.ClassID} at {BaseTile_I.To_G(Island)} with rotation {Rotation_G.FormatAsRotation()}";
	}

	public bool TryGetDescribedEntity<T>(out T entity) where T : MapEntity
	{
		entity = Island.GetEntity_I(in BaseTile_I) as T;
		return entity != null && entity.Rotation_G == Rotation_G && entity.InternalVariant == InternalVariant;
	}
}
