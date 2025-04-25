using System;

public struct ChunkMaterialTuple : IEquatable<ChunkMaterialTuple>
{
	public readonly BackgroundChunkData BackgroundChunkData;

	public readonly int MaterialId;

	public ChunkMaterialTuple(BackgroundChunkData backgroundChunkData, int materialId)
	{
		BackgroundChunkData = backgroundChunkData;
		MaterialId = materialId;
	}

	public bool Equals(ChunkMaterialTuple other)
	{
		return BackgroundChunkData.Equals(other.BackgroundChunkData) && MaterialId == other.MaterialId;
	}

	public override bool Equals(object obj)
	{
		return obj is ChunkMaterialTuple other && Equals(other);
	}

	public override int GetHashCode()
	{
		int hash = 17;
		hash = hash * 31 + BackgroundChunkData.GetHashCode();
		return hash * 31 + MaterialId.GetHashCode();
	}
}
