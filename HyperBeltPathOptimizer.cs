using Unity.Collections;
using Unity.Mathematics;

public static class HyperBeltPathOptimizer
{
	private const int MIN_DISTANCE_INCLUSIVE_TO_REPLACE_WITH_TUNNEL = 3;

	public static HyperBelt OptimizeBeltWithTunnels(HyperBelt path)
	{
		HyperBelt optimizedPath = HyperBelt.Empty(Allocator.Temp);
		int i = 0;
		while (i < path.Nodes.Length)
		{
			if (path.Nodes[i].Part == HyperBeltPart.Forward)
			{
				TryReplacingWithATunnel(path, optimizedPath, ref i);
			}
			else
			{
				optimizedPath.Add(path.Nodes[i++]);
			}
		}
		return optimizedPath;
	}

	private static void TryReplacingWithATunnel(HyperBelt path, HyperBelt optimized, ref int baseIndex)
	{
		int maxIndex = math.min(baseIndex + TunnelConstants.TunnelMaxLength, path.Nodes.Length);
		HyperBeltNode start = path.Nodes[baseIndex];
		HyperBeltNode end = start;
		int nextIndex = baseIndex;
		for (int currentIndex = baseIndex; currentIndex < maxIndex && path.Nodes[currentIndex].Part == HyperBeltPart.Forward; currentIndex++)
		{
			end = path.Nodes[currentIndex];
			nextIndex = currentIndex + 1;
		}
		if (end.Position.DistanceManhattan(start.Position) + 1 < 3)
		{
			for (int i = baseIndex; i < nextIndex; i++)
			{
				optimized.Add(path.Nodes[i]);
			}
		}
		else
		{
			optimized.Add(new HyperBeltNode(HyperBeltPart.TunnelSender, start.Position, start.Direction));
			optimized.Add(new HyperBeltNode(HyperBeltPart.TunnelReceiver, end.Position, end.Direction));
		}
		baseIndex = nextIndex;
	}
}
