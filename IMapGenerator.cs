using System.Collections.Generic;

public interface IMapGenerator
{
	IEnumerable<IResourceSourceData> Generate(SuperChunkCoordinate origin_SC);
}
