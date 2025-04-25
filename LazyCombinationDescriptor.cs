using System;
using Unity.Jobs;
using UnityEngine;

public struct LazyCombinationDescriptor : IEquatable<LazyCombinationDescriptor>
{
	public Mesh.MeshDataArray MeshDataArray;

	public BoundsAggregationJob BoundsAggregationJob;

	public JobHandle JobHandle;

	public LazyMeshId LazyMeshId;

	public int TotalVertices;

	public int TotalIndices;

	public bool Equals(LazyCombinationDescriptor other)
	{
		return LazyMeshId.Equals(other.LazyMeshId);
	}

	public override bool Equals(object obj)
	{
		return obj is LazyCombinationDescriptor other && Equals(other);
	}

	public override int GetHashCode()
	{
		return LazyMeshId.GetHashCode();
	}
}
