public class HUBCenterIslandChunk : IslandChunk
{
	public class PlayingfieldModificator : IslandChunkPlayingfieldModificator
	{
		public override void ApplyModifications(MetaIslandChunk config)
		{
			for (int i = 0; i < config.EdgeTypes.Length; i++)
			{
				config.EdgeTypes[i] = MetaIslandChunkBase.EdgeType.Expand;
			}
			for (int x = 0; x < 20; x++)
			{
				for (int y = 0; y < 20; y++)
				{
					bool buildable = (y > 2 && y < 17 && (x == 0 || x == 19)) || (x > 2 && x < 17 && (y == 0 || y == 19));
					int index = MetaIslandChunk.GetBuildableLookupIndex_L(new ChunkTileCoordinate(x, y, 0));
					config.TileBuildableFlags_L[MetaIslandChunk.GetBuildableLookupIndex_L(new ChunkTileCoordinate(x, y, 0))] = buildable;
					if (!buildable)
					{
						continue;
					}
					Grid.Direction notchDirection = Grid.Direction.Right;
					switch (x)
					{
					case 0:
						notchDirection = Grid.Direction.Right;
						break;
					case 19:
						notchDirection = Grid.Direction.Left;
						break;
					default:
						switch (y)
						{
						case 0:
							notchDirection = Grid.Direction.Bottom;
							break;
						case 19:
							notchDirection = Grid.Direction.Top;
							break;
						}
						break;
					}
					config.TileNotchFlags_L[index] = notchDirection;
				}
			}
		}
	}

	public HubEntity Hub;

	public HUBCenterIslandChunk(Island island, MetaIslandChunk chunkConfig)
		: base(island, chunkConfig)
	{
	}

	protected override bool Draw_NeedsCustomPlayingfieldMesh()
	{
		return true;
	}

	public override void AddEntity(MapEntity entity)
	{
		if (entity is HubEntity hub)
		{
			Hub = hub;
		}
		base.AddEntity(entity);
	}
}
