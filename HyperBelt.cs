using System;
using Unity.Collections;

public struct HyperBelt : IDisposable
{
	private NativeList<HyperBeltNode> InternalNodes;

	public NativeSlice<HyperBeltNode> Nodes => InternalNodes.AsArray();

	public static HyperBelt Empty(Allocator allocator)
	{
		return new HyperBelt
		{
			InternalNodes = new NativeList<HyperBeltNode>(allocator)
		};
	}

	public void Dispose()
	{
		InternalNodes.Dispose();
	}

	public void Add(HyperBeltNode node)
	{
		InternalNodes.Add(in node);
	}
}
