using System.Linq;

public static class SpaceThemeShapeAsteroid
{
	public static void GenerateShapeAsteroidMesh(ShapeResourceSource source, IShapeAsteroidBuilder builder)
	{
		for (int i = 0; i < source.Tiles_LC.Length; i++)
		{
			ChunkDirection position_LC = source.Tiles_LC[i];
			ShapeDefinition definition = source.Definitions[i];
			builder.PushResource(position_LC, definition);
			bool isNorthEmpty = source.Tiles_LC.All((ChunkDirection other) => other != position_LC + ChunkDirection.North);
			bool isNorthEastEmpty = source.Tiles_LC.All((ChunkDirection other) => other != position_LC + ChunkDirection.North + ChunkDirection.East);
			bool isEastEmpty = source.Tiles_LC.All((ChunkDirection other) => other != position_LC + ChunkDirection.East);
			bool isSouthEastEmpty = source.Tiles_LC.All((ChunkDirection other) => other != position_LC + ChunkDirection.South + ChunkDirection.East);
			bool isSouthEmpty = source.Tiles_LC.All((ChunkDirection other) => other != position_LC + ChunkDirection.South);
			bool isSouthWestEmpty = source.Tiles_LC.All((ChunkDirection other) => other != position_LC + ChunkDirection.South + ChunkDirection.West);
			bool isWestEmpty = source.Tiles_LC.All((ChunkDirection other) => other != position_LC + ChunkDirection.West);
			bool isNorthWestEmpty = source.Tiles_LC.All((ChunkDirection other) => other != position_LC + ChunkDirection.North + ChunkDirection.West);
			builder.PushPlatform();
			if (!isSouthEmpty)
			{
				builder.PushFillerEdge(Grid.Direction.Bottom);
			}
			if (!isEastEmpty)
			{
				builder.PushFillerEdge(Grid.Direction.Right);
			}
			if (!isSouthEmpty && !isSouthEastEmpty && !isEastEmpty)
			{
				builder.PushFillerCrossing();
			}
			if (isNorthEmpty && isWestEmpty)
			{
				builder.PushPart(OutlineCardinal.NorthWest, ShapeAsteroidDecorationPart.Convex, Grid.Direction.Top);
			}
			else if (isNorthEmpty)
			{
				builder.PushPart(OutlineCardinal.NorthWest, (!isNorthWestEmpty) ? ShapeAsteroidDecorationPart.ConcaveRight : ShapeAsteroidDecorationPart.StraightRight, Grid.Direction.Top);
			}
			else if (isWestEmpty)
			{
				builder.PushPart(OutlineCardinal.NorthWest, isNorthWestEmpty ? ShapeAsteroidDecorationPart.StraightLeft : ShapeAsteroidDecorationPart.ConcaveLeft, Grid.Direction.Left);
			}
			if (isNorthEmpty && isEastEmpty)
			{
				builder.PushPart(OutlineCardinal.NorthEast, ShapeAsteroidDecorationPart.Convex);
			}
			else if (isNorthEmpty)
			{
				builder.PushPart(OutlineCardinal.NorthEast, isNorthEastEmpty ? ShapeAsteroidDecorationPart.StraightLeft : ShapeAsteroidDecorationPart.ConcaveLeft, Grid.Direction.Top);
			}
			else if (isEastEmpty)
			{
				builder.PushPart(OutlineCardinal.NorthEast, (!isNorthEastEmpty) ? ShapeAsteroidDecorationPart.ConcaveRight : ShapeAsteroidDecorationPart.StraightRight);
			}
			if (isSouthEmpty && isEastEmpty)
			{
				builder.PushPart(OutlineCardinal.SouthEast, ShapeAsteroidDecorationPart.Convex, Grid.Direction.Bottom);
			}
			else if (isSouthEmpty)
			{
				builder.PushPart(OutlineCardinal.SouthEast, (!isSouthEastEmpty) ? ShapeAsteroidDecorationPart.ConcaveRight : ShapeAsteroidDecorationPart.StraightRight, Grid.Direction.Bottom);
			}
			else if (isEastEmpty)
			{
				builder.PushPart(OutlineCardinal.SouthEast, isSouthEastEmpty ? ShapeAsteroidDecorationPart.StraightLeft : ShapeAsteroidDecorationPart.ConcaveLeft);
			}
			if (isSouthEmpty && isWestEmpty)
			{
				builder.PushPart(OutlineCardinal.SouthWest, ShapeAsteroidDecorationPart.Convex, Grid.Direction.Left);
			}
			else if (isSouthEmpty)
			{
				builder.PushPart(OutlineCardinal.SouthWest, isSouthWestEmpty ? ShapeAsteroidDecorationPart.StraightLeft : ShapeAsteroidDecorationPart.ConcaveLeft, Grid.Direction.Bottom);
			}
			else if (isWestEmpty)
			{
				builder.PushPart(OutlineCardinal.SouthWest, (!isSouthWestEmpty) ? ShapeAsteroidDecorationPart.ConcaveRight : ShapeAsteroidDecorationPart.StraightRight, Grid.Direction.Left);
			}
		}
	}
}
