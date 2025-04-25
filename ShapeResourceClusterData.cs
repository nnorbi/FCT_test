using System.Collections.Generic;
using System.Linq;

public class ShapeResourceClusterData : IResourceSourceData
{
	public readonly ShapeResourceSourceData[] ShapeResources;

	public readonly GlobalChunkCoordinate Center_GC;

	public ShapeResourceClusterData(IEnumerable<ShapeResourceSourceData> shapeResources, GlobalChunkCoordinate center_GC)
	{
		ShapeResources = shapeResources.ToArray();
		Center_GC = center_GC;
	}

	public ResourceSource Create()
	{
		return new ShapeResourceSource(Center_GC, ShapeResources.Select((ShapeResourceSourceData r) => r.Offset_LC).ToArray(), ShapeResources.Select((ShapeResourceSourceData r) => Singleton<GameCore>.G.Shapes.GetDefinitionByHash(r.Definition)).ToArray());
	}
}
