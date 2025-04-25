using Unity.Collections;

public static class HyperBeltSelfIntersectChecker
{
	public static HyperBelt InvalidateOverlappingNodes(HyperBelt path)
	{
		NativeHashSet<GlobalChunkCoordinate> coordinatesSet = new NativeHashSet<GlobalChunkCoordinate>(0, Allocator.Temp);
		HyperBelt validPath = HyperBelt.Empty(Allocator.Temp);
		for (int i = 0; i < path.Nodes.Length; i++)
		{
			HyperBeltNode node = path.Nodes[i];
			if (coordinatesSet.Contains(node.Position))
			{
				validPath.Add(new HyperBeltNode(HyperBeltPart.Invalid, node.Position, node.Direction));
				continue;
			}
			coordinatesSet.Add(node.Position);
			validPath.Add(node);
		}
		coordinatesSet.Dispose();
		return validPath;
	}
}
